using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class FlowerSpeciesRegionButton : MonoBehaviour
{
    private FlowerSpeciesMapController mapController;
    private FlowerSpeciesState speciesState;
    private GameObject background;
    private TextMesh label;
    private TextMesh statusLabel;

    public string SpeciesId => speciesState != null ? speciesState.Definition.SpeciesId : string.Empty;

    public void Initialize(FlowerSpeciesMapController controller, FlowerSpeciesState state)
    {
        mapController = controller;
        speciesState = state;
        EnsureVisuals();
        Refresh();
    }

    public void Refresh()
    {
        if (speciesState == null)
        {
            return;
        }

        FlowerSpeciesDefinition definition = speciesState.Definition;
        transform.localPosition = definition.MapButtonPosition;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = definition.MapButtonSize;

        if (background != null)
        {
            SimpleShapeFactory.SetColor(background, speciesState.IsUnlocked
                ? definition.AccentColor
                : new Color(0.17f, 0.19f, 0.22f, 0.92f));
        }

        if (label != null)
        {
            label.text = definition.DisplayName;
        }

        if (statusLabel != null)
        {
            statusLabel.text = speciesState.IsUnlocked
                ? "点击进入"
                : $"解锁 {definition.UnlockCost}";
        }
    }

    private void OnMouseDown()
    {
        mapController?.HandleRegionClicked(speciesState);
    }

    private void EnsureVisuals()
    {
        FlowerSpeciesDefinition definition = speciesState.Definition;

        if (background == null)
        {
            background = SimpleShapeFactory.CreateRectangle(
                "Background",
                transform,
                definition.MapButtonSize,
                definition.AccentColor,
                31);
        }

        if (label == null)
        {
            GameObject labelObject = new GameObject("Name");
            labelObject.transform.SetParent(transform, false);
            labelObject.transform.localPosition = new Vector3(0f, 0.1f, -0.01f);

            label = labelObject.AddComponent<TextMesh>();
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.characterSize = 0.05f;
            label.fontSize = 48;
            label.color = Color.white;
            label.GetComponent<MeshRenderer>().sortingOrder = 32;
        }

        if (statusLabel == null)
        {
            GameObject statusObject = new GameObject("Status");
            statusObject.transform.SetParent(transform, false);
            statusObject.transform.localPosition = new Vector3(0f, -0.12f, -0.01f);

            statusLabel = statusObject.AddComponent<TextMesh>();
            statusLabel.anchor = TextAnchor.MiddleCenter;
            statusLabel.alignment = TextAlignment.Center;
            statusLabel.characterSize = 0.036f;
            statusLabel.fontSize = 42;
            statusLabel.color = new Color(0.96f, 0.96f, 0.96f, 0.95f);
            statusLabel.GetComponent<MeshRenderer>().sortingOrder = 32;
        }
    }
}
