using System.Text;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OrderView : MonoBehaviour
{
    private const float PanelWidth = 2.2f;
    private const float PanelHeight = 0.9f;

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
        labelObject.transform.localPosition = new Vector3(-0.95f, 0.34f, -0.01f);

        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.UpperLeft;
        label.alignment = TextAlignment.Left;
        label.characterSize = 0.052f;
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
            label.text = "顾客订单\n\n暂无订单";
            SetPanelColor(false);
            return;
        }

        if (orderData.IsCompleted)
        {
            label.text = $"{orderData.CustomerName}\n已完成";
            SetPanelColor(true);
            return;
        }

        bool selected = orderSystem.SelectedOrder == orderData;

        StringBuilder builder = new StringBuilder();
        builder.AppendLine(selected ? $"{orderData.CustomerName} 进行中" : orderData.CustomerName);
        builder.AppendLine(TrimSummary(orderData.ChatSummary, 24));
        builder.Append(orderData.HasBouquetOrder ? "点击查看" : "点击提交");

        label.text = builder.ToString();
        SetPanelColor(selected);
    }

    private static string TrimSummary(string summary, int maxLength)
    {
        if (string.IsNullOrEmpty(summary) || summary.Length <= maxLength)
        {
            return summary;
        }

        return summary.Substring(0, maxLength - 3) + "...";
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
