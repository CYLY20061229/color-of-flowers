using System;
using UnityEngine;

[Serializable]
public class FlowerSpeciesDefinition
{
    [SerializeField] private string speciesId = "magnolia";
    [SerializeField] private string displayName = "玉兰";
    [SerializeField] private int unlockCost = 0;
    [SerializeField] private bool unlockedByDefault = false;
    [SerializeField] private Vector3 mapButtonPosition = Vector3.zero;
    [SerializeField] private Vector2 mapButtonSize = new Vector2(1.45f, 0.56f);
    [SerializeField] private Color accentColor = new Color(0.87f, 0.78f, 0.48f, 0.95f);

    public string SpeciesId => string.IsNullOrWhiteSpace(speciesId) ? "species" : speciesId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? SpeciesId : displayName;
    public int UnlockCost => Mathf.Max(0, unlockCost);
    public bool UnlockedByDefault => unlockedByDefault;
    public Vector3 MapButtonPosition => mapButtonPosition;
    public Vector2 MapButtonSize => mapButtonSize;
    public Color AccentColor => accentColor;

    public static FlowerSpeciesDefinition CreateDefault(
        string id,
        string name,
        int cost,
        bool defaultUnlocked,
        Vector3 buttonPosition,
        Color buttonColor)
    {
        return new FlowerSpeciesDefinition
        {
            speciesId = id,
            displayName = name,
            unlockCost = cost,
            unlockedByDefault = defaultUnlocked,
            mapButtonPosition = buttonPosition,
            accentColor = buttonColor
        };
    }
}
