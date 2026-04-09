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
        switch ((id - 1) % 4)
        {
            case 0:
                return new OrderData(id, new[]
                {
                    new OrderRequirement(FlowerColor.Red, 2),
                    new OrderRequirement(FlowerColor.Magenta, 1)
                });
            case 1:
                return new OrderData(id, new[]
                {
                    new OrderRequirement(FlowerColor.Green, 2),
                    new OrderRequirement(FlowerColor.Cyan, 1)
                });
            case 2:
                return new OrderData(id, new[]
                {
                    new OrderRequirement(FlowerColor.Blue, 2),
                    new OrderRequirement(FlowerColor.Yellow, 1)
                });
            default:
                return new OrderData(id, new[]
                {
                    new OrderRequirement(FlowerColor.Red, 1),
                    new OrderRequirement(FlowerColor.Green, 1),
                    new OrderRequirement(FlowerColor.Blue, 1)
                });
        }
    }
}
