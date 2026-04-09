using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class InventoryFlowerItemView : MonoBehaviour
{
    private const float ItemWidth = 1.75f;
    private const float ItemHeight = 0.28f;

    private FlowerColor color;
    private TextMesh label;
    private GameObject background;
    private BouquetSystem bouquetSystem;
    private BouquetOrderManager bouquetOrderManager;

    public void Initialize(FlowerColor flowerColor, int count, BouquetSystem bouquet, BouquetOrderManager bouquetOrders = null)
    {
        color = flowerColor;
        bouquetSystem = bouquet;
        bouquetOrderManager = bouquetOrders;
        EnsureVisuals();
        EnsureDragHandler();
        Refresh(count);
    }

    private void OnMouseDown()
    {
        if (bouquetOrderManager != null && bouquetOrderManager.HasActiveBouquetOrder)
        {
            return;
        }

        if (bouquetSystem == null)
        {
            return;
        }

        bouquetSystem.TrySubmitFlower(color);
    }

    private void EnsureVisuals()
    {
        if (label != null)
        {
            return;
        }

        background = SimpleShapeFactory.CreateRectangle("ItemBackground", transform, new Vector2(ItemWidth, ItemHeight), new Color(0.16f, 0.16f, 0.16f, 0.9f), 12);
        GameObject colorChip = SimpleShapeFactory.CreateTriangle("FlowerChip", transform, new Vector2(0.22f, 0.2f), FlowerColorPalette.ToUnityColor(color), 13);
        colorChip.transform.localPosition = new Vector3(-0.72f, 0f, -0.01f);

        GameObject labelObject = new GameObject("ItemLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(-0.55f, 0.09f, -0.02f);

        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.UpperLeft;
        label.alignment = TextAlignment.Left;
        label.characterSize = 0.07f;
        label.fontSize = 48;
        label.color = Color.white;

        MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 14;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(ItemWidth, ItemHeight);
    }

    private void Refresh(int count)
    {
        label.text = $"{FlowerColorPalette.GetDisplayName(color)} x{count}";

        bool needed = bouquetSystem != null && bouquetSystem.IsColorNeededBySelectedOrder(color);
        Color backgroundColor = needed
            ? new Color(0.16f, 0.32f, 0.2f, 0.95f)
            : new Color(0.16f, 0.16f, 0.16f, 0.9f);
        SimpleShapeFactory.SetColor(background, backgroundColor);
    }

    private void EnsureDragHandler()
    {
        if (bouquetOrderManager == null)
        {
            return;
        }

        FlowerDragToSlotHandler dragHandler = GetComponent<FlowerDragToSlotHandler>();
        if (dragHandler == null)
        {
            dragHandler = gameObject.AddComponent<FlowerDragToSlotHandler>();
        }

        dragHandler.Initialize(color, bouquetOrderManager);
    }
}
