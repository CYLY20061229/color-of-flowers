using UnityEngine;

public class FlowerDragToSlotHandler : MonoBehaviour
{
    private const float SnapDistance = 1.2f;

    private FlowerColor color;
    private BouquetOrderManager bouquetOrderManager;
    private Camera mainCamera;
    private GameObject ghostObject;
    private Sprite grownSprite;
    private BouquetSlotView previewSlot;

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
        UpdatePreviewSlot(ghostObject.transform.position);
    }

    private void OnMouseDrag()
    {
        if (ghostObject == null || !ghostObject.activeSelf)
        {
            return;
        }

        ghostObject.transform.position = GetMouseWorldPosition();
        UpdatePreviewSlot(ghostObject.transform.position);
    }

    private void OnMouseUp()
    {
        if (ghostObject == null || !ghostObject.activeSelf)
        {
            return;
        }

        BouquetSlotView slotView = FindBestSlot(GetMouseWorldPosition());
        if (slotView != null)
        {
            bouquetOrderManager.TryPlaceFlower(slotView.SlotState.SlotIndex, color);
        }

        ClearPreviewSlot();
        ghostObject.SetActive(false);
    }

    private bool CanStartDrag()
    {
        return bouquetOrderManager != null && bouquetOrderManager.HasActiveBouquetOrder;
    }

    private BouquetSlotView FindBestSlot(Vector3 worldPosition)
    {
        Vector2 mousePoint = worldPosition;
        Collider2D[] hits = Physics2D.OverlapPointAll(mousePoint);

        for (int i = 0; i < hits.Length; i++)
        {
            BouquetSlotView slotView = hits[i].GetComponent<BouquetSlotView>();
            if (slotView != null)
            {
                return slotView;
            }
        }

        return BouquetSlotView.FindClosestSlot(worldPosition, SnapDistance);
    }

    private void UpdatePreviewSlot(Vector3 worldPosition)
    {
        BouquetSlotView nextSlot = FindBestSlot(worldPosition);
        if (nextSlot == previewSlot)
        {
            return;
        }

        if (previewSlot != null)
        {
            previewSlot.SetPreviewHighlight(false);
        }

        previewSlot = nextSlot;
        if (previewSlot != null)
        {
            previewSlot.SetPreviewHighlight(true);
        }
    }

    private void ClearPreviewSlot()
    {
        if (previewSlot != null)
        {
            previewSlot.SetPreviewHighlight(false);
            previewSlot = null;
        }
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
