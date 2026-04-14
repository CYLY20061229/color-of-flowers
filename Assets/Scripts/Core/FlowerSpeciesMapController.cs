using System.Collections.Generic;
using UnityEngine;

public class FlowerSpeciesMapController : MonoBehaviour
{
    [SerializeField] private List<FlowerSpeciesDefinition> speciesDefinitions = new List<FlowerSpeciesDefinition>();
    [SerializeField] private List<FlowerSpeciesRegionButton> regionButtons = new List<FlowerSpeciesRegionButton>();
    [SerializeField] private Vector3 mapTitlePosition = new Vector3(0f, 4.45f, 0f);
    [SerializeField] private Vector3 mapHintPosition = new Vector3(0f, 3.9f, 0f);

    private readonly List<FlowerSpeciesState> speciesStates = new List<FlowerSpeciesState>();

    private CurrencyGateway currencyGateway;
    private BranchFactory branchFactory;
    private Transform mapRoot;
    private Transform workspaceRoot;
    private UnlockedSpeciesQuickSwitchView quickSwitchView;
    private TextMesh titleLabel;
    private TextMesh hintLabel;

    public FlowerSpeciesState CurrentSpecies { get; private set; }

    public void Initialize(
        CurrencyGateway currency,
        BranchFactory branches,
        Transform targetMapRoot,
        Transform targetWorkspaceRoot)
    {
        currencyGateway = currency;
        branchFactory = branches;
        mapRoot = targetMapRoot;
        workspaceRoot = targetWorkspaceRoot;

        EnsureDefaultSpeciesDefinitions();
        BuildRuntimeStates();
        EnsureMapText();
        EnsureRegionButtons();
        RefreshRegionButtons();
        EnsureQuickSwitchView();
        ShowMap();
    }

    public void HandleRegionClicked(FlowerSpeciesState state)
    {
        if (state == null)
        {
            return;
        }

        if (!state.IsUnlocked)
        {
            if (currencyGateway == null || !currencyGateway.TrySpend(state.Definition.UnlockCost))
            {
                SetHint($"金币不足，无法解锁 {state.Definition.DisplayName}");
                return;
            }

            state.Unlock();
            SetHint($"已解锁 {state.Definition.DisplayName}");
            RefreshRegionButtons();
        }

        EnterSpecies(state);
    }

    public void EnterSpecies(FlowerSpeciesState state)
    {
        if (state == null || !state.IsUnlocked)
        {
            return;
        }

        CurrentSpecies = state;

        if (mapRoot != null)
        {
            mapRoot.gameObject.SetActive(false);
        }

        if (workspaceRoot != null)
        {
            workspaceRoot.gameObject.SetActive(true);
        }

        if (branchFactory != null)
        {
            branchFactory.ResetBranches();
        }

        if (quickSwitchView != null)
        {
            quickSwitchView.gameObject.SetActive(true);
            quickSwitchView.Refresh(GetUnlockedSpecies(), CurrentSpecies);
        }
    }

    public void ShowMap()
    {
        CurrentSpecies = null;

        if (workspaceRoot != null)
        {
            workspaceRoot.gameObject.SetActive(false);
        }

        if (mapRoot != null)
        {
            mapRoot.gameObject.SetActive(true);
        }

        if (quickSwitchView != null)
        {
            quickSwitchView.gameObject.SetActive(false);
        }

        SetHint("点击已解锁区域进入花种，或花金币解锁新区域");
        RefreshRegionButtons();
    }

    public IReadOnlyList<FlowerSpeciesState> GetUnlockedSpecies()
    {
        List<FlowerSpeciesState> unlockedSpecies = new List<FlowerSpeciesState>();
        for (int i = 0; i < speciesStates.Count; i++)
        {
            if (speciesStates[i].IsUnlocked)
            {
                unlockedSpecies.Add(speciesStates[i]);
            }
        }

        return unlockedSpecies;
    }

    private void EnsureDefaultSpeciesDefinitions()
    {
        if (speciesDefinitions.Count > 0)
        {
            return;
        }

        speciesDefinitions.Add(FlowerSpeciesDefinition.CreateDefault(
            "magnolia",
            "玉兰",
            0,
            true,
            new Vector3(-2.25f, 1.5f, 0f),
            new Color(0.87f, 0.76f, 0.58f, 0.96f)));

        speciesDefinitions.Add(FlowerSpeciesDefinition.CreateDefault(
            "cherry",
            "樱花",
            60,
            false,
            new Vector3(0f, 2.3f, 0f),
            new Color(0.88f, 0.56f, 0.72f, 0.96f)));

        speciesDefinitions.Add(FlowerSpeciesDefinition.CreateDefault(
            "hydrangea",
            "绣球",
            90,
            false,
            new Vector3(2.2f, 1.55f, 0f),
            new Color(0.47f, 0.67f, 0.92f, 0.96f)));
    }

    private void BuildRuntimeStates()
    {
        speciesStates.Clear();
        for (int i = 0; i < speciesDefinitions.Count; i++)
        {
            speciesStates.Add(new FlowerSpeciesState(speciesDefinitions[i], speciesDefinitions[i].UnlockedByDefault));
        }
    }

    private void EnsureMapText()
    {
        if (mapRoot == null)
        {
            return;
        }

        if (titleLabel == null)
        {
            GameObject titleObject = new GameObject("MapTitle");
            titleObject.transform.SetParent(mapRoot, false);
            titleObject.transform.localPosition = mapTitlePosition;

            titleLabel = titleObject.AddComponent<TextMesh>();
            titleLabel.text = "花种大树";
            titleLabel.anchor = TextAnchor.MiddleCenter;
            titleLabel.alignment = TextAlignment.Center;
            titleLabel.characterSize = 0.07f;
            titleLabel.fontSize = 56;
            titleLabel.color = Color.white;
            titleLabel.GetComponent<MeshRenderer>().sortingOrder = 34;
        }

        if (hintLabel == null)
        {
            GameObject hintObject = new GameObject("MapHint");
            hintObject.transform.SetParent(mapRoot, false);
            hintObject.transform.localPosition = mapHintPosition;

            hintLabel = hintObject.AddComponent<TextMesh>();
            hintLabel.anchor = TextAnchor.MiddleCenter;
            hintLabel.alignment = TextAlignment.Center;
            hintLabel.characterSize = 0.042f;
            hintLabel.fontSize = 46;
            hintLabel.color = new Color(0.97f, 0.97f, 0.97f, 0.96f);
            hintLabel.GetComponent<MeshRenderer>().sortingOrder = 34;
        }
    }

    private void EnsureRegionButtons()
    {
        if (mapRoot == null)
        {
            return;
        }

        regionButtons.RemoveAll(button => button == null);
        if (regionButtons.Count == speciesStates.Count)
        {
            for (int i = 0; i < regionButtons.Count; i++)
            {
                regionButtons[i].Initialize(this, speciesStates[i]);
            }

            return;
        }

        for (int i = 0; i < regionButtons.Count; i++)
        {
            if (regionButtons[i] != null)
            {
                Destroy(regionButtons[i].gameObject);
            }
        }

        regionButtons.Clear();
        for (int i = 0; i < speciesStates.Count; i++)
        {
            GameObject buttonObject = new GameObject($"{speciesStates[i].Definition.DisplayName}区域");
            buttonObject.transform.SetParent(mapRoot, false);

            FlowerSpeciesRegionButton button = buttonObject.AddComponent<FlowerSpeciesRegionButton>();
            button.Initialize(this, speciesStates[i]);
            regionButtons.Add(button);
        }
    }

    private void RefreshRegionButtons()
    {
        for (int i = 0; i < regionButtons.Count; i++)
        {
            if (regionButtons[i] != null)
            {
                regionButtons[i].Refresh();
            }
        }
    }

    private void EnsureQuickSwitchView()
    {
        if (workspaceRoot == null)
        {
            return;
        }

        if (quickSwitchView == null)
        {
            GameObject viewObject = new GameObject("UnlockedSpeciesQuickSwitchView");
            viewObject.transform.SetParent(workspaceRoot, false);
            quickSwitchView = viewObject.AddComponent<UnlockedSpeciesQuickSwitchView>();
        }

        quickSwitchView.Initialize(this);
        quickSwitchView.gameObject.SetActive(false);
    }

    private void SetHint(string message)
    {
        if (hintLabel != null)
        {
            hintLabel.text = message;
        }
    }
}
