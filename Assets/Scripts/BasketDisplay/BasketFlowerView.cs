using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BasketFlowerView : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private BoxCollider2D interactionCollider;

    public FlowerColor Color { get; private set; }
    public Sprite CurrentSprite => spriteRenderer != null ? spriteRenderer.sprite : null;
    public Vector3 CurrentScale => transform.localScale;

    public void Initialize(Sprite flowerSprite, FlowerColor flowerColor, int sortingOrder, Vector3 localPosition, float rotationDegrees, Vector3 localScale)
    {
        EnsureRenderer();
        EnsureCollider();
        Color = flowerColor;

        spriteRenderer.sprite = flowerSprite;
        spriteRenderer.color = FlowerColorPalette.ToUnityColor(flowerColor);
        spriteRenderer.sortingOrder = sortingOrder;

        transform.localPosition = localPosition;
        transform.localRotation = Quaternion.Euler(0f, 0f, rotationDegrees);
        transform.localScale = localScale;

        if (flowerSprite != null)
        {
            interactionCollider.size = flowerSprite.bounds.size;
            interactionCollider.offset = flowerSprite.bounds.center;
        }

        EnsureDragHandler();
    }

    private void EnsureRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }
    }

    private void EnsureCollider()
    {
        if (interactionCollider == null)
        {
            interactionCollider = GetComponent<BoxCollider2D>();
            if (interactionCollider == null)
            {
                interactionCollider = gameObject.AddComponent<BoxCollider2D>();
            }
        }

        interactionCollider.isTrigger = true;
    }

    private void EnsureDragHandler()
    {
        BasketFlowerDragToOrderHandler dragHandler = GetComponent<BasketFlowerDragToOrderHandler>();
        if (dragHandler == null)
        {
            gameObject.AddComponent<BasketFlowerDragToOrderHandler>();
        }
    }
}
