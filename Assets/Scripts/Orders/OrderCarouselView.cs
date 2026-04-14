using System.Text;
using UnityEngine;

public class OrderCarouselView : MonoBehaviour
{
    private const float PanelWidth = 5.0f;
    private const float PanelHeight = 4.85f;

    private static readonly Color PaperColor = new Color(0.96f, 0.9f, 0.76f, 0.985f);
    private static readonly Color PaperShadowColor = new Color(0.34f, 0.25f, 0.16f, 0.26f);
    private static readonly Color InkColor = new Color(0.28f, 0.19f, 0.11f, 1f);
    private static readonly Color TapeColor = new Color(0.96f, 0.88f, 0.62f, 0.82f);
    private static readonly Color ButtonPaperColor = new Color(0.92f, 0.84f, 0.66f, 0.98f);

    private OrderSystem orderSystem;
    private int currentIndex;

    private TextMesh titleText;
    private TextMesh customerText;
    private TextMesh summaryText;
    private TextMesh requirementsText;
    private TextMesh pageText;
    private TextMesh emptyText;
    private SpriteRenderer referenceRenderer;

    public void Initialize(OrderSystem orders)
    {
        if (orderSystem != null)
        {
            orderSystem.OrdersChanged -= HandleOrdersChanged;
        }

        orderSystem = orders;
        if (orderSystem != null)
        {
            orderSystem.OrdersChanged += HandleOrdersChanged;
        }

        EnsureVisuals();
        Hide();
    }

    private void OnDestroy()
    {
        if (orderSystem != null)
        {
            orderSystem.OrdersChanged -= HandleOrdersChanged;
        }
    }

    public void ShowAtCurrentSelection()
    {
        SyncIndexToSelectedOrder();
        gameObject.SetActive(true);
        Refresh();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void ShowPrevious()
    {
        int count = GetOrderCount();
        if (count == 0)
        {
            return;
        }

        currentIndex = (currentIndex - 1 + count) % count;
        Refresh();
    }

    public void ShowNext()
    {
        int count = GetOrderCount();
        if (count == 0)
        {
            return;
        }

        currentIndex = (currentIndex + 1) % count;
        Refresh();
    }

    public void SelectCurrentOrder()
    {
        OrderData order = GetCurrentOrder();
        if (order == null || orderSystem == null)
        {
            return;
        }

        orderSystem.SelectOrder(order);
        Hide();
    }

    private void HandleOrdersChanged()
    {
        ClampIndex();
        if (gameObject.activeSelf)
        {
            Refresh();
        }
    }

    private void EnsureVisuals()
    {
        if (titleText != null)
        {
            return;
        }

        GameObject shadow = SimpleShapeFactory.CreateRectangle(
            "CarouselShadow",
            transform,
            new Vector2(PanelWidth, PanelHeight),
            PaperShadowColor,
            40);
        shadow.transform.localPosition = new Vector3(0.1f, -0.08f, 0f);
        shadow.transform.localRotation = Quaternion.Euler(0f, 0f, 1.5f);

        GameObject paper = SimpleShapeFactory.CreateRectangle(
            "CarouselPaper",
            transform,
            new Vector2(PanelWidth, PanelHeight),
            PaperColor,
            41);
        paper.transform.localRotation = Quaternion.Euler(0f, 0f, 1.5f);

        CreatePaperEdge(new Vector3(0f, PanelHeight * 0.5f - 0.03f, -0.005f), new Vector2(PanelWidth - 0.06f, 0.04f), 1.5f);
        CreatePaperEdge(new Vector3(0f, -PanelHeight * 0.5f + 0.03f, -0.005f), new Vector2(PanelWidth - 0.06f, 0.04f), 1.5f);
        CreatePaperEdge(new Vector3(-PanelWidth * 0.5f + 0.03f, 0f, -0.005f), new Vector2(0.04f, PanelHeight - 0.06f), 1.5f);
        CreatePaperEdge(new Vector3(PanelWidth * 0.5f - 0.03f, 0f, -0.005f), new Vector2(0.04f, PanelHeight - 0.06f), 1.5f);

        GameObject tapeLeft = SimpleShapeFactory.CreateRectangle(
            "TapeLeft",
            transform,
            new Vector2(0.82f, 0.24f),
            TapeColor,
            42);
        tapeLeft.transform.localPosition = new Vector3(-1.55f, 2.15f, -0.01f);
        tapeLeft.transform.localRotation = Quaternion.Euler(0f, 0f, -24f);

        GameObject tapeRight = SimpleShapeFactory.CreateRectangle(
            "TapeRight",
            transform,
            new Vector2(0.82f, 0.24f),
            TapeColor,
            42);
        tapeRight.transform.localPosition = new Vector3(1.55f, 2.15f, -0.01f);
        tapeRight.transform.localRotation = Quaternion.Euler(0f, 0f, 22f);

        titleText = CreateText("TitleText", "订单合集", new Vector3(0f, 2.02f, -0.02f), 0.075f, TextAnchor.MiddleCenter, TextAlignment.Center, 44);
        customerText = CreateText("CustomerText", string.Empty, new Vector3(0f, 1.55f, -0.02f), 0.06f, TextAnchor.MiddleCenter, TextAlignment.Center, 44);
        summaryText = CreateText("SummaryText", string.Empty, new Vector3(-1.9f, 1.1f, -0.02f), 0.048f, TextAnchor.UpperLeft, TextAlignment.Left, 44);
        requirementsText = CreateText("RequirementsText", string.Empty, new Vector3(-1.9f, -0.08f, -0.02f), 0.048f, TextAnchor.UpperLeft, TextAlignment.Left, 44);
        pageText = CreateText("PageText", string.Empty, new Vector3(0f, -1.8f, -0.02f), 0.045f, TextAnchor.MiddleCenter, TextAlignment.Center, 44);
        emptyText = CreateText("EmptyText", "暂时没有新订单", new Vector3(0f, 0f, -0.02f), 0.06f, TextAnchor.MiddleCenter, TextAlignment.Center, 44);

        GameObject referenceFrame = SimpleShapeFactory.CreateRectangle(
            "ReferenceFrame",
            transform,
            new Vector2(1.26f, 1.48f),
            new Color(0.98f, 0.96f, 0.9f, 1f),
            42);
        referenceFrame.transform.localPosition = new Vector3(1.62f, 0.5f, -0.015f);
        referenceFrame.transform.localRotation = Quaternion.Euler(0f, 0f, -3.5f);

        GameObject referenceShadow = SimpleShapeFactory.CreateRectangle(
            "ReferenceShadow",
            transform,
            new Vector2(1.26f, 1.48f),
            new Color(0.36f, 0.28f, 0.18f, 0.16f),
            41);
        referenceShadow.transform.localPosition = new Vector3(1.7f, 0.42f, -0.016f);
        referenceShadow.transform.localRotation = Quaternion.Euler(0f, 0f, -3.5f);

        GameObject captionLine = SimpleShapeFactory.CreateRectangle(
            "ReferenceCaptionLine",
            transform,
            new Vector2(0.8f, 0.03f),
            new Color(0.85f, 0.8f, 0.68f, 0.95f),
            43);
        captionLine.transform.localPosition = new Vector3(1.62f, -0.05f, -0.016f);
        captionLine.transform.localRotation = Quaternion.Euler(0f, 0f, -3.5f);

        GameObject referenceObject = new GameObject("ReferenceImage");
        referenceObject.transform.SetParent(transform, false);
        referenceObject.transform.localPosition = new Vector3(1.62f, 0.66f, -0.02f);
        referenceObject.transform.localRotation = Quaternion.Euler(0f, 0f, -3.5f);
        referenceRenderer = referenceObject.AddComponent<SpriteRenderer>();
        referenceRenderer.sortingOrder = 43;
        referenceRenderer.color = Color.white;
        referenceObject.transform.localScale = new Vector3(0.58f, 0.58f, 1f);

        CreateButton("PrevButton", "上一张", new Vector3(-1.45f, -1.28f, -0.02f), new Vector2(1.08f, 0.48f), OrderCarouselAction.Previous);
        CreateButton("NextButton", "下一张", new Vector3(1.45f, -1.28f, -0.02f), new Vector2(1.08f, 0.48f), OrderCarouselAction.Next);
        CreateButton("SelectButton", "选择这张", new Vector3(0f, -1.28f, -0.02f), new Vector2(1.35f, 0.48f), OrderCarouselAction.Select);
        CreateButton("CloseButton", "收起", new Vector3(1.95f, 2.03f, -0.02f), new Vector2(0.74f, 0.4f), OrderCarouselAction.Close);
    }

    private void CreateButton(string objectName, string labelText, Vector3 localPosition, Vector2 size, OrderCarouselAction action)
    {
        GameObject buttonObject = new GameObject(objectName);
        buttonObject.transform.SetParent(transform, false);
        buttonObject.transform.localPosition = localPosition;
        buttonObject.transform.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-1.5f, 1.5f));

        SimpleShapeFactory.CreateRectangle("ButtonBg", buttonObject.transform, size, ButtonPaperColor, 42);

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(buttonObject.transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        TextMesh textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.text = labelText;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.045f;
        textMesh.fontSize = 48;
        textMesh.color = InkColor;

        MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 43;

        OrderCarouselButton button = buttonObject.AddComponent<OrderCarouselButton>();
        button.Initialize(this, action, size);
    }

    private TextMesh CreateText(string objectName, string content, Vector3 localPosition, float characterSize, TextAnchor anchor, TextAlignment alignment, int sortingOrder)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = localPosition;
        textObject.transform.localRotation = Quaternion.Euler(0f, 0f, 1.5f);

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = content;
        textMesh.anchor = anchor;
        textMesh.alignment = alignment;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 48;
        textMesh.color = InkColor;

        MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = sortingOrder;
        return textMesh;
    }

    private void CreatePaperEdge(Vector3 localPosition, Vector2 size, float rotationZ)
    {
        GameObject edge = SimpleShapeFactory.CreateRectangle(
            "PaperEdge",
            transform,
            size,
            new Color(0.82f, 0.72f, 0.56f, 0.38f),
            42);
        edge.transform.localPosition = localPosition;
        edge.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
    }

    private void Refresh()
    {
        OrderData order = GetCurrentOrder();
        bool hasOrder = order != null;

        emptyText.gameObject.SetActive(!hasOrder);
        customerText.gameObject.SetActive(hasOrder);
        summaryText.gameObject.SetActive(hasOrder);
        requirementsText.gameObject.SetActive(hasOrder);
        pageText.gameObject.SetActive(hasOrder);
        referenceRenderer.enabled = hasOrder && order.ReferenceImage != null;

        if (!hasOrder)
        {
            customerText.text = string.Empty;
            summaryText.text = string.Empty;
            requirementsText.text = string.Empty;
            pageText.text = string.Empty;
            return;
        }

        customerText.text = order.CustomerName;
        summaryText.text = $"留言\n{order.ChatSummary}";
        requirementsText.text = BuildRequirementsText(order);
        pageText.text = $"{currentIndex + 1} / {GetOrderCount()}";

        if (order.ReferenceImage != null)
        {
            referenceRenderer.sprite = order.ReferenceImage;
        }
    }

    private string BuildRequirementsText(OrderData order)
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

        if (order.RewardCoins > 0)
        {
            builder.AppendLine();
            builder.Append($"报酬：{order.RewardCoins} 金币");
        }

        return builder.ToString();
    }

    private void SyncIndexToSelectedOrder()
    {
        if (orderSystem == null || orderSystem.SelectedOrder == null)
        {
            ClampIndex();
            return;
        }

        for (int i = 0; i < orderSystem.ActiveOrders.Count; i++)
        {
            if (orderSystem.ActiveOrders[i] == orderSystem.SelectedOrder)
            {
                currentIndex = i;
                return;
            }
        }

        ClampIndex();
    }

    private void ClampIndex()
    {
        int count = GetOrderCount();
        if (count <= 0)
        {
            currentIndex = 0;
            return;
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, count - 1);
    }

    private OrderData GetCurrentOrder()
    {
        if (orderSystem == null || orderSystem.ActiveOrders.Count == 0)
        {
            return null;
        }

        ClampIndex();
        return orderSystem.ActiveOrders[currentIndex];
    }

    private int GetOrderCount()
    {
        return orderSystem != null ? orderSystem.ActiveOrders.Count : 0;
    }
}
