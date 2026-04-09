using System.Text;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OrderView : MonoBehaviour
{
    private const float PanelWidth = 2.8f;
    private const float PanelHeight = 1.35f;

    private OrderSystem orderSystem;
    private OrderData orderData;
    private TextMesh label;
    private GameObject panel;

    public void Initialize(OrderSystem system, OrderData order)
    {
        if (orderSystem != null)
        {
            orderSystem.OrdersChanged -= Refresh;
            orderSystem.ActiveOrderChanged -= HandleActiveOrderChanged;
        }

        orderSystem = system;
        orderData = order;
        orderSystem.OrdersChanged += Refresh;
        orderSystem.ActiveOrderChanged += HandleActiveOrderChanged;

        EnsureVisuals();
        Refresh();
    }

    private void OnDestroy()
    {
        if (orderSystem != null)
        {
            orderSystem.OrdersChanged -= Refresh;
            orderSystem.ActiveOrderChanged -= HandleActiveOrderChanged;
        }
    }

    private void OnMouseDown()
    {
        if (orderSystem == null || orderData == null)
        {
            return;
        }

        orderSystem.SelectOrder(orderData);
    }

    private void EnsureVisuals()
    {
        if (label != null)
        {
            return;
        }

        panel = SimpleShapeFactory.CreateRectangle("OrderPanel", transform, new Vector2(PanelWidth, PanelHeight), new Color(0.1f, 0.14f, 0.18f, 0.85f), 10);

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(PanelWidth, PanelHeight);

        GameObject labelObject = new GameObject("OrderLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(-1.2f, 0.46f, -0.01f);

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
        if (orderSystem == null || label == null)
        {
            return;
        }

        if (orderData == null)
        {
            label.text = "Customer Order\n\nNo orders";
            SetPanelColor(false);
            return;
        }

        if (orderData.IsCompleted)
        {
            label.text = "Order Complete\n\nThanks!";
            SetPanelColor(true);
            return;
        }

        bool selected = orderSystem.SelectedOrder == orderData;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(selected ? $"Customer #{orderData.Id} Selected" : $"Customer #{orderData.Id}");
        builder.AppendLine("Click to submit");
        builder.AppendLine();

        for (int i = 0; i < orderData.Requirements.Count; i++)
        {
            OrderRequirement requirement = orderData.Requirements[i];
            builder.AppendLine($"{FlowerColorPalette.GetDisplayName(requirement.Color)} x{requirement.RequiredCount}");
        }

        label.text = builder.ToString();
        SetPanelColor(selected);
    }

    private void SetPanelColor(bool selected)
    {
        if (panel == null)
        {
            return;
        }

        Color color = selected
            ? new Color(0.16f, 0.36f, 0.24f, 0.9f)
            : new Color(0.1f, 0.14f, 0.18f, 0.85f);
        SimpleShapeFactory.SetColor(panel, color);
    }
}
