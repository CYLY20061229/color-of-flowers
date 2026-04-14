using System.Collections.Generic;
using UnityEngine;

public class BranchFactory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform branchRoot;

    [Header("Layout")]
    [SerializeField] private int columnsPerColor = GameConstants.BranchesPerBaseColor;
    [SerializeField] private float groupSpacing = 3.05f;
    [SerializeField] private float intraGroupSpacing = 0.92f;
    [SerializeField] private Vector3 startPosition = new Vector3(-3.6f, 0.9f, 0f);

    [Header("Branch Visuals")]
    [SerializeField] private Vector2 branchBodySize = new Vector2(0.25f, 1f);
    [SerializeField] private Vector2 previewFlowerSize = new Vector2(0.7f, 0.6f);
    [SerializeField] private Vector3 previewLocalPosition = new Vector3(0f, 1.05f, 0f);
    [SerializeField] private Vector3 branchSpriteScale = new Vector3(0.95f, 1.15f, 1f);
    [SerializeField] private Sprite budSprite;
    [SerializeField] private Sprite grownSprite;
    [SerializeField] private Sprite initialBranchSprite;
    [SerializeField] private Sprite cutSourceBranchSprite;
    [SerializeField] private Sprite graftPreviewBranchSprite;
    [SerializeField] private Sprite graftBandagedBranchSprite;
    [SerializeField] private Sprite matureBranchSprite;
    [SerializeField] private Sprite scionDragSprite;
    [SerializeField] private Vector3 flowerVisualScale = new Vector3(0.6f, 0.6f, 1f);
    [SerializeField] private Vector3 scionDragScale = new Vector3(0.6f, 0.6f, 1f);
    [SerializeField] private float sourceRecoveryDurationSeconds = 3.5f;

    private readonly List<BranchController> spawnedBranches = new List<BranchController>();

    public IReadOnlyList<BranchController> SpawnedBranches => spawnedBranches;

    private void Start()
    {
        ApplyPrototypeDefaults();
        SpawnInitialBranches();
    }

    public void ResetBranches()
    {
        ClearBranches();
        SpawnInitialBranches();
    }

    public void ClearBranches()
    {
        spawnedBranches.Clear();

        if (branchRoot == null)
        {
            return;
        }

        for (int childIndex = branchRoot.childCount - 1; childIndex >= 0; childIndex--)
        {
            Destroy(branchRoot.GetChild(childIndex).gameObject);
        }
    }

    public void SpawnInitialBranches()
    {
        if (spawnedBranches.Count > 0)
        {
            return;
        }

        EnsureBranchRoot();
        EnsureFlowerSprites();

        FlowerColor[] baseColors = { FlowerColor.Red, FlowerColor.Green, FlowerColor.Blue };
        int branchIndex = 0;

        for (int groupIndex = 0; groupIndex < baseColors.Length; groupIndex++)
        {
            for (int column = 0; column < columnsPerColor; column++)
            {
                Vector3 position = GetBranchPosition(groupIndex, column);
                BranchController branch = CreateBranch(branchIndex, baseColors[groupIndex], position);
                spawnedBranches.Add(branch);
                branchIndex++;
            }
        }
    }

    private void ApplyPrototypeDefaults()
    {
        columnsPerColor = GameConstants.BranchesPerBaseColor;
        groupSpacing = 2.12f;
        intraGroupSpacing = 0.58f;
        startPosition = new Vector3(-2.5f, 1.1f, 0f);

        branchSpriteScale = new Vector3(0.5f, 0.62f, 1f);
        flowerVisualScale = new Vector3(0.34f, 0.34f, 1f);
        scionDragScale = new Vector3(0.3f, 0.3f, 1f);

        branchBodySize = new Vector2(0.14f, 0.56f);
        previewFlowerSize = new Vector2(0.38f, 0.36f);
    }

    private Vector3 GetBranchPosition(int groupIndex, int columnIndex)
    {
        float x = startPosition.x + groupIndex * groupSpacing + columnIndex * intraGroupSpacing;
        float y = startPosition.y + GetGroupHeightOffset(groupIndex) + GetWithinGroupHeightOffset(columnIndex);
        return new Vector3(x, y, startPosition.z);
    }

    private float GetGroupHeightOffset(int groupIndex)
    {
        switch (groupIndex)
        {
            case 1:
                return 0.34f;
            case 2:
                return 0.02f;
            default:
                return 0f;
        }
    }

    private float GetWithinGroupHeightOffset(int columnIndex)
    {
        switch (columnIndex)
        {
            case 0:
                return -0.08f;
            case 2:
                return 0.08f;
            default:
                return 0f;
        }
    }

    private BranchController CreateBranch(int index, FlowerColor color, Vector3 localPosition)
    {
        GameObject branchObject = new GameObject($"Branch_{index:00}");
        branchObject.transform.SetParent(branchRoot, false);
        branchObject.transform.localPosition = localPosition;

        BranchController branch = branchObject.AddComponent<BranchController>();
        branch.Initialize(new BranchData(index, color), CreateVisualSettings());
        return branch;
    }

    private BranchVisualSettings CreateVisualSettings()
    {
        return new BranchVisualSettings
        {
            BranchBodySize = branchBodySize,
            PreviewFlowerSize = previewFlowerSize,
            PreviewLocalPosition = previewLocalPosition,
            BranchSpriteScale = branchSpriteScale,
            BudSprite = budSprite,
            GrownSprite = grownSprite,
            InitialBranchSprite = initialBranchSprite,
            CutSourceBranchSprite = cutSourceBranchSprite,
            GraftPreviewBranchSprite = graftPreviewBranchSprite,
            GraftBandagedBranchSprite = graftBandagedBranchSprite,
            MatureBranchSprite = matureBranchSprite,
            ScionDragSprite = scionDragSprite,
            FlowerVisualScale = flowerVisualScale,
            ScionDragScale = scionDragScale,
            SourceRecoveryDurationSeconds = sourceRecoveryDurationSeconds
        };
    }

    private void EnsureBranchRoot()
    {
        if (branchRoot != null)
        {
            if (GameManager.Instance != null && GameManager.Instance.WorkspaceRoot != null && branchRoot.parent != GameManager.Instance.WorkspaceRoot)
            {
                branchRoot.SetParent(GameManager.Instance.WorkspaceRoot, false);
            }

            return;
        }

        GameObject rootObject = GameObject.Find("BranchRoot");
        if (rootObject == null)
        {
            rootObject = new GameObject("BranchRoot");
        }

        branchRoot = rootObject.transform;

        if (GameManager.Instance != null && GameManager.Instance.WorkspaceRoot != null)
        {
            branchRoot.SetParent(GameManager.Instance.WorkspaceRoot, false);
        }
    }

    private void EnsureFlowerSprites()
    {
        if (budSprite == null)
        {
            budSprite = LoadSpriteResource("Flowers/idle");
        }

        if (grownSprite == null)
        {
            grownSprite = LoadSpriteResource("Flowers/grown");
        }

        if (initialBranchSprite == null)
        {
            initialBranchSprite = LoadSpriteResource("Branches/branch_original");
        }

        if (cutSourceBranchSprite == null)
        {
            cutSourceBranchSprite = LoadSpriteResource("Branches/branch_cut");
        }

        if (graftPreviewBranchSprite == null)
        {
            graftPreviewBranchSprite = LoadSpriteResource("Branches/branch_crack");
        }

        if (graftBandagedBranchSprite == null)
        {
            graftBandagedBranchSprite = LoadSpriteResource("Branches/branch_recover");
        }

        if (matureBranchSprite == null)
        {
            matureBranchSprite = LoadSpriteResource("Branches/branch_mature");
        }

        if (scionDragSprite == null)
        {
            scionDragSprite = LoadSpriteResource("Branches/cut");
        }
    }

    private Sprite LoadSpriteResource(string resourcePath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            512f);
    }
}
