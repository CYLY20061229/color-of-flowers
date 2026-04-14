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
    public string FeedbackMessage { get; private set; } = "请先选择顾客订单";
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
            FeedbackMessage = "请先选择顾客订单";
            return false;
        }

        BouquetSlotState slot = FindSlot(slotIndex);
        if (slot == null)
        {
            FeedbackMessage = "这个花束位置不可用";
            BouquetOrderChanged?.Invoke();
            return false;
        }

        if (!inventorySystem.TryRemoveFlower(flowerColor))
        {
            FeedbackMessage = $"花篮里没有{FlowerColorPalette.GetDisplayName(flowerColor)}花";
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

        if (!slot.Requirement.IsRequired)
        {
            FeedbackMessage = replacedFlower ? "已替换花朵" : "已放入花朵";
        }
        else
        {
            FeedbackMessage = slot.IsCorrect
                ? (replacedFlower ? "已替换花朵" : "已放入花朵")
                : $"已放入，但这里需要{FlowerColorPalette.GetDisplayName(slot.Requirement.RequiredFlowerColor)}";
        }

        BouquetOrderChanged?.Invoke();
        return true;
    }

    public bool TrySubmitActiveOrder()
    {
        if (ActiveOrder == null)
        {
            FeedbackMessage = "请先选择顾客订单";
            BouquetOrderChanged?.Invoke();
            return false;
        }

        OrderValidationResult validation = OrderValidationSystem.ValidateBouquet(slotStates);
        if (!validation.IsSuccess)
        {
            FeedbackMessage = validation.Summary;
            BouquetOrderChanged?.Invoke();
            return false;
        }

        OrderData completedOrder = ActiveOrder;
        CurrencyGateway currencyGateway = GameManager.Instance != null
            ? GameManager.Instance.Currency
            : FindFirstObjectByType<CurrencyGateway>();

        if (currencyGateway != null && completedOrder.RewardCoins > 0)
        {
            currencyGateway.AddCoins(completedOrder.RewardCoins);
            FeedbackMessage = $"订单提交成功，获得 {completedOrder.RewardCoins} 金币！";
        }
        else
        {
            FeedbackMessage = "订单提交成功！";
        }

        BouquetOrderChanged?.Invoke();
        orderSystem?.CompleteOrder(completedOrder);
        return true;
    }

    public bool TryReturnFlowerToInventory(int slotIndex)
    {
        if (ActiveOrder == null || inventorySystem == null)
        {
            FeedbackMessage = "请先选择顾客订单";
            BouquetOrderChanged?.Invoke();
            return false;
        }

        BouquetSlotState slot = FindSlot(slotIndex);
        if (slot == null || !slot.IsFilled)
        {
            FeedbackMessage = "这里没有可退回的花";
            BouquetOrderChanged?.Invoke();
            return false;
        }

        FlowerData returnedFlower = slot.ClearFlower();
        inventorySystem.AddFlower(returnedFlower);
        FeedbackMessage = "花朵已退回花篮";
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
        OrderData nextOrder = order != null && order.HasBouquetOrder ? order : null;
        ReturnPlacedFlowersToInventoryIfNeeded(nextOrder);

        ActiveOrder = nextOrder;
        FeedbackMessage = ActiveOrder != null ? "把花拖到对应位置后再提交" : FeedbackMessage;
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

        BouquetSlotRequirement[] boardLayout = BouquetTemplateFactory.CreateSevenSlotBoardLayout();
        for (int i = 0; i < boardLayout.Length; i++)
        {
            BouquetSlotRequirement boardSlot = boardLayout[i];
            BouquetSlotRequirement requiredSlot = FindRequiredSlotForBoardIndex(ActiveOrder.BouquetOrder, boardSlot.SlotIndex);

            BouquetSlotRequirement resolvedRequirement = requiredSlot != null
                ? new BouquetSlotRequirement(
                    boardSlot.SlotIndex,
                    boardSlot.LocalPosition,
                    requiredSlot.RequiredFlowerColor,
                    true)
                : new BouquetSlotRequirement(
                    boardSlot.SlotIndex,
                    boardSlot.LocalPosition,
                    FlowerColor.White,
                    false);

            slotStates.Add(new BouquetSlotState(resolvedRequirement));
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

    private void ReturnPlacedFlowersToInventoryIfNeeded(OrderData nextOrder)
    {
        if (inventorySystem == null || ActiveOrder == null || ActiveOrder == nextOrder || ActiveOrder.IsCompleted)
        {
            return;
        }

        bool returnedAnyFlower = false;
        for (int i = 0; i < slotStates.Count; i++)
        {
            if (!slotStates[i].IsFilled)
            {
                continue;
            }

            FlowerData returnedFlower = slotStates[i].ClearFlower();
            if (returnedFlower == null)
            {
                continue;
            }

            inventorySystem.AddFlower(returnedFlower);
            returnedAnyFlower = true;
        }

        if (returnedAnyFlower)
        {
            FeedbackMessage = "未完成订单中的花已退回花篮";
        }
    }

    private static BouquetSlotRequirement FindRequiredSlotForBoardIndex(BouquetOrderData bouquetOrder, int slotIndex)
    {
        if (bouquetOrder == null)
        {
            return null;
        }

        for (int i = 0; i < bouquetOrder.Slots.Count; i++)
        {
            BouquetSlotRequirement requirement = bouquetOrder.Slots[i];
            if (requirement.SlotIndex == slotIndex)
            {
                return requirement;
            }
        }

        return null;
    }
}
