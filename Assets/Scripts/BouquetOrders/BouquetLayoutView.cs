using System.Collections.Generic;
using UnityEngine;

public class BouquetLayoutView : MonoBehaviour
{
    private const float PanelWidth = 2.8f;
    private const float PanelHeight = 2.6f;
    private const string BouquetBaseSpriteResourcePath = "Bouquets/bouquet";

    private BouquetOrderManager bouquetOrderManager;
    private TextMesh titleText;
    private TextMesh hintText;
    private Transform slotRoot;
    private readonly List<BouquetSlotView> slotViews = new List<BouquetSlotView>();
    private SpriteRenderer bouquetBaseRenderer;
    private Sprite bouquetBaseSprite;

    public void Initialize(BouquetOrderManager manager)
    {
        if (bouquetOrderManager != null)
        {
            bouquetOrderManager.BouquetOrderChanged -= Refresh;
        }

        bouquetOrderManager = manager;
        bouquetOrderManager.BouquetOrderChanged += Refresh;

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

    private void EnsureVisuals()
    {
        if (titleText != null)
        {
            return;
        }

        EnsureBouquetBase();

        titleText = CreateText("TitleText", new Vector3(0f, 0.9f, -0.01f), 0.07f);
        hintText = CreateText("HintText", new Vector3(0f, -0.96f, -0.01f), 0.05f);

        GameObject slotRootObject = new GameObject("SlotRoot");
        slotRootObject.transform.SetParent(transform, false);
        slotRootObject.transform.localPosition = new Vector3(0f, 0.42f, -0.01f);
        slotRoot = slotRootObject.transform;
    }

    private TextMesh CreateText(string objectName, Vector3 localPosition, float characterSize)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = localPosition;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 48;
        textMesh.color = Color.white;

        MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 24;
        return textMesh;
    }

    private void EnsureBouquetBase()
    {
        if (bouquetBaseRenderer == null)
        {
            GameObject bouquetBaseObject = new GameObject("BouquetBase");
            bouquetBaseObject.transform.SetParent(transform, false);
            bouquetBaseObject.transform.localPosition = new Vector3(0f, -0.14f, -0.005f);
            bouquetBaseRenderer = bouquetBaseObject.AddComponent<SpriteRenderer>();
        }

        if (bouquetBaseSprite == null)
        {
            bouquetBaseSprite = LoadBouquetBaseSprite();
        }

        bouquetBaseRenderer.sprite = bouquetBaseSprite;
        bouquetBaseRenderer.sortingOrder = 21;
        bouquetBaseRenderer.color = new Color(1f, 1f, 1f, 0.92f);
        bouquetBaseRenderer.transform.localScale = new Vector3(1.08f, 1.08f, 1f);
    }

    private void Refresh()
    {
        ClearSlots();

        if (bouquetOrderManager == null || bouquetOrderManager.ActiveOrder == null)
        {
            titleText.text = "花束摆放";
            hintText.text = bouquetOrderManager != null ? bouquetOrderManager.FeedbackMessage : "请先选择订单";
            return;
        }

        BouquetOrderData bouquetOrder = bouquetOrderManager.ActiveOrder.BouquetOrder;
        titleText.text = bouquetOrder.DisplayName;
        hintText.text = bouquetOrderManager.FeedbackMessage;

        for (int i = 0; i < bouquetOrderManager.SlotStates.Count; i++)
        {
            BouquetSlotState state = bouquetOrderManager.SlotStates[i];
            GameObject slotObject = new GameObject($"BouquetSlot_{state.SlotIndex}");
            slotObject.transform.SetParent(slotRoot, false);
            slotObject.transform.localPosition = state.Requirement.LocalPosition;

            BouquetSlotView slotView = slotObject.AddComponent<BouquetSlotView>();
            slotView.Initialize(state, bouquetOrderManager);
            slotViews.Add(slotView);
        }
    }

    private void ClearSlots()
    {
        for (int i = 0; i < slotViews.Count; i++)
        {
            if (slotViews[i] != null)
            {
                Destroy(slotViews[i].gameObject);
            }
        }

        slotViews.Clear();
    }

    private Sprite LoadBouquetBaseSprite()
    {
        Sprite sprite = Resources.Load<Sprite>(BouquetBaseSpriteResourcePath);
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(BouquetBaseSpriteResourcePath);
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
