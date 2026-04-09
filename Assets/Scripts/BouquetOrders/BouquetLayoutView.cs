using System.Collections.Generic;
using UnityEngine;

public class BouquetLayoutView : MonoBehaviour
{
    private const float PanelWidth = 2.8f;
    private const float PanelHeight = 2.35f;

    private BouquetOrderManager bouquetOrderManager;
    private TextMesh titleText;
    private TextMesh hintText;
    private Transform slotRoot;
    private readonly List<BouquetSlotView> slotViews = new List<BouquetSlotView>();

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

        SimpleShapeFactory.CreateRectangle("BouquetOrderPanel", transform, new Vector2(PanelWidth, PanelHeight), new Color(0.08f, 0.1f, 0.13f, 0.9f), 20);

        titleText = CreateText("TitleText", new Vector3(0f, 0.92f, -0.01f), 0.085f);
        hintText = CreateText("HintText", new Vector3(0f, -0.92f, -0.01f), 0.065f);

        GameObject slotRootObject = new GameObject("SlotRoot");
        slotRootObject.transform.SetParent(transform, false);
        slotRootObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);
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

    private void Refresh()
    {
        ClearSlots();

        if (bouquetOrderManager == null || bouquetOrderManager.ActiveOrder == null)
        {
            titleText.text = "Bouquet Layout";
            hintText.text = bouquetOrderManager != null ? bouquetOrderManager.FeedbackMessage : "Select a bouquet customer";
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
}
