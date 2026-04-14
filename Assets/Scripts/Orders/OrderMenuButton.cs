using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OrderMenuButton : MonoBehaviour
{
    private const float ButtonWidth = 1.12f;
    private const float ButtonHeight = 0.44f;

    private static readonly Color PaperColor = new Color(0.95f, 0.89f, 0.75f, 0.98f);
    private static readonly Color ShadowColor = new Color(0.32f, 0.23f, 0.14f, 0.22f);
    private static readonly Color InkColor = new Color(0.28f, 0.19f, 0.11f, 1f);

    private OrderCarouselView carouselView;
    private TextMesh label;
    private GameObject background;

    public void Initialize(OrderCarouselView targetCarouselView)
    {
        carouselView = targetCarouselView;
        EnsureVisuals();
    }

    private void OnMouseDown()
    {
        carouselView?.ShowAtCurrentSelection();
    }

    private void EnsureVisuals()
    {
        if (label != null)
        {
            return;
        }

        GameObject shadow = SimpleShapeFactory.CreateRectangle(
            "OrderMenuShadow",
            transform,
            new Vector2(ButtonWidth, ButtonHeight),
            ShadowColor,
            26);
        shadow.transform.localPosition = new Vector3(0.05f, -0.05f, 0f);
        shadow.transform.localRotation = Quaternion.Euler(0f, 0f, -4f);

        background = SimpleShapeFactory.CreateRectangle(
            "OrderMenuButton",
            transform,
            new Vector2(ButtonWidth, ButtonHeight),
            PaperColor,
            27);
        background.transform.localRotation = Quaternion.Euler(0f, 0f, -4f);

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(ButtonWidth, ButtonHeight);

        GameObject labelObject = new GameObject("Label");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);
        labelObject.transform.localRotation = Quaternion.Euler(0f, 0f, -4f);

        label = labelObject.AddComponent<TextMesh>();
        label.text = "订单";
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.045f;
        label.fontSize = 48;
        label.color = InkColor;

        MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 28;
    }
}
