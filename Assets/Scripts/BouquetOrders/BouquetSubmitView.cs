using UnityEngine;

public class BouquetSubmitView : MonoBehaviour
{
    private const float PanelWidth = 2.15f;
    private const float PanelHeight = 0.56f;

    private BouquetOrderManager bouquetOrderManager;
    private TextMesh label;
    private GameObject panel;

    public void Initialize(BouquetOrderManager manager)
    {
        if (bouquetOrderManager != null)
        {
            bouquetOrderManager.BouquetOrderChanged -= Refresh;
        }

        bouquetOrderManager = manager;
        if (bouquetOrderManager != null)
        {
            bouquetOrderManager.BouquetOrderChanged += Refresh;
        }

        EnsureVisuals();
        Refresh();
    }

    private void OnDestroy()
    {
        if (bouquetOrderManager != null)
        {
            bouquetOrderManager.BouquetOrderChanged -= Refresh;
        }
    }

    private void OnMouseDown()
    {
        if (bouquetOrderManager == null)
        {
            return;
        }

        bouquetOrderManager.TrySubmitActiveOrder();
    }

    private void EnsureVisuals()
    {
        if (label != null)
        {
            return;
        }

        panel = SimpleShapeFactory.CreateRectangle("SubmitPanel", transform, new Vector2(PanelWidth, PanelHeight), new Color(0.2f, 0.35f, 0.24f, 0.92f), 22);

        BoxCollider2D collider = gameObject.GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
        collider.size = new Vector2(PanelWidth, PanelHeight);

        GameObject labelObject = new GameObject("SubmitLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.05f;
        label.fontSize = 48;
        label.color = Color.white;

        MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 23;
    }

    private void Refresh()
    {
        if (label == null)
        {
            return;
        }

        bool hasActiveOrder = bouquetOrderManager != null && bouquetOrderManager.ActiveOrder != null;
        label.text = hasActiveOrder ? "提交订单" : "选择订单";

        if (panel != null)
        {
            Color color = hasActiveOrder
                ? new Color(0.2f, 0.35f, 0.24f, 0.92f)
                : new Color(0.15f, 0.16f, 0.18f, 0.88f);
            SimpleShapeFactory.SetColor(panel, color);
        }
    }
}
