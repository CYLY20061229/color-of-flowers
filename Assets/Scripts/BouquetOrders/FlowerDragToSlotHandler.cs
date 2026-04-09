using UnityEngine;

public class FlowerDragToSlotHandler : MonoBehaviour
{
    private FlowerColor color;
    private BouquetOrderManager bouquetOrderManager;
    private Camera mainCamera;
    private GameObject ghostObject;
    private Sprite grownSprite;

    public void Initialize(FlowerColor flowerColor, BouquetOrderManager manager)
    {
        color = flowerColor;
        bouquetOrderManager = manager;
        mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        if (!CanStartDrag())
        {
            return;
        }

        EnsureGhost();
        ghostObject.SetActive(true);
        ghostObject.transform.position = GetMouseWorldPosition();
    }

    private void OnMouseDrag()
    {
        if (ghostObject == null || !ghostObject.activeSelf)
        {
            return;
        }

        ghostObject.transform.position = GetMouseWorldPosition();
    }

    private void OnMouseUp()
    {
        if (ghostObject == null || !ghostObject.activeSelf)
        {
            return;
        }

        BouquetSlotView slotView = FindSlotUnderMouse();
        if (slotView != null)
        {
            bouquetOrderManager.TryPlaceFlower(slotView.SlotState.SlotIndex, color);
        }

        ghostObject.SetActive(false);
    }

    private bool CanStartDrag()
    {
        return bouquetOrderManager != null && bouquetOrderManager.HasActiveBouquetOrder;
    }

    private BouquetSlotView FindSlotUnderMouse()
    {
        Vector2 mousePoint = GetMouseWorldPosition();
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePoint);

        for (int i = 0; i < hits.Length; i++)
        {
            BouquetSlotView slotView = hits[i].GetComponent<BouquetSlotView>();
            if (slotView != null)
            {
                return slotView;
            }
        }

        return null;
    }

    private void EnsureGhost()
    {
        if (ghostObject != null)
        {
            return;
        }

        ghostObject = new GameObject($"DragGhost_{color}");
        SpriteRenderer renderer = ghostObject.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadGrownSprite();
        renderer.color = FlowerColorPalette.ToUnityColor(color);
        renderer.sortingOrder = 50;
        ghostObject.transform.localScale = new Vector3(0.55f, 0.55f, 1f);
        ghostObject.SetActive(false);
    }

    private Sprite LoadGrownSprite()
    {
        if (grownSprite != null)
        {
            return grownSprite;
        }

        grownSprite = Resources.Load<Sprite>("Flowers/grown");
        if (grownSprite != null)
        {
            return grownSprite;
        }

        Texture2D texture = Resources.Load<Texture2D>("Flowers/grown");
        if (texture == null)
        {
            return null;
        }

        grownSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            512f);
        return grownSprite;
    }

    private Vector3 GetMouseWorldPosition()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        if (mainCamera == null)
        {
            return transform.position;
        }

        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(mousePosition);
        worldPosition.z = 0f;
        return worldPosition;
    }
}
