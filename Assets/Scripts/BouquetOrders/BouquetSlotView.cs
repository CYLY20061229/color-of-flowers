using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class BouquetSlotView : MonoBehaviour
{
    private const float SlotSize = 0.72f;
    private const string FlowerSpriteResourcePath = "Flowers/grown";
    private static readonly Color OptionalSlotColor = new Color(1f, 1f, 1f, 0.14f);
    private static readonly Color RequiredSlotColor = new Color(1f, 0.88f, 0.42f, 0.2f);
    private static readonly Color RequiredHintColor = new Color(1f, 0.95f, 0.55f, 0.45f);
    private static readonly Color OptionalHighlightColor = new Color(1f, 1f, 1f, 0.28f);
    private static readonly Color RequiredHighlightColor = new Color(1f, 0.92f, 0.48f, 0.38f);
    private static readonly List<BouquetSlotView> ActiveSlotViews = new List<BouquetSlotView>();

    private BouquetSlotState slotState;
    private BouquetOrderManager bouquetOrderManager;
    private GameObject slotBackground;
    private GameObject targetHint;
    private GameObject placedFlowerVisual;
    private TextMesh label;
    private SpriteRenderer placedFlowerRenderer;
    private Sprite flowerSprite;
    private bool isPreviewHighlighted;

    public BouquetSlotState SlotState => slotState;

    private void OnEnable()
    {
        if (!ActiveSlotViews.Contains(this))
        {
            ActiveSlotViews.Add(this);
        }
    }

    private void OnDisable()
    {
        ActiveSlotViews.Remove(this);
    }

    private void OnDestroy()
    {
        ActiveSlotViews.Remove(this);
    }

    public void Initialize(BouquetSlotState state, BouquetOrderManager manager)
    {
        slotState = state;
        bouquetOrderManager = manager;
        EnsureVisuals();
        EnsureReturnDragHandler();
        Refresh();
    }

    public void Refresh()
    {
        if (slotState == null)
        {
            return;
        }

        bool filled = slotState.IsFilled;
        bool isRequired = slotState.Requirement != null && slotState.Requirement.IsRequired;

        placedFlowerVisual.SetActive(filled);
        if (filled)
        {
            placedFlowerRenderer.sprite = flowerSprite;
            placedFlowerRenderer.color = FlowerColorPalette.ToUnityColor(slotState.CurrentFlowerData.Color);
            placedFlowerVisual.transform.localRotation = Quaternion.Euler(0f, 0f, GetRotationForSlot(slotState.SlotIndex));
            placedFlowerVisual.transform.localScale = GetScaleForSlot(slotState.SlotIndex);
        }

        SimpleShapeFactory.SetColor(
            slotBackground,
            isPreviewHighlighted
                ? (isRequired ? RequiredHighlightColor : OptionalHighlightColor)
                : (isRequired ? RequiredSlotColor : OptionalSlotColor));

        SimpleShapeFactory.SetColor(
            targetHint,
            isPreviewHighlighted
                ? new Color(1f, 0.98f, 0.7f, 0.78f)
                : (isRequired ? RequiredHintColor : new Color(1f, 1f, 1f, 0f)));

        if (label != null)
        {
            label.text = isRequired ? $"位{slotState.SlotIndex}" : slotState.SlotIndex.ToString();
            label.color = filled ? new Color(1f, 1f, 1f, 0.45f) : new Color(1f, 1f, 1f, 0.78f);
        }

        SetVisualActive(slotBackground, true);
        SetVisualActive(targetHint, isRequired || isPreviewHighlighted);
        SetVisualActive(label != null ? label.gameObject : null, true);
    }

    public void SetPreviewHighlight(bool highlighted)
    {
        if (isPreviewHighlighted == highlighted)
        {
            return;
        }

        isPreviewHighlighted = highlighted;
        Refresh();
    }

    public static BouquetSlotView FindClosestSlot(Vector3 worldPosition, float maxDistance)
    {
        BouquetSlotView closestSlot = null;
        float closestDistance = maxDistance;

        for (int i = 0; i < ActiveSlotViews.Count; i++)
        {
            BouquetSlotView slotView = ActiveSlotViews[i];
            if (slotView == null || !slotView.isActiveAndEnabled || slotView.SlotState == null)
            {
                continue;
            }

            float distance = Vector2.Distance(worldPosition, slotView.transform.position);
            if (distance > closestDistance)
            {
                continue;
            }

            closestDistance = distance;
            closestSlot = slotView;
        }

        return closestSlot;
    }

    private void EnsureVisuals()
    {
        if (slotBackground != null)
        {
            return;
        }

        flowerSprite = LoadFlowerSprite();
        slotBackground = SimpleShapeFactory.CreateRectangle("SlotBackground", transform, new Vector2(SlotSize, SlotSize), OptionalSlotColor, 21);
        targetHint = SimpleShapeFactory.CreateCircle("TargetColorHint", transform, 0.16f, RequiredHintColor, 22);
        placedFlowerVisual = new GameObject("PlacedFlowerVisual");
        placedFlowerVisual.transform.SetParent(transform, false);
        placedFlowerRenderer = placedFlowerVisual.AddComponent<SpriteRenderer>();
        placedFlowerRenderer.sortingOrder = 23;
        placedFlowerVisual.transform.localPosition = GetLocalOffsetForSlot(slotState != null ? slotState.SlotIndex : 0);
        placedFlowerVisual.SetActive(false);

        GameObject labelObject = new GameObject("SlotLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, -0.42f, -0.01f);

        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.05f;
        label.fontSize = 48;
        label.color = Color.white;

        MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 24;

        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(SlotSize, SlotSize);
        collider.offset = Vector2.zero;
    }

    private void EnsureReturnDragHandler()
    {
        if (bouquetOrderManager == null)
        {
            return;
        }

        SlotFlowerReturnDragHandler dragHandler = GetComponent<SlotFlowerReturnDragHandler>();
        if (dragHandler == null)
        {
            dragHandler = gameObject.AddComponent<SlotFlowerReturnDragHandler>();
        }

        dragHandler.Initialize(this, bouquetOrderManager);
    }

    private static void SetVisualActive(GameObject target, bool active)
    {
        if (target != null && target.activeSelf != active)
        {
            target.SetActive(active);
        }
    }

    private Vector3 GetLocalOffsetForSlot(int slotIndex)
    {
        switch (slotIndex % 5)
        {
            case 0:
                return new Vector3(0f, 0.08f, 0f);
            case 1:
                return new Vector3(-0.05f, 0.12f, 0f);
            case 2:
                return new Vector3(0.06f, 0.04f, 0f);
            case 3:
                return new Vector3(-0.04f, -0.02f, 0f);
            default:
                return new Vector3(0.05f, -0.05f, 0f);
        }
    }

    private float GetRotationForSlot(int slotIndex)
    {
        switch (slotIndex % 5)
        {
            case 0:
                return -4f;
            case 1:
                return 5f;
            case 2:
                return -6f;
            case 3:
                return 3f;
            default:
                return -2f;
        }
    }

    private Vector3 GetScaleForSlot(int slotIndex)
    {
        switch (slotIndex % 5)
        {
            case 0:
                return new Vector3(0.29f, 0.29f, 1f);
            case 1:
                return new Vector3(0.3f, 0.3f, 1f);
            case 2:
                return new Vector3(0.32f, 0.32f, 1f);
            case 3:
                return new Vector3(0.29f, 0.29f, 1f);
            default:
                return new Vector3(0.3f, 0.3f, 1f);
        }
    }

    private Sprite LoadFlowerSprite()
    {
        Sprite sprite = Resources.Load<Sprite>(FlowerSpriteResourcePath);
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(FlowerSpriteResourcePath);
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
}
