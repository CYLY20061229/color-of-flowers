using System;
using System.Collections.Generic;
using UnityEngine;

public class OrderSystem : MonoBehaviour
{
    private const int StartingOrderCount = 3;
    private const int MaxActiveOrders = 3;

    private readonly List<OrderData> activeOrders = new List<OrderData>();
    private int nextOrderId = 1;

    public event Action<OrderData> ActiveOrderChanged;
    public event Action OrdersChanged;

    public IReadOnlyList<OrderData> ActiveOrders => activeOrders;
    public OrderData SelectedOrder { get; private set; }

    public void InitializeDefaultOrders()
    {
        if (activeOrders.Count > 0)
        {
            return;
        }

        for (int i = 0; i < StartingOrderCount; i++)
        {
            activeOrders.Add(CreateOrder(nextOrderId));
            nextOrderId++;
        }

        OrdersChanged?.Invoke();
    }

    public void SelectOrder(OrderData order)
    {
        if (order == null || order.IsCompleted)
        {
            return;
        }

        SelectedOrder = order;
        ActiveOrderChanged?.Invoke(SelectedOrder);
        OrdersChanged?.Invoke();
    }

    public void CompleteOrder(OrderData order)
    {
        if (order == null || order.IsCompleted)
        {
            return;
        }

        order.MarkCompleted();

        if (SelectedOrder == order)
        {
            SelectedOrder = null;
            ActiveOrderChanged?.Invoke(SelectedOrder);
        }

        activeOrders.Remove(order);
        RefillOrders();
        OrdersChanged?.Invoke();
    }

    private void RefillOrders()
    {
        while (activeOrders.Count < MaxActiveOrders)
        {
            activeOrders.Add(CreateOrder(nextOrderId));
            nextOrderId++;
        }
    }

    private OrderData CreateOrder(int id)
    {
        BouquetOrderData bouquetOrder;
        string customerName;
        string chatSummary;
        int rewardCoins;

        switch ((id - 1) % 4)
        {
            case 0:
                bouquetOrder = BouquetTemplateFactory.CreateThreeFlowerTemplate(id, FlowerColor.Magenta, FlowerColor.Red, FlowerColor.Red);
                customerName = "Luna";
                chatSummary = "微信留言：想要一束偏暖色的花，顶部更亮一点，准备送给朋友。";
                rewardCoins = 25;
                break;
            case 1:
                bouquetOrder = BouquetTemplateFactory.CreateThreeFlowerTemplate(id, FlowerColor.Cyan, FlowerColor.Green, FlowerColor.Green);
                customerName = "Mika";
                chatSummary = "私信需求：整体想要清新一些，中间简单一点，感觉柔和干净。";
                rewardCoins = 28;
                break;
            case 2:
                bouquetOrder = BouquetTemplateFactory.CreateThreeFlowerTemplate(id, FlowerColor.Yellow, FlowerColor.Blue, FlowerColor.Blue);
                customerName = "Noah";
                chatSummary = "聊天摘要：希望有一点亮黄色点缀，再用冷色把它衬出来。";
                rewardCoins = 30;
                break;
            default:
                bouquetOrder = BouquetTemplateFactory.CreateFiveFlowerTemplate(
                    id,
                    FlowerColor.White,
                    FlowerColor.Red,
                    FlowerColor.Green,
                    FlowerColor.Blue,
                    FlowerColor.Yellow);
                customerName = "Iris";
                chatSummary = "聊天摘要：想要层次更丰富的花束，中心柔和一些，周围颜色分开。";
                rewardCoins = 42;
                break;
        }

        return new OrderData(
            id,
            customerName,
            chatSummary,
            BuildDisplayRequirements(bouquetOrder),
            bouquetOrder,
            null,
            null,
            rewardCoins);
    }

    private static List<OrderRequirement> BuildDisplayRequirements(BouquetOrderData bouquetOrder)
    {
        List<OrderRequirement> requirements = new List<OrderRequirement>();
        if (bouquetOrder == null)
        {
            return requirements;
        }

        for (int i = 0; i < bouquetOrder.Slots.Count; i++)
        {
            BouquetSlotRequirement slot = bouquetOrder.Slots[i];
            if (!slot.IsRequired)
            {
                continue;
            }

            requirements.Add(new OrderRequirement(slot.RequiredFlowerColor, 1, slot.SlotIndex));
        }

        return requirements;
    }
}
