using System.Collections.Generic;
using UnityEngine;

public class BranchFactory : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform branchRoot;

    [Header("Layout")]
    [SerializeField] private int columnsPerColor = GameConstants.BranchesPerBaseColor;
    [SerializeField] private float horizontalSpacing = 1.25f;
    [SerializeField] private float verticalSpacing = 1.75f;
    [SerializeField] private Vector3 startPosition = new Vector3(-3.15f, 1.75f, 0f);

    [Header("Branch Visuals")]
    [SerializeField] private Vector2 branchBodySize = new Vector2(0.25f, 1f);
    [SerializeField] private Vector2 previewFlowerSize = new Vector2(0.7f, 0.6f);
    [SerializeField] private Vector3 previewLocalPosition = new Vector3(0f, 1.05f, 0f);
    [SerializeField] private Sprite budSprite;
    [SerializeField] private Sprite grownSprite;
    [SerializeField] private Vector3 flowerVisualScale = new Vector3(0.6f, 0.6f, 1f);

    private readonly List<BranchController> spawnedBranches = new List<BranchController>();

    public IReadOnlyList<BranchController> SpawnedBranches => spawnedBranches;

    private void Start()
    {
        SpawnInitialBranches();
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

        for (int row = 0; row < baseColors.Length; row++)
        {
            for (int column = 0; column < columnsPerColor; column++)
            {
                Vector3 position = startPosition + new Vector3(column * horizontalSpacing, -row * verticalSpacing, 0f);
                BranchController branch = CreateBranch(branchIndex, baseColors[row], position);
                spawnedBranches.Add(branch);
                branchIndex++;
            }
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
            BudSprite = budSprite,
            GrownSprite = grownSprite,
            FlowerVisualScale = flowerVisualScale
        };
    }

    private void EnsureBranchRoot()
    {
        if (branchRoot != null)
        {
            return;
        }

        GameObject rootObject = GameObject.Find("BranchRoot");
        if (rootObject == null)
        {
            rootObject = new GameObject("BranchRoot");
        }

        branchRoot = rootObject.transform;
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
