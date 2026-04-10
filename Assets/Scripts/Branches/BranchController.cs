using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BranchController : MonoBehaviour
{
    [Header("Shape Layout")]
    [SerializeField] private Vector2 branchBodySize = new Vector2(0.25f, 1f);
    [SerializeField] private Vector2 previewFlowerSize = new Vector2(0.7f, 0.6f);
    [SerializeField] private Vector3 bodyLocalPosition = new Vector3(0f, -0.35f, 0f);
    [SerializeField] private Vector3 markerLocalPosition = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private Vector3 previewLocalPosition = new Vector3(0f, 1.05f, 0f);
    [SerializeField] private Vector3 growthBarLocalPosition = new Vector3(0f, -1.1f, 0f);
    [SerializeField] private Vector2 growthBarSize = new Vector2(0.9f, 0.12f);

    private Sprite budSprite;
    private Sprite grownSprite;
    private Sprite initialBranchSprite;
    private Sprite cutSourceBranchSprite;
    private Sprite graftPreviewBranchSprite;
    private Sprite graftBandagedBranchSprite;
    private Sprite matureBranchSprite;
    private Sprite scionDragSprite;
    private Vector3 branchSpriteScale = Vector3.one;
    private Vector3 flowerVisualScale = Vector3.one;
    private Vector3 scionDragScale = Vector3.one;
    private float sourceRecoveryDurationSeconds = 3.5f;
    private bool isGraftPreviewActive;
    private Coroutine sourceRecoveryRoutine;

    private SpriteRenderer branchSpriteRenderer;
    private SpriteRenderer colorMarkerRenderer;
    private SpriteRenderer previewFlowerRenderer;
    private SpriteRenderer matureFlowerRenderer;
    private GameObject growthBarRoot;
    private GameObject growthBarFill;
    private BoxCollider2D interactionCollider;
    private GrowthSystem growthSystem;

    public BranchData Data { get; private set; }
    public FlowerColor CurrentColor => Data.CurrentColor;
    public bool IsPreviewVisible => previewFlowerRenderer != null && previewFlowerRenderer.gameObject.activeSelf;
    public int DamageLevel => Data.DamageLevel;
    public Sprite ScionDragSprite => scionDragSprite;
    public Vector3 ScionDragScale => scionDragScale;

    public void Initialize(BranchData data, BranchVisualSettings visualSettings)
    {
        Data = data;
        branchBodySize = visualSettings.BranchBodySize;
        previewFlowerSize = visualSettings.PreviewFlowerSize;
        previewLocalPosition = visualSettings.PreviewLocalPosition;
        branchSpriteScale = visualSettings.BranchSpriteScale == Vector3.zero ? Vector3.one : visualSettings.BranchSpriteScale;
        budSprite = visualSettings.BudSprite;
        grownSprite = visualSettings.GrownSprite;
        initialBranchSprite = visualSettings.InitialBranchSprite;
        cutSourceBranchSprite = visualSettings.CutSourceBranchSprite;
        graftPreviewBranchSprite = visualSettings.GraftPreviewBranchSprite;
        graftBandagedBranchSprite = visualSettings.GraftBandagedBranchSprite;
        matureBranchSprite = visualSettings.MatureBranchSprite;
        scionDragSprite = visualSettings.ScionDragSprite;
        flowerVisualScale = visualSettings.FlowerVisualScale == Vector3.zero ? Vector3.one : visualSettings.FlowerVisualScale;
        scionDragScale = visualSettings.ScionDragScale == Vector3.zero ? Vector3.one : visualSettings.ScionDragScale;
        sourceRecoveryDurationSeconds = Mathf.Max(0.1f, visualSettings.SourceRecoveryDurationSeconds);

        gameObject.name = $"Branch_{Data.Index:00}_{Data.InitialColor}";
        EnsureVisuals();
        EnsureChargeHarvestSubscription();
        RefreshVisuals();
    }

    public void ShowPreview()
    {
        if (Data.State != BranchState.Idle || isGraftPreviewActive || previewFlowerRenderer == null)
        {
            return;
        }

        previewFlowerRenderer.gameObject.SetActive(true);
    }

    public void HidePreview()
    {
        if (previewFlowerRenderer != null)
        {
            previewFlowerRenderer.gameObject.SetActive(false);
        }
    }

    public FlowerColor GetPreviewFlowerColor()
    {
        return Data.CurrentColor;
    }

    public bool TryStartGrowthFromPreview()
    {
        if (!IsPreviewVisible || Data.State != BranchState.Idle)
        {
            return false;
        }

        HidePreview();
        return growthSystem.TryStartGrowth(GetPreviewFlowerColor());
    }

    public bool TryHarvestMatureFlower()
    {
        if (Data.State != BranchState.Mature || GameManager.Instance == null || GameManager.Instance.Inventory == null)
        {
            return false;
        }

        GameManager.Instance.Inventory.AddFlower(new FlowerData(Data.CurrentColor));
        Data.SetState(BranchState.Idle);
        isGraftPreviewActive = false;
        RefreshVisuals();
        return true;
    }

    public bool TryStartChargeHarvestFromMatureFlower()
    {
        if (Data.State != BranchState.Mature || GameManager.Instance == null || GameManager.Instance.ChargeHarvest == null)
        {
            return false;
        }

        return GameManager.Instance.ChargeHarvest.StartChargeHarvest(this);
    }

    public bool IsWorldPointInsideMatureFlower(Vector3 worldPoint)
    {
        return Data.State == BranchState.Mature
            && matureFlowerRenderer != null
            && matureFlowerRenderer.gameObject.activeSelf
            && matureFlowerRenderer.bounds.Contains(worldPoint);
    }

    public bool IsWorldPointInsidePreviewFlower(Vector3 worldPoint)
    {
        return IsPreviewVisible
            && previewFlowerRenderer != null
            && previewFlowerRenderer.bounds.Contains(worldPoint);
    }

    public bool CanStartScionDrag()
    {
        return Data != null && Data.State == BranchState.Idle;
    }

    public bool CanReceiveGraft()
    {
        return Data != null && Data.State == BranchState.Idle;
    }

    public void BeginSourceRecovery()
    {
        if (!CanStartScionDrag())
        {
            return;
        }

        HidePreview();
        Data.SetState(BranchState.SourceRecovering);
        isGraftPreviewActive = false;
        RefreshVisuals();

        if (sourceRecoveryRoutine != null)
        {
            StopCoroutine(sourceRecoveryRoutine);
        }

        sourceRecoveryRoutine = StartCoroutine(RecoverSourceBranchRoutine());
    }

    public void SetGraftPreviewActive(bool isActive)
    {
        bool nextState = isActive && CanReceiveGraft();
        if (isGraftPreviewActive == nextState)
        {
            return;
        }

        isGraftPreviewActive = nextState;
        if (isGraftPreviewActive)
        {
            HidePreview();
        }

        RefreshVisuals();
    }

    public bool TryBeginGraftedGrowth(FlowerColor fusedColor)
    {
        if (!CanReceiveGraft() || growthSystem == null)
        {
            return false;
        }

        HidePreview();
        isGraftPreviewActive = false;
        return growthSystem.TryStartGrowth(fusedColor, BranchState.GraftGrowing);
    }

    public void SetGrowthState(FlowerColor flowerColor, BranchState growthState)
    {
        Data.SetCurrentColor(flowerColor);
        Data.SetState(growthState);
        RefreshVisuals();
    }

    public void SetMature(FlowerColor flowerColor)
    {
        Data.SetCurrentColor(flowerColor);
        Data.SetState(BranchState.Mature);
        isGraftPreviewActive = false;
        RefreshVisuals();
    }

    public void SetCurrentColor(FlowerColor color)
    {
        Data.SetCurrentColor(color);
        RefreshVisuals();
    }

    public void ApplyChargeHarvestResult(HarvestResult result)
    {
        if (GameManager.Instance == null || GameManager.Instance.ChargeHarvestConfig == null)
        {
            return;
        }

        switch (result)
        {
            case HarvestResult.Perfect:
                Data.ReduceDamage(GameManager.Instance.ChargeHarvestConfig.PerfectDamageRecovery);
                break;
            case HarvestResult.Bad:
                Data.IncreaseDamage(GameManager.Instance.ChargeHarvestConfig.BadDamageIncrease);
                break;
        }

        TryHarvestMatureFlower();
    }

    public void RefreshVisuals()
    {
        gameObject.name = $"Branch_{Data.Index:00}_{Data.InitialColor}_Dmg{Data.DamageLevel}";
        Color branchColor = FlowerColorPalette.ToUnityColor(Data.CurrentColor);

        if (branchSpriteRenderer != null)
        {
            branchSpriteRenderer.sprite = ResolveBranchSprite();
            branchSpriteRenderer.color = Color.white;
        }

        if (colorMarkerRenderer != null)
        {
            colorMarkerRenderer.sprite = budSprite;
            colorMarkerRenderer.color = branchColor;
            colorMarkerRenderer.gameObject.SetActive(ShouldShowBud());
        }

        if (previewFlowerRenderer != null)
        {
            previewFlowerRenderer.sprite = grownSprite;
            previewFlowerRenderer.color = branchColor;
            if (Data.State != BranchState.Idle || isGraftPreviewActive)
            {
                previewFlowerRenderer.gameObject.SetActive(false);
            }
        }

        if (matureFlowerRenderer != null)
        {
            matureFlowerRenderer.sprite = grownSprite;
            matureFlowerRenderer.color = branchColor;
            matureFlowerRenderer.gameObject.SetActive(Data.State == BranchState.Mature);
        }

        if (Data.State != BranchState.Growing && Data.State != BranchState.GraftGrowing)
        {
            SetGrowthProgressVisible(false);
        }
    }

    public void BeginGrowthProgress(float totalDurationSeconds)
    {
        SetGrowthProgressVisible(true);
        UpdateGrowthProgress(0f, totalDurationSeconds);
    }

    public void UpdateGrowthProgress(float elapsedSeconds, float totalDurationSeconds)
    {
        if (growthBarFill == null)
        {
            return;
        }

        float normalizedProgress = totalDurationSeconds <= 0f ? 1f : Mathf.Clamp01(elapsedSeconds / totalDurationSeconds);
        float fullWidth = growthBarSize.x - 0.06f;
        float fillWidth = Mathf.Max(0.02f, fullWidth * normalizedProgress);

        MeshFilter filter = growthBarFill.GetComponent<MeshFilter>();
        filter.sharedMesh = CreateProgressBarMesh(fillWidth, growthBarSize.y - 0.04f);
        growthBarFill.transform.localPosition = new Vector3(-fullWidth * 0.5f + fillWidth * 0.5f, 0f, -0.01f);
    }

    public void EndGrowthProgress()
    {
        SetGrowthProgressVisible(false);
    }

    private void EnsureVisuals()
    {
        if (branchSpriteRenderer == null)
        {
            branchSpriteRenderer = CreateSpriteRenderer("BranchBodySprite", bodyLocalPosition, branchSpriteScale, 0);
        }

        if (colorMarkerRenderer == null)
        {
            colorMarkerRenderer = CreateSpriteRenderer("BudVisual", markerLocalPosition, flowerVisualScale, 1);
        }

        if (previewFlowerRenderer == null)
        {
            previewFlowerRenderer = CreateSpriteRenderer("PreviewFlower", previewLocalPosition, flowerVisualScale, 2);
            previewFlowerRenderer.gameObject.SetActive(false);
        }

        if (matureFlowerRenderer == null)
        {
            matureFlowerRenderer = CreateSpriteRenderer("MatureFlower", markerLocalPosition, flowerVisualScale, 2);
            matureFlowerRenderer.gameObject.SetActive(false);
        }

        if (growthBarRoot == null)
        {
            growthBarRoot = new GameObject("GrowthBarRoot");
            growthBarRoot.transform.SetParent(transform, false);
            growthBarRoot.transform.localPosition = growthBarLocalPosition;

            SimpleShapeFactory.CreateRectangle("GrowthBarBackground", growthBarRoot.transform, growthBarSize, new Color(0.14f, 0.14f, 0.16f, 0.95f), 2);
            growthBarFill = SimpleShapeFactory.CreateRectangle("GrowthBarFill", growthBarRoot.transform, new Vector2(growthBarSize.x - 0.06f, growthBarSize.y - 0.04f), new Color(0.38f, 0.9f, 0.42f, 1f), 3);
            SetGrowthProgressVisible(false);
        }

        EnsureCollider();
        EnsureHoverController();
        EnsureDragDropController();
        EnsureGrowthSystem();
    }

    private SpriteRenderer CreateSpriteRenderer(string objectName, Vector3 localPosition, Vector3 localScale, int sortingOrder)
    {
        GameObject visualObject = new GameObject(objectName);
        visualObject.transform.SetParent(transform, false);
        visualObject.transform.localPosition = localPosition;
        visualObject.transform.localScale = localScale;

        SpriteRenderer renderer = visualObject.AddComponent<SpriteRenderer>();
        renderer.color = Color.white;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }

    private void EnsureCollider()
    {
        interactionCollider = GetComponent<BoxCollider2D>();
        interactionCollider.isTrigger = true;
        interactionCollider.size = new Vector2(
            Mathf.Max(previewFlowerSize.x, branchBodySize.x * Mathf.Max(1f, branchSpriteScale.x * 2f), flowerVisualScale.x),
            branchBodySize.y * Mathf.Max(1.25f, branchSpriteScale.y) + previewFlowerSize.y + 1.1f);
        interactionCollider.offset = new Vector2(0f, 0.15f);
    }

    private void EnsureHoverController()
    {
        HoverPreviewController hoverPreviewController = GetComponent<HoverPreviewController>();
        if (hoverPreviewController == null)
        {
            hoverPreviewController = gameObject.AddComponent<HoverPreviewController>();
        }

        hoverPreviewController.Initialize(this);
    }

    private void EnsureDragDropController()
    {
        DragDropController dragDropController = GetComponent<DragDropController>();
        if (dragDropController == null)
        {
            dragDropController = gameObject.AddComponent<DragDropController>();
        }

        dragDropController.Initialize(this);
    }

    private void EnsureGrowthSystem()
    {
        growthSystem = GetComponent<GrowthSystem>();
        if (growthSystem == null)
        {
            growthSystem = gameObject.AddComponent<GrowthSystem>();
        }

        growthSystem.Initialize(this);
    }

    private void EnsureChargeHarvestSubscription()
    {
        if (GameManager.Instance == null || GameManager.Instance.ChargeHarvest == null)
        {
            return;
        }

        GameManager.Instance.ChargeHarvest.ChargeHarvestFinished -= HandleChargeHarvestFinished;
        GameManager.Instance.ChargeHarvest.ChargeHarvestFinished += HandleChargeHarvestFinished;
    }

    private void HandleChargeHarvestFinished(BranchController targetBranch, HarvestResult result)
    {
        if (targetBranch != this)
        {
            return;
        }

        ApplyChargeHarvestResult(result);
    }

    private IEnumerator RecoverSourceBranchRoutine()
    {
        yield return new WaitForSeconds(sourceRecoveryDurationSeconds);

        if (Data != null && Data.State == BranchState.SourceRecovering)
        {
            Data.SetState(BranchState.Idle);
            RefreshVisuals();
        }

        sourceRecoveryRoutine = null;
    }

    private Sprite ResolveBranchSprite()
    {
        if (Data == null)
        {
            return initialBranchSprite;
        }

        switch (Data.State)
        {
            case BranchState.SourceRecovering:
                return cutSourceBranchSprite != null ? cutSourceBranchSprite : initialBranchSprite;
            case BranchState.GraftGrowing:
                return graftBandagedBranchSprite != null ? graftBandagedBranchSprite : initialBranchSprite;
            case BranchState.Mature:
                return matureBranchSprite != null ? matureBranchSprite : initialBranchSprite;
            default:
                if (isGraftPreviewActive && graftPreviewBranchSprite != null)
                {
                    return graftPreviewBranchSprite;
                }

                return initialBranchSprite;
        }
    }

    private bool ShouldShowBud()
    {
        if (Data == null)
        {
            return false;
        }

        return Data.State != BranchState.Mature
            && Data.State != BranchState.SourceRecovering
            && !isGraftPreviewActive;
    }

    private void SetGrowthProgressVisible(bool isVisible)
    {
        if (growthBarRoot != null)
        {
            growthBarRoot.SetActive(isVisible);
        }
    }

    private static Mesh CreateProgressBarMesh(float width, float height)
    {
        float halfWidth = width * 0.5f;
        float halfHeight = height * 0.5f;

        Mesh mesh = new Mesh();
        mesh.name = "GrowthProgressBar";
        mesh.vertices = new[]
        {
            new Vector3(-halfWidth, -halfHeight, 0f),
            new Vector3(halfWidth, -halfHeight, 0f),
            new Vector3(-halfWidth, halfHeight, 0f),
            new Vector3(halfWidth, halfHeight, 0f)
        };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateBounds();
        return mesh;
    }
}
