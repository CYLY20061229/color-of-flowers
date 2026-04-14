using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasketDisplay : MonoBehaviour
{
    private const int DefaultSlotCount = 10;
    private const string BasketSpriteResourcePath = "Baskets/blanket";
    private const string FlowerSpriteResourcePath = "Flowers/grown";

    [Header("References")]
    [SerializeField] private SpriteRenderer basketSpriteRenderer;
    [SerializeField] private Transform flowerContainer;
    [SerializeField] private List<Transform> slotAnchors = new List<Transform>();

    [Header("Sprites")]
    [SerializeField] private Sprite basketSprite;
    [SerializeField] private Sprite flowerSprite;

    [Header("Layout")]
    [SerializeField] private bool autoCreateDefaultAnchors = true;
    [SerializeField] private int maxDisplayCount = DefaultSlotCount;
    [SerializeField] private Vector3 basketScale = new Vector3(0.94f, 0.94f, 1f);
    [SerializeField] private Vector3 flowerBaseScale = new Vector3(0.38f, 0.38f, 1f);
    [SerializeField] private Vector2 positionJitterRange = new Vector2(0.06f, 0.04f);
    [SerializeField] private Vector2 rotationJitterRange = new Vector2(-7f, 7f);
    [SerializeField] private Vector2 scaleJitterRange = new Vector2(-0.05f, 0.08f);
    [SerializeField] private int basketSortingOrder = 5;
    [SerializeField] private int flowerSortingBaseOrder = 6;
    [SerializeField] private Vector3 messageLocalPosition = new Vector3(0f, 1.05f, -0.02f);
    [SerializeField] private float messageDurationSeconds = 1.2f;

    private readonly List<BasketDisplaySlot> slots = new List<BasketDisplaySlot>();
    private readonly List<FlowerColor> displayedFlowers = new List<FlowerColor>();
    private InventorySystem inventorySystem;
    private TextMesh statusText;
    private Coroutine statusMessageRoutine;
    private BoxCollider2D basketCollider;

    public int Capacity => Mathf.Max(1, maxDisplayCount);
    public int DisplayedCount => displayedFlowers.Count;
    public bool HasEmptySlot => DisplayedCount < slots.Count;
    public IReadOnlyList<FlowerColor> DisplayedFlowers => displayedFlowers;

    public Vector3 GetHarvestTargetPosition()
    {
        if (flowerContainer != null)
        {
            return flowerContainer.position + new Vector3(0f, 0.15f, -0.1f);
        }

        return transform.position + new Vector3(0f, 0.15f, -0.1f);
    }

    public void Initialize(InventorySystem inventory)
    {
        if (inventorySystem != null)
        {
            inventorySystem.InventoryChanged -= RefreshFromInventory;
        }

        inventorySystem = inventory;
        EnsureVisuals();
        BuildSlots();

        if (inventorySystem != null)
        {
            inventorySystem.InventoryChanged += RefreshFromInventory;
        }

        RefreshFromInventory();
    }

    private void OnDestroy()
    {
        if (inventorySystem != null)
        {
            inventorySystem.InventoryChanged -= RefreshFromInventory;
        }
    }

    public bool TryAddFlowerDisplay(FlowerColor color)
    {
        BasketDisplaySlot emptySlot = FindFirstEmptySlot();
        if (emptySlot == null || emptySlot.Anchor == null || flowerSprite == null)
        {
            return false;
        }

        GameObject flowerObject = new GameObject($"BasketFlower_{color}_{displayedFlowers.Count:00}");
        flowerObject.transform.SetParent(flowerContainer, false);

        BasketFlowerView flowerView = flowerObject.AddComponent<BasketFlowerView>();
        flowerView.Initialize(
            flowerSprite,
            color,
            flowerSortingBaseOrder + displayedFlowers.Count,
            GetJitteredLocalPosition(emptySlot.Anchor.localPosition),
            Random.Range(rotationJitterRange.x, rotationJitterRange.y),
            GetJitteredScale());

        emptySlot.Occupy(new FlowerData(color), flowerView);
        displayedFlowers.Add(color);
        return true;
    }

    public bool TryRemoveFlowerDisplay(FlowerColor color)
    {
        for (int i = slots.Count - 1; i >= 0; i--)
        {
            BasketDisplaySlot slot = slots[i];
            if (!slot.IsOccupied || slot.FlowerData == null || slot.FlowerData.Color != color)
            {
                continue;
            }

            if (slot.FlowerView != null)
            {
                Destroy(slot.FlowerView.gameObject);
            }

            slot.Release();
            displayedFlowers.Remove(color);
            return true;
        }

        return false;
    }

    public void ClearDisplay()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            BasketDisplaySlot slot = slots[i];
            if (slot.FlowerView != null)
            {
                Destroy(slot.FlowerView.gameObject);
            }

            slot.Release();
        }

        displayedFlowers.Clear();
    }

    public void RefreshFromInventory()
    {
        ClearDisplay();
        if (inventorySystem == null)
        {
            return;
        }

        for (int i = 0; i < inventorySystem.Flowers.Count && i < slots.Count; i++)
        {
            TryAddFlowerDisplay(inventorySystem.Flowers[i].Color);
        }
    }

    public void ShowStatusMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        EnsureStatusText();
        statusText.text = message;
        statusText.gameObject.SetActive(true);

        if (statusMessageRoutine != null)
        {
            StopCoroutine(statusMessageRoutine);
        }

        statusMessageRoutine = StartCoroutine(HideStatusMessageAfterDelay());
    }

    private void EnsureVisuals()
    {
        if (basketSprite == null)
        {
            basketSprite = LoadSprite(BasketSpriteResourcePath);
        }

        if (flowerSprite == null)
        {
            flowerSprite = LoadSprite(FlowerSpriteResourcePath);
        }

        if (basketSpriteRenderer == null)
        {
            GameObject basketObject = new GameObject("BasketSprite");
            basketObject.transform.SetParent(transform, false);
            basketSpriteRenderer = basketObject.AddComponent<SpriteRenderer>();
        }

        basketSpriteRenderer.sprite = basketSprite;
        basketSpriteRenderer.sortingOrder = basketSortingOrder;
        basketSpriteRenderer.color = Color.white;
        basketSpriteRenderer.transform.localScale = basketScale;
        EnsureDropCollider();

        if (flowerContainer == null)
        {
            GameObject flowerContainerObject = new GameObject("FlowerContainer");
            flowerContainerObject.transform.SetParent(transform, false);
            flowerContainer = flowerContainerObject.transform;
        }

        EnsureStatusText();
    }

    private void BuildSlots()
    {
        slots.Clear();

        if ((slotAnchors == null || slotAnchors.Count == 0) && autoCreateDefaultAnchors)
        {
            CreateDefaultAnchors();
        }

        if (slotAnchors == null)
        {
            slotAnchors = new List<Transform>();
        }

        for (int i = 0; i < slotAnchors.Count && i < Capacity; i++)
        {
            if (slotAnchors[i] != null)
            {
                slots.Add(new BasketDisplaySlot(slotAnchors[i]));
            }
        }
    }

    private void CreateDefaultAnchors()
    {
        slotAnchors = new List<Transform>(Capacity);
        Vector3[] defaultPositions =
        {
            new Vector3(-0.95f, 0.18f, 0f),
            new Vector3(-0.35f, 0.32f, 0f),
            new Vector3(0.3f, 0.34f, 0f),
            new Vector3(0.92f, 0.18f, 0f),
            new Vector3(-0.78f, -0.12f, 0f),
            new Vector3(-0.22f, -0.04f, 0f),
            new Vector3(0.34f, -0.02f, 0f),
            new Vector3(0.86f, -0.12f, 0f),
            new Vector3(-0.4f, -0.36f, 0f),
            new Vector3(0.28f, -0.38f, 0f)
        };

        for (int i = 0; i < Capacity && i < defaultPositions.Length; i++)
        {
            GameObject anchorObject = new GameObject($"SlotAnchor_{i + 1:00}");
            anchorObject.transform.SetParent(transform, false);
            anchorObject.transform.localPosition = defaultPositions[i];
            slotAnchors.Add(anchorObject.transform);
        }
    }

    private BasketDisplaySlot FindFirstEmptySlot()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            if (!slots[i].IsOccupied)
            {
                return slots[i];
            }
        }

        return null;
    }

    private Vector3 GetJitteredLocalPosition(Vector3 anchorLocalPosition)
    {
        return anchorLocalPosition + new Vector3(
            Random.Range(-positionJitterRange.x, positionJitterRange.x),
            Random.Range(-positionJitterRange.y, positionJitterRange.y),
            0f);
    }

    private Vector3 GetJitteredScale()
    {
        float scaleFactor = 1f + Random.Range(scaleJitterRange.x, scaleJitterRange.y);
        return flowerBaseScale * scaleFactor;
    }

    private Sprite LoadSprite(string resourcePath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(resourcePath);
        if (texture == null)
        {
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            512f);
    }

    private void EnsureStatusText()
    {
        if (statusText != null)
        {
            return;
        }

        GameObject statusObject = new GameObject("StatusText");
        statusObject.transform.SetParent(transform, false);
        statusObject.transform.localPosition = messageLocalPosition;

        statusText = statusObject.AddComponent<TextMesh>();
        statusText.anchor = TextAnchor.MiddleCenter;
        statusText.alignment = TextAlignment.Center;
        statusText.characterSize = 0.11f;
        statusText.fontSize = 48;
        statusText.color = new Color(1f, 0.94f, 0.78f, 1f);
        statusText.text = string.Empty;

        MeshRenderer renderer = statusObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = flowerSortingBaseOrder + Capacity + 4;
        statusObject.SetActive(false);
    }

    private void EnsureDropCollider()
    {
        if (basketCollider == null)
        {
            basketCollider = GetComponent<BoxCollider2D>();
            if (basketCollider == null)
            {
                basketCollider = gameObject.AddComponent<BoxCollider2D>();
            }
        }

        basketCollider.isTrigger = true;
        if (basketSprite != null)
        {
            basketCollider.size = basketSprite.bounds.size;
            basketCollider.offset = basketSprite.bounds.center;
        }
    }

    private IEnumerator HideStatusMessageAfterDelay()
    {
        yield return new WaitForSeconds(messageDurationSeconds);

        if (statusText != null)
        {
            statusText.text = string.Empty;
            statusText.gameObject.SetActive(false);
        }

        statusMessageRoutine = null;
    }
}
