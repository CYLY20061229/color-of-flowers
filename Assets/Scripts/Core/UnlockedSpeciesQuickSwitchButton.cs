using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class UnlockedSpeciesQuickSwitchButton : MonoBehaviour
{
    private UnlockedSpeciesQuickSwitchView owner;
    private FlowerSpeciesState speciesState;
    private bool isMapButton;
    private GameObject background;
    private TextMesh label;

    public void Initialize(UnlockedSpeciesQuickSwitchView targetOwner, FlowerSpeciesState state, bool mapButton)
    {
        owner = targetOwner;
        speciesState = state;
        isMapButton = mapButton;
        EnsureVisuals();
        Refresh(false);
    }

    public void Refresh(bool isCurrent)
    {
        if (background == null)
        {
            return;
        }

        Color color;
        string text;

        if (isMapButton)
        {
            color = new Color(0.13f, 0.16f, 0.2f, 0.92f);
            text = "地图";
        }
        else
        {
            color = isCurrent
                ? speciesState.Definition.AccentColor
                : new Color(0.18f, 0.22f, 0.26f, 0.92f);
            text = speciesState.Definition.DisplayName;
        }

        SimpleShapeFactory.SetColor(background, color);
        label.text = text;
    }

    private void OnMouseDown()
    {
        if (isMapButton)
        {
            owner?.HandleBackToMap();
            return;
        }

        owner?.HandleSpeciesSelected(speciesState);
    }

    private void EnsureVisuals()
    {
        if (background == null)
        {
            background = SimpleShapeFactory.CreateRectangle(
                "Background",
                transform,
                new Vector2(0.92f, 0.4f),
                new Color(0.18f, 0.22f, 0.26f, 0.92f),
                36);
        }

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.92f, 0.4f);

        if (label != null)
        {
            return;
        }

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.036f;
        label.fontSize = 42;
        label.color = Color.white;
        label.GetComponent<MeshRenderer>().sortingOrder = 37;
    }
}
