using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BouquetSlotView : MonoBehaviour
{
    private const float SlotSize = 0.42f;

    private BouquetSlotState slotState;
    private BouquetOrderManager bouquetOrderManager;
    private GameObject slotBackground;
    private GameObject targetHint;
    private GameObject placedFlowerVisual;
    private TextMesh label;

    public BouquetSlotState SlotState => slotState;

    public void Initialize(BouquetSlotState state, BouquetOrderManager manager)
    {
        slotState = state;
        bouquetOrderManager = manager;
        EnsureVisuals();
        EnsureReturnDragHandler();
        Refresh();
    }

    public void Refresh()
    {
        if (slotState == null)
        {
            return;
        }

        Color targetColor = FlowerColorPalette.ToUnityColor(slotState.Requirement.RequiredFlowerColor);
        SimpleShapeFactory.SetColor(targetHint, new Color(targetColor.r, targetColor.g, targetColor.b, 0.45f));

        bool filled = slotState.IsFilled;
        placedFlowerVisual.SetActive(filled);
        if (filled)
        {
            SimpleShapeFactory.SetColor(placedFlowerVisual, FlowerColorPalette.ToUnityColor(slotState.CurrentFlowerData.Color));
        }

        Color backgroundColor = filled
            ? (slotState.IsCorrect ? new Color(0.15f, 0.45f, 0.22f, 0.9f) : new Color(0.55f, 0.16f, 0.14f, 0.9f))
            : new Color(0.08f, 0.08f, 0.08f, 0.85f);
        SimpleShapeFactory.SetColor(slotBackground, backgroundColor);

        label.text = FlowerColorPalette.GetDisplayName(slotState.Requirement.RequiredFlowerColor);
    }

    private void EnsureVisuals()
    {
        if (slotBackground != null)
        {
            return;
        }

        slotBackground = SimpleShapeFactory.CreateRectangle("SlotBackground", transform, new Vector2(SlotSize, SlotSize), new Color(0.08f, 0.08f, 0.08f, 0.85f), 21);
        targetHint = SimpleShapeFactory.CreateCircle("TargetColorHint", transform, 0.16f, Color.white, 22);
        placedFlowerVisual = SimpleShapeFactory.CreateTriangle("PlacedFlowerVisual", transform, new Vector2(0.34f, 0.32f), Color.white, 23);
        placedFlowerVisual.SetActive(false);

        GameObject labelObject = new GameObject("SlotLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, -0.34f, -0.01f);

        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.055f;
        label.fontSize = 48;
        label.color = Color.white;

        MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 24;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(SlotSize, SlotSize);
    }

    private void EnsureReturnDragHandler()
    {
        if (bouquetOrderManager == null)
        {
            return;
        }

        SlotFlowerReturnDragHandler dragHandler = GetComponent<SlotFlowerReturnDragHandler>();
        if (dragHandler == null)
        {
            dragHandler = gameObject.AddComponent<SlotFlowerReturnDragHandler>();
        }

        dragHandler.Initialize(this, bouquetOrderManager);
    }
}
