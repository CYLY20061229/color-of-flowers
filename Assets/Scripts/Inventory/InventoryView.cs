using System.Text;
using System.Collections.Generic;
using UnityEngine;

public class InventoryView : MonoBehaviour
{
    private const float PanelWidth = 2.2f;
    private const float PanelHeight = 2.6f;

    private InventorySystem inventorySystem;
    private BouquetSystem bouquetSystem;
    private BouquetOrderManager bouquetOrderManager;
    private TextMesh label;
    private Transform itemRoot;
    private readonly List<InventoryFlowerItemView> itemViews = new List<InventoryFlowerItemView>();

    public void Initialize(InventorySystem inventory, BouquetSystem bouquet = null, BouquetOrderManager bouquetOrders = null)
    {
        if (inventorySystem != null)
        {
            inventorySystem.InventoryChanged -= Refresh;
        }

        if (bouquetSystem != null)
        {
            bouquetSystem.BouquetChanged -= Refresh;
        }

        if (bouquetOrderManager != null)
        {
            bouquetOrderManager.BouquetOrderChanged -= Refresh;
        }

        inventorySystem = inventory;
        bouquetSystem = bouquet;
        bouquetOrderManager = bouquetOrders;
        inventorySystem.InventoryChanged += Refresh;

        if (bouquetSystem != null)
        {
            bouquetSystem.BouquetChanged += Refresh;
        }

        if (bouquetOrderManager != null)
        {
            bouquetOrderManager.BouquetOrderChanged += Refresh;
        }

        EnsureVisuals();
        Refresh();
    }

    private void OnDestroy()
    {
        if (inventorySystem != null)
        {
            inventorySystem.InventoryChanged -= Refresh;
        }

        if (bouquetSystem != null)
        {
            bouquetSystem.BouquetChanged -= Refresh;
        }

        if (bouquetOrderManager != null)
        {
            bouquetOrderManager.BouquetOrderChanged -= Refresh;
        }
    }

    private void EnsureVisuals()
    {
        if (label != null)
        {
            return;
        }

        SimpleShapeFactory.CreateRectangle("InventoryPanel", transform, new Vector2(PanelWidth, PanelHeight), new Color(0.08f, 0.08f, 0.08f, 0.75f), 10);
        EnsureDropCollider();

        GameObject labelObject = new GameObject("InventoryLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(-0.9f, 0.95f, -0.01f);

        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.UpperLeft;
        label.alignment = TextAlignment.Left;
        label.characterSize = 0.085f;
        label.fontSize = 48;
        label.color = Color.white;

        MeshRenderer renderer = labelObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 11;

        GameObject itemRootObject = new GameObject("InventoryItems");
        itemRootObject.transform.SetParent(transform, false);
        itemRootObject.transform.localPosition = new Vector3(0f, 0.35f, 0f);
        itemRoot = itemRootObject.transform;
    }

    private void EnsureDropCollider()
    {
        BoxCollider2D collider = GetComponent<BoxCollider2D>();
        if (collider == null)
        {
            collider = gameObject.AddComponent<BoxCollider2D>();
        }

        collider.isTrigger = true;
        collider.size = new Vector2(PanelWidth, PanelHeight);
    }

    private void Refresh()
    {
        if (inventorySystem == null || label == null)
        {
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Flower Basket");
        builder.AppendLine(bouquetOrderManager != null && bouquetOrderManager.HasActiveBouquetOrder ? "Drag flower to slot" : "Click flower to submit");

        if (inventorySystem.Flowers.Count == 0)
        {
            builder.AppendLine("Empty");
        }

        label.text = builder.ToString();
        RefreshItems();
    }

    private void RefreshItems()
    {
        ClearItems();

        int itemIndex = 0;
        foreach (FlowerColor color in System.Enum.GetValues(typeof(FlowerColor)))
        {
            int count = inventorySystem.GetCount(color);
            if (count <= 0)
            {
                continue;
            }

            GameObject itemObject = new GameObject($"InventoryItem_{color}");
            itemObject.transform.SetParent(itemRoot, false);
            itemObject.transform.localPosition = new Vector3(0f, -itemIndex * 0.34f, -0.02f);

            InventoryFlowerItemView itemView = itemObject.AddComponent<InventoryFlowerItemView>();
            itemView.Initialize(color, count, bouquetSystem, bouquetOrderManager);
            itemViews.Add(itemView);

            itemIndex++;
        }
    }

    private void ClearItems()
    {
        for (int i = 0; i < itemViews.Count; i++)
        {
            if (itemViews[i] != null)
            {
                Destroy(itemViews[i].gameObject);
            }
        }

        itemViews.Clear();
    }
}
