using System.Text;
using UnityEngine;

public class BouquetDropZone : MonoBehaviour
{
    private const float PanelWidth = 2.8f;
    private const float PanelHeight = 1.35f;

    private BouquetSystem bouquetSystem;
    private OrderSystem orderSystem;
    private TextMesh label;

    public void Initialize(BouquetSystem bouquet, OrderSystem orders)
    {
        if (bouquetSystem != null)
        {
            bouquetSystem.BouquetChanged -= Refresh;
        }

        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged -= HandleActiveOrderChanged;
            orderSystem.OrdersChanged -= Refresh;
        }

        bouquetSystem = bouquet;
        orderSystem = orders;

        bouquetSystem.BouquetChanged += Refresh;
        orderSystem.ActiveOrderChanged += HandleActiveOrderChanged;
        orderSystem.OrdersChanged += Refresh;

        EnsureVisuals();
        Refresh();
    }

    private void OnDestroy()
    {
        if (bouquetSystem != null)
        {
            bouquetSystem.BouquetChanged -= Refresh;
        }

        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged -= HandleActiveOrderChanged;
            orderSystem.OrdersChanged -= Refresh;
        }
    }

    private void EnsureVisuals()
    {
        if (label != null)
        {
            return;
        }

        SimpleShapeFactory.CreateRectangle("BouquetPanel", transform, new Vector2(PanelWidth, PanelHeight), new Color(0.12f, 0.1f, 0.08f, 0.85f), 10);

        GameObject labelObject = new GameObject("BouquetLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(-1.2f, 0.45f, -0.01f);

        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.UpperLeft;
        label.alignment = TextAlignment.Left;
        label.characterSize = 0.078f;
        label.fontSize = 48;
        label.color = Color.white;

        MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 11;
    }

    private void HandleActiveOrderChanged(OrderData order)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (label == null || bouquetSystem == null || orderSystem == null)
        {
            return;
        }

        OrderData order = orderSystem.SelectedOrder;
        if (order == null)
        {
            label.text = "Bouquet\n\nSelect an order";
            return;
        }

        if (order.IsCompleted)
        {
            label.text = "Bouquet\n\nOrder complete";
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Bouquet");

        for (int i = 0; i < order.Requirements.Count; i++)
        {
            OrderRequirement requirement = order.Requirements[i];
            int submittedCount = bouquetSystem.GetSubmittedCount(requirement.Color);
            builder.AppendLine($"{FlowerColorPalette.GetDisplayName(requirement.Color)} {submittedCount}/{requirement.RequiredCount}");
        }

        label.text = builder.ToString();
    }
}
