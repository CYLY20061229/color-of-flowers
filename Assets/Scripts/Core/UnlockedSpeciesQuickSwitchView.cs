using System.Collections.Generic;
using UnityEngine;

public class UnlockedSpeciesQuickSwitchView : MonoBehaviour
{
    private readonly List<UnlockedSpeciesQuickSwitchButton> buttons = new List<UnlockedSpeciesQuickSwitchButton>();

    private FlowerSpeciesMapController mapController;
    private TextMesh titleLabel;

    public void Initialize(FlowerSpeciesMapController controller)
    {
        mapController = controller;
        EnsureVisuals();
    }

    public void Refresh(IReadOnlyList<FlowerSpeciesState> unlockedSpecies, FlowerSpeciesState currentSpecies)
    {
        EnsureVisuals();
        ClearButtons();

        if (unlockedSpecies == null)
        {
            return;
        }

        titleLabel.text = currentSpecies != null
            ? $"当前花种：{currentSpecies.Definition.DisplayName}"
            : "当前花种";

        float startX = -1.4f;
        float spacing = 1.02f;

        for (int i = 0; i < unlockedSpecies.Count; i++)
        {
            GameObject buttonObject = new GameObject($"Species_{unlockedSpecies[i].Definition.SpeciesId}");
            buttonObject.transform.SetParent(transform, false);
            buttonObject.transform.localPosition = new Vector3(startX + i * spacing, -0.34f, 0f);

            UnlockedSpeciesQuickSwitchButton button = buttonObject.AddComponent<UnlockedSpeciesQuickSwitchButton>();
            button.Initialize(this, unlockedSpecies[i], false);
            button.Refresh(unlockedSpecies[i] == currentSpecies);
            buttons.Add(button);
        }

        GameObject mapButtonObject = new GameObject("MapButton");
        mapButtonObject.transform.SetParent(transform, false);
        mapButtonObject.transform.localPosition = new Vector3(2.45f, -0.34f, 0f);

        UnlockedSpeciesQuickSwitchButton mapButton = mapButtonObject.AddComponent<UnlockedSpeciesQuickSwitchButton>();
        mapButton.Initialize(this, null, true);
        buttons.Add(mapButton);
    }

    public void HandleSpeciesSelected(FlowerSpeciesState species)
    {
        mapController?.EnterSpecies(species);
    }

    public void HandleBackToMap()
    {
        mapController?.ShowMap();
    }

    private void EnsureVisuals()
    {
        transform.position = new Vector3(0f, 3.55f, 0f);

        if (titleLabel != null)
        {
            return;
        }

        GameObject titleObject = new GameObject("Title");
        titleObject.transform.SetParent(transform, false);
        titleObject.transform.localPosition = new Vector3(-2.1f, 0f, -0.01f);

        titleLabel = titleObject.AddComponent<TextMesh>();
        titleLabel.anchor = TextAnchor.MiddleLeft;
        titleLabel.alignment = TextAlignment.Left;
        titleLabel.characterSize = 0.042f;
        titleLabel.fontSize = 48;
        titleLabel.color = Color.white;
        titleLabel.GetComponent<MeshRenderer>().sortingOrder = 37;
    }

    private void ClearButtons()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            if (buttons[i] != null)
            {
                Destroy(buttons[i].gameObject);
            }
        }

        buttons.Clear();
    }
}
