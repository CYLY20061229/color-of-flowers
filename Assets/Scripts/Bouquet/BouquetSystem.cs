using System;
using System.Collections.Generic;
using UnityEngine;

public class BouquetSystem : MonoBehaviour
{
    private readonly Dictionary<FlowerColor, int> submittedCounts = new Dictionary<FlowerColor, int>();

    private InventorySystem inventorySystem;
    private OrderSystem orderSystem;

    public event Action BouquetChanged;

    public void Initialize(InventorySystem inventory, OrderSystem orders)
    {
        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged -= HandleActiveOrderChanged;
        }

        inventorySystem = inventory;
        orderSystem = orders;

        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged += HandleActiveOrderChanged;
        }

        ResetSubmission();
    }

    private void OnDestroy()
    {
        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged -= HandleActiveOrderChanged;
        }
    }

    public bool TrySubmitFlower(FlowerColor color)
    {
        OrderData order = orderSystem != null ? orderSystem.SelectedOrder : null;
        if (order == null || order.IsCompleted || inventorySystem == null)
        {
            return false;
        }

        OrderRequirement requirement = FindRequirement(order, color);
        if (requirement == null)
        {
            return false;
        }

        int submittedCount = GetSubmittedCount(color);
        if (submittedCount >= requirement.RequiredCount)
        {
            return false;
        }

        if (!inventorySystem.TryRemoveFlower(color))
        {
            return false;
        }

        submittedCounts[color] = submittedCount + 1;
        BouquetChanged?.Invoke();

        if (IsOrderSatisfied(order))
        {
            orderSystem.CompleteOrder(order);
        }

        return true;
    }

    public int GetSubmittedCount(FlowerColor color)
    {
        return submittedCounts.TryGetValue(color, out int count) ? count : 0;
    }

    public bool IsColorNeededBySelectedOrder(FlowerColor color)
    {
        OrderData order = orderSystem != null ? orderSystem.SelectedOrder : null;
        return order != null && !order.IsCompleted && FindRequirement(order, color) != null;
    }

    private void HandleActiveOrderChanged(OrderData order)
    {
        ResetSubmission();
    }

    private void ResetSubmission()
    {
        submittedCounts.Clear();
        BouquetChanged?.Invoke();
    }

    private bool IsOrderSatisfied(OrderData order)
    {
        for (int i = 0; i < order.Requirements.Count; i++)
        {
            OrderRequirement requirement = order.Requirements[i];
            if (GetSubmittedCount(requirement.Color) < requirement.RequiredCount)
            {
                return false;
            }
        }

        return true;
    }

    private static OrderRequirement FindRequirement(OrderData order, FlowerColor color)
    {
        for (int i = 0; i < order.Requirements.Count; i++)
        {
            OrderRequirement requirement = order.Requirements[i];
            if (requirement.Color == color)
            {
                return requirement;
            }
        }

        return null;
    }
}
