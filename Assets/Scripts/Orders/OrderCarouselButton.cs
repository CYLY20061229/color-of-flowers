using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class OrderCarouselButton : MonoBehaviour
{
    private OrderCarouselView carouselView;
    private OrderCarouselAction action;

    public void Initialize(OrderCarouselView targetCarouselView, OrderCarouselAction targetAction, Vector2 colliderSize)
    {
        carouselView = targetCarouselView;
        action = targetAction;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = colliderSize;
    }

    private void OnMouseDown()
    {
        if (carouselView == null)
        {
            return;
        }

        switch (action)
        {
            case OrderCarouselAction.Previous:
                carouselView.ShowPrevious();
                break;
            case OrderCarouselAction.Next:
                carouselView.ShowNext();
                break;
            case OrderCarouselAction.Select:
                carouselView.SelectCurrentOrder();
                break;
            case OrderCarouselAction.Close:
                carouselView.Hide();
                break;
        }
    }
}
