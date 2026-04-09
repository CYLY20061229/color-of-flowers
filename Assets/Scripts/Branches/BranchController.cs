using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BranchController : MonoBehaviour
{
    [Header("Shape Layout")]
    [SerializeField] private Vector2 branchBodySize = new Vector2(0.25f, 1f);
    [SerializeField] private float colorMarkerRadius = 0.32f;
    [SerializeField] private Vector2 previewFlowerSize = new Vector2(0.7f, 0.6f);
    [SerializeField] private Vector3 bodyLocalPosition = new Vector3(0f, -0.35f, 0f);
    [SerializeField] private Vector3 markerLocalPosition = new Vector3(0f, 0.35f, 0f);
    [SerializeField] private Vector3 previewLocalPosition = new Vector3(0f, 1.05f, 0f);

    private GameObject bodyVisual;
    private GameObject colorMarkerVisual;
    private GameObject previewFlowerVisual;
    private GameObject matureFlowerVisual;
    private BoxCollider2D interactionCollider;
    private GrowthSystem growthSystem;

    public BranchData Data { get; private set; }
    public FlowerColor CurrentColor => Data.CurrentColor;
    public bool IsPreviewVisible => previewFlowerVisual != null && previewFlowerVisual.activeSelf;

    public void Initialize(BranchData data, BranchVisualSettings visualSettings)
    {
        Data = data;
        branchBodySize = visualSettings.BranchBodySize;
        colorMarkerRadius = visualSettings.ColorMarkerRadius;
        previewFlowerSize = visualSettings.PreviewFlowerSize;
        previewLocalPosition = visualSettings.PreviewLocalPosition;

        gameObject.name = $"Branch_{Data.Index:00}_{Data.InitialColor}";
        EnsureVisuals();
        RefreshVisuals();
    }

    public void ShowPreview()
    {
        if (Data.State != BranchState.Idle)
        {
            return;
        }

        previewFlowerVisual.SetActive(true);
    }

    public void HidePreview()
    {
        previewFlowerVisual.SetActive(false);
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
        RefreshVisuals();
        return true;
    }

    public bool IsWorldPointInsideMatureFlower(Vector3 worldPoint)
    {
        if (Data.State != BranchState.Mature)
        {
            return false;
        }

        Vector3 localPoint = transform.InverseTransformPoint(worldPoint) - markerLocalPosition;
        float halfWidth = previewFlowerSize.x * 0.5f;
        float halfHeight = previewFlowerSize.y * 0.5f;

        Vector2 top = new Vector2(0f, halfHeight);
        Vector2 bottomLeft = new Vector2(-halfWidth, -halfHeight);
        Vector2 bottomRight = new Vector2(halfWidth, -halfHeight);
        return IsPointInTriangle(localPoint, top, bottomLeft, bottomRight);
    }

    public bool IsWorldPointInsidePreviewFlower(Vector3 worldPoint)
    {
        if (!IsPreviewVisible)
        {
            return false;
        }

        Vector3 localPoint = transform.InverseTransformPoint(worldPoint) - previewLocalPosition;
        float halfWidth = previewFlowerSize.x * 0.5f;
        float halfHeight = previewFlowerSize.y * 0.5f;

        Vector2 top = new Vector2(0f, halfHeight);
        Vector2 bottomLeft = new Vector2(-halfWidth, -halfHeight);
        Vector2 bottomRight = new Vector2(halfWidth, -halfHeight);
        return IsPointInTriangle(localPoint, top, bottomLeft, bottomRight);
    }

    public void SetGrowing(FlowerColor flowerColor)
    {
        Data.SetCurrentColor(flowerColor);
        Data.SetState(BranchState.Growing);
        RefreshVisuals();
    }

    public void SetMature(FlowerColor flowerColor)
    {
        Data.SetCurrentColor(flowerColor);
        Data.SetState(BranchState.Mature);
        RefreshVisuals();
    }

    public void SetCurrentColor(FlowerColor color)
    {
        Data.SetCurrentColor(color);
        RefreshVisuals();
    }

    public void RefreshVisuals()
    {
        Color branchColor = FlowerColorPalette.ToUnityColor(Data.CurrentColor);
        SimpleShapeFactory.SetColor(colorMarkerVisual, branchColor);
        SimpleShapeFactory.SetColor(previewFlowerVisual, branchColor);
        SimpleShapeFactory.SetColor(matureFlowerVisual, branchColor);

        bool mature = Data.State == BranchState.Mature;
        colorMarkerVisual.SetActive(!mature);
        matureFlowerVisual.SetActive(mature);
        previewFlowerVisual.SetActive(false);
    }

    private void EnsureVisuals()
    {
        if (bodyVisual == null)
        {
            bodyVisual = SimpleShapeFactory.CreateRectangle("BranchBody", transform, branchBodySize, GameConstants.BranchBodyColor, 0);
            bodyVisual.transform.localPosition = bodyLocalPosition;
        }

        if (colorMarkerVisual == null)
        {
            colorMarkerVisual = SimpleShapeFactory.CreateCircle("ColorMarker", transform, colorMarkerRadius, Color.white, 1);
            colorMarkerVisual.transform.localPosition = markerLocalPosition;
        }

        if (previewFlowerVisual == null)
        {
            previewFlowerVisual = SimpleShapeFactory.CreateTriangle("PreviewFlower", transform, previewFlowerSize, Color.white, 2);
            previewFlowerVisual.transform.localPosition = previewLocalPosition;
            previewFlowerVisual.SetActive(false);
        }

        if (matureFlowerVisual == null)
        {
            matureFlowerVisual = SimpleShapeFactory.CreateTriangle("MatureFlower", transform, previewFlowerSize, Color.white, 2);
            matureFlowerVisual.transform.localPosition = markerLocalPosition;
            matureFlowerVisual.SetActive(false);
        }

        EnsureCollider();
        EnsureHoverController();
        EnsureDragDropController();
        EnsureGrowthSystem();
    }

    private void EnsureCollider()
    {
        interactionCollider = GetComponent<BoxCollider2D>();
        interactionCollider.isTrigger = true;
        interactionCollider.size = new Vector2(
            Mathf.Max(previewFlowerSize.x, branchBodySize.x, colorMarkerRadius * 2f),
            branchBodySize.y + colorMarkerRadius * 2f + previewFlowerSize.y);
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

    private static bool IsPointInTriangle(Vector2 point, Vector2 a, Vector2 b, Vector2 c)
    {
        float area = GetTriangleSign(a, b, c);
        float area1 = GetTriangleSign(point, b, c);
        float area2 = GetTriangleSign(a, point, c);
        float area3 = GetTriangleSign(a, b, point);

        bool hasNegative = area1 < 0f || area2 < 0f || area3 < 0f;
        bool hasPositive = area1 > 0f || area2 > 0f || area3 > 0f;
        return Mathf.Approximately(area, 0f) == false && !(hasNegative && hasPositive);
    }

    private static float GetTriangleSign(Vector2 a, Vector2 b, Vector2 c)
    {
        return (a.x - c.x) * (b.y - c.y) - (b.x - c.x) * (a.y - c.y);
    }
}
