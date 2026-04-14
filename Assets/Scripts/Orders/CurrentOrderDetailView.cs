using System.Text;
using UnityEngine;

public class CurrentOrderDetailView : MonoBehaviour
{
    private const float PanelWidth = 5.45f;
    private const float PanelHeight = 1.72f;

    private static readonly Color PaperColor = new Color(0.96f, 0.9f, 0.76f, 0.98f);
    private static readonly Color PaperShadowColor = new Color(0.34f, 0.25f, 0.16f, 0.24f);
    private static readonly Color InkColor = new Color(0.27f, 0.19f, 0.11f, 1f);
    private static readonly Color AccentColor = new Color(0.72f, 0.53f, 0.3f, 0.95f);

    private OrderSystem orderSystem;
    private TextMesh titleText;
    private TextMesh summaryText;
    private TextMesh requirementsText;
    private TextMesh rewardText;
    private SpriteRenderer referenceRenderer;

    public void Initialize(OrderSystem orders)
    {
        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged -= Refresh;
            orderSystem.OrdersChanged -= Refresh;
        }

        orderSystem = orders;

        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged += Refresh;
            orderSystem.OrdersChanged += Refresh;
        }

        EnsureVisuals();
        Refresh();
    }

    private void OnDestroy()
    {
        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged -= Refresh;
            orderSystem.OrdersChanged -= Refresh;
        }
    }

    private void EnsureVisuals()
    {
        if (titleText != null)
        {
            return;
        }

        GameObject shadow = SimpleShapeFactory.CreateRectangle(
            "PaperShadow",
            transform,
            new Vector2(PanelWidth, PanelHeight),
            PaperShadowColor,
            18);
        shadow.transform.localPosition = new Vector3(0.08f, -0.08f, 0f);
        shadow.transform.localRotation = Quaternion.Euler(0f, 0f, -1.2f);

        GameObject paper = SimpleShapeFactory.CreateRectangle(
            "CurrentOrderPaper",
            transform,
            new Vector2(PanelWidth, PanelHeight),
            PaperColor,
            19);
        paper.transform.localRotation = Quaternion.Euler(0f, 0f, -1.2f);

        CreatePaperEdge(new Vector3(0f, PanelHeight * 0.5f - 0.03f, -0.005f), new Vector2(PanelWidth - 0.06f, 0.04f), -1.2f);
        CreatePaperEdge(new Vector3(0f, -PanelHeight * 0.5f + 0.03f, -0.005f), new Vector2(PanelWidth - 0.06f, 0.04f), -1.2f);
        CreatePaperEdge(new Vector3(-PanelWidth * 0.5f + 0.03f, 0f, -0.005f), new Vector2(0.04f, PanelHeight - 0.06f), -1.2f);
        CreatePaperEdge(new Vector3(PanelWidth * 0.5f - 0.03f, 0f, -0.005f), new Vector2(0.04f, PanelHeight - 0.06f), -1.2f);

        GameObject header = SimpleShapeFactory.CreateRectangle(
            "HeaderStrip",
            transform,
            new Vector2(1.45f, 0.28f),
            AccentColor,
            20);
        header.transform.localPosition = new Vector3(-1.82f, 0.66f, -0.01f);
        header.transform.localRotation = Quaternion.Euler(0f, 0f, -1.2f);

        GameObject pin = SimpleShapeFactory.CreateCircle(
            "PaperPin",
            transform,
            0.075f,
            new Color(0.75f, 0.33f, 0.28f, 0.98f),
            21);
        pin.transform.localPosition = new Vector3(2.33f, 0.68f, -0.01f);

        titleText = CreateText("TitleText", new Vector3(-2.28f, 0.6f, -0.02f), TextAnchor.UpperLeft, TextAlignment.Left, 0.056f, true);
        summaryText = CreateText("SummaryText", new Vector3(-2.28f, 0.24f, -0.02f), TextAnchor.UpperLeft, TextAlignment.Left, 0.044f, false);
        requirementsText = CreateText("RequirementsText", new Vector3(-2.28f, -0.36f, -0.02f), TextAnchor.UpperLeft, TextAlignment.Left, 0.045f, false);
        rewardText = CreateText("RewardText", new Vector3(1.26f, 0.5f, -0.02f), TextAnchor.UpperLeft, TextAlignment.Left, 0.048f, false);

        GameObject referenceFrame = SimpleShapeFactory.CreateRectangle(
            "ReferenceFrame",
            transform,
            new Vector2(1.14f, 1.28f),
            new Color(0.98f, 0.96f, 0.9f, 1f),
            20);
        referenceFrame.transform.localPosition = new Vector3(2.05f, -0.04f, -0.012f);
        referenceFrame.transform.localRotation = Quaternion.Euler(0f, 0f, 2.5f);

        GameObject referenceShadow = SimpleShapeFactory.CreateRectangle(
            "ReferenceShadow",
            transform,
            new Vector2(1.14f, 1.28f),
            new Color(0.36f, 0.28f, 0.18f, 0.18f),
            19);
        referenceShadow.transform.localPosition = new Vector3(2.11f, -0.1f, -0.013f);
        referenceShadow.transform.localRotation = Quaternion.Euler(0f, 0f, 2.5f);

        GameObject captionLine = SimpleShapeFactory.CreateRectangle(
            "ReferenceCaptionLine",
            transform,
            new Vector2(0.72f, 0.03f),
            new Color(0.85f, 0.8f, 0.68f, 0.95f),
            21);
        captionLine.transform.localPosition = new Vector3(2.05f, -0.47f, -0.013f);
        captionLine.transform.localRotation = Quaternion.Euler(0f, 0f, 2.5f);

        GameObject referenceObject = new GameObject("ReferenceImage");
        referenceObject.transform.SetParent(transform, false);
        referenceObject.transform.localPosition = new Vector3(2.05f, 0.08f, -0.02f);
        referenceObject.transform.localRotation = Quaternion.Euler(0f, 0f, 2.5f);
        referenceRenderer = referenceObject.AddComponent<SpriteRenderer>();
        referenceRenderer.sortingOrder = 21;
        referenceRenderer.color = new Color(1f, 1f, 1f, 0.95f);
        referenceObject.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
    }

    private TextMesh CreateText(string objectName, Vector3 localPosition, TextAnchor anchor, TextAlignment alignment, float characterSize, bool boldLike)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = localPosition;
        textObject.transform.localRotation = Quaternion.Euler(0f, 0f, -1.2f);

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.anchor = anchor;
        textMesh.alignment = alignment;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = boldLike ? 56 : 48;
        textMesh.color = InkColor;

        MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 22;
        return textMesh;
    }

    private void CreatePaperEdge(Vector3 localPosition, Vector2 size, float rotationZ)
    {
        GameObject edge = SimpleShapeFactory.CreateRectangle(
            "PaperEdge",
            transform,
            size,
            new Color(0.82f, 0.72f, 0.56f, 0.4f),
            20);
        edge.transform.localPosition = localPosition;
        edge.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    private void Refresh(OrderData _)
    {
        Refresh();
    }

    private void Refresh()
    {
        if (titleText == null)
        {
            return;
        }

        if (orderSystem == null || orderSystem.SelectedOrder == null)
        {
            titleText.text = "今日订单";
            summaryText.text = "还没有选中的顾客。\n点上方订单按钮，挑一张便签开始做花束。";
            requirementsText.text = "要求会写在这里。";
            rewardText.text = string.Empty;
            referenceRenderer.enabled = false;
            return;
        }

        OrderData order = orderSystem.SelectedOrder;
        titleText.text = $"{order.CustomerName} 的委托";
        summaryText.text = $"留言\n{order.ChatSummary}";
        requirementsText.text = BuildRequirementsText(order);
        rewardText.text = order.RewardCoins > 0 ? $"报酬\n{order.RewardCoins} 金币" : string.Empty;

        if (order.ReferenceImage != null)
        {
            referenceRenderer.enabled = true;
            referenceRenderer.sprite = order.ReferenceImage;
        }
        else
        {
            referenceRenderer.enabled = false;
        }
    }

    private static string BuildRequirementsText(OrderData order)
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine("要求");

        for (int i = 0; i < order.Requirements.Count; i++)
        {
            OrderRequirement requirement = order.Requirements[i];
            if (requirement.RequirementType == OrderRequirementType.SlotColor && requirement.SlotIndex >= 0)
            {
                builder.AppendLine($"槽位 {requirement.SlotIndex + 1}：{FlowerColorPalette.GetDisplayName(requirement.Color)}");
            }
            else
            {
                builder.AppendLine($"{FlowerColorPalette.GetDisplayName(requirement.Color)} x{requirement.RequiredCount}");
            }
        }

        return builder.ToString();
    }
}
