using System;
using System.Collections.Generic;
using UnityEngine;

public class BouquetOrderManager : MonoBehaviour
{
    private readonly List<BouquetSlotState> slotStates = new List<BouquetSlotState>();
    private InventorySystem inventorySystem;
    private OrderSystem orderSystem;

    public event Action BouquetOrderChanged;

    public OrderData ActiveOrder { get; private set; }
    public bool HasActiveBouquetOrder => ActiveOrder != null;
    public string FeedbackMessage { get; private set; } = "Select a bouquet customer";
    public IReadOnlyList<BouquetSlotState> SlotStates => slotStates;

    public void Initialize(InventorySystem inventory, OrderSystem orders)
    {
        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged -= HandleActiveOrderChanged;
            orderSystem.OrdersChanged -= HandleOrdersChanged;
        }

        inventorySystem = inventory;
        orderSystem = orders;

        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged += HandleActiveOrderChanged;
            orderSystem.OrdersChanged += HandleOrdersChanged;
        }

        SetActiveOrder(orderSystem != null ? orderSystem.SelectedOrder : null);
    }

    public bool TryPlaceFlower(int slotIndex, FlowerColor flowerColor)
    {
        if (ActiveOrder == null || inventorySystem == null)
        {
            FeedbackMessage = "Select a bouquet customer first";
            return false;
        }

        BouquetSlotState slot = FindSlot(slotIndex);
        if (slot == null)
        {
            FeedbackMessage = "That bouquet slot is unavailable";
            BouquetOrderChanged?.Invoke();
            return false;
        }

        if (!inventorySystem.TryRemoveFlower(flowerColor))
        {
            FeedbackMessage = $"No {flowerColor} flower in basket";
            BouquetOrderChanged?.Invoke();
            return false;
        }

        bool replacedFlower = slot.IsFilled;
        FlowerData previousFlower = replacedFlower ? slot.CurrentFlowerData : null;
        slot.SetFlower(new FlowerData(flowerColor));

        if (previousFlower != null)
        {
            inventorySystem.AddFlower(previousFlower);
        }

        FeedbackMessage = slot.IsCorrect
            ? (replacedFlower ? "Flower replaced" : "Flower placed")
            : $"Placed, needs {slot.Requirement.RequiredFlowerColor}";

        if (OrderValidationSystem.IsBouquetComplete(slotStates))
        {
            OrderData completedOrder = ActiveOrder;
            FeedbackMessage = "Bouquet complete!";
            BouquetOrderChanged?.Invoke();
            orderSystem?.CompleteOrder(completedOrder);
            return true;
        }

        BouquetOrderChanged?.Invoke();
        return true;
    }

    public bool TryReturnFlowerToInventory(int slotIndex)
    {
        if (ActiveOrder == null || inventorySystem == null)
        {
            FeedbackMessage = "Select a bouquet customer first";
            BouquetOrderChanged?.Invoke();
            return false;
        }

        BouquetSlotState slot = FindSlot(slotIndex);
        if (slot == null || !slot.IsFilled)
        {
            FeedbackMessage = "No flower to return";
            BouquetOrderChanged?.Invoke();
            return false;
        }

        FlowerData returnedFlower = slot.ClearFlower();
        inventorySystem.AddFlower(returnedFlower);
        FeedbackMessage = "Flower returned to basket";
        BouquetOrderChanged?.Invoke();
        return true;
    }

    private void OnDestroy()
    {
        if (orderSystem != null)
        {
            orderSystem.ActiveOrderChanged -= HandleActiveOrderChanged;
            orderSystem.OrdersChanged -= HandleOrdersChanged;
        }
    }

    private void HandleActiveOrderChanged(OrderData order)
    {
        SetActiveOrder(order);
    }

    private void HandleOrdersChanged()
    {
        if (ActiveOrder != null && ActiveOrder.IsCompleted)
        {
            SetActiveOrder(null);
        }
        else
        {
            BouquetOrderChanged?.Invoke();
        }
    }

    private void SetActiveOrder(OrderData order)
    {
        ActiveOrder = order != null && order.HasBouquetOrder ? order : null;
        FeedbackMessage = ActiveOrder != null ? "Drag flowers into matching slots" : FeedbackMessage;
        RebuildSlotStates();
        BouquetOrderChanged?.Invoke();
    }

    private void RebuildSlotStates()
    {
        slotStates.Clear();
        if (ActiveOrder == null || ActiveOrder.BouquetOrder == null)
        {
            return;
        }

        for (int i = 0; i < ActiveOrder.BouquetOrder.Slots.Count; i++)
        {
            slotStates.Add(new BouquetSlotState(ActiveOrder.BouquetOrder.Slots[i]));
        }
    }

    private BouquetSlotState FindSlot(int slotIndex)
    {
        for (int i = 0; i < slotStates.Count; i++)
        {
            if (slotStates[i].SlotIndex == slotIndex)
            {
                return slotStates[i];
            }
        }

        return null;
    }
}
