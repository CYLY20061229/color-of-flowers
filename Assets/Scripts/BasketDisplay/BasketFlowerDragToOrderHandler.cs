using UnityEngine;

[RequireComponent(typeof(BasketFlowerView))]
public class BasketFlowerDragToOrderHandler : MonoBehaviour
{
    private const float SnapDistance = 1.2f;

    private BasketFlowerView flowerView;
    private Camera mainCamera;
    private GameObject ghostObject;
    private SpriteRenderer ghostRenderer;
    private BouquetSlotView previewSlot;

    private void Awake()
    {
        flowerView = GetComponent<BasketFlowerView>();
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

        BouquetOrderManager bouquetOrders = GameManager.Instance != null ? GameManager.Instance.BouquetOrders : null;
        BouquetSlotView slotView = FindBestSlot(GetMouseWorldPosition());
        if (bouquetOrders != null && slotView != null)
        {
            bouquetOrders.TryPlaceFlower(slotView.SlotState.SlotIndex, flowerView.Color);
        }

        ClearPreviewSlot();
        ghostObject.SetActive(false);
    }

    private bool CanStartDrag()
    {
        return flowerView != null
            && GameManager.Instance != null
            && GameManager.Instance.BouquetOrders != null
            && GameManager.Instance.BouquetOrders.HasActiveBouquetOrder;
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

        ghostObject = new GameObject($"BasketDragGhost_{flowerView.Color}");
        ghostRenderer = ghostObject.AddComponent<SpriteRenderer>();
        ghostRenderer.sortingOrder = 50;
        ghostObject.SetActive(false);
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

    private void LateUpdate()
    {
        if (ghostRenderer == null || ghostObject == null || !ghostObject.activeSelf || flowerView == null)
        {
            return;
        }

        ghostRenderer.sprite = flowerView.CurrentSprite;
        ghostRenderer.color = FlowerColorPalette.ToUnityColor(flowerView.Color);
        ghostObject.transform.localScale = flowerView.CurrentScale;
    }
}
