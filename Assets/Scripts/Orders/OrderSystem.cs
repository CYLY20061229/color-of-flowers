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
        switch ((id - 1) % 4)
        {
            case 0:
                bouquetOrder = BouquetTemplateFactory.CreateThreeFlowerTemplate(id, FlowerColor.Magenta, FlowerColor.Red, FlowerColor.Red);
                return new OrderData(id, BouquetTemplateFactory.BuildCountRequirements(bouquetOrder), bouquetOrder);
            case 1:
                bouquetOrder = BouquetTemplateFactory.CreateThreeFlowerTemplate(id, FlowerColor.Cyan, FlowerColor.Green, FlowerColor.Green);
                return new OrderData(id, BouquetTemplateFactory.BuildCountRequirements(bouquetOrder), bouquetOrder);
            case 2:
                bouquetOrder = BouquetTemplateFactory.CreateThreeFlowerTemplate(id, FlowerColor.Yellow, FlowerColor.Blue, FlowerColor.Blue);
                return new OrderData(id, BouquetTemplateFactory.BuildCountRequirements(bouquetOrder), bouquetOrder);
            default:
                bouquetOrder = BouquetTemplateFactory.CreateFiveFlowerTemplate(
                    id,
                    FlowerColor.White,
                    FlowerColor.Red,
                    FlowerColor.Green,
                    FlowerColor.Blue,
                    FlowerColor.Yellow);
                return new OrderData(id, BouquetTemplateFactory.BuildCountRequirements(bouquetOrder), bouquetOrder);
        }
    }
}
