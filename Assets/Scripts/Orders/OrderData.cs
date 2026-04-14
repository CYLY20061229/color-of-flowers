using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OrderData
{
    private readonly List<OrderRequirement> requirements = new List<OrderRequirement>();

    public int Id { get; private set; }
    public string CustomerName { get; private set; }
    public Sprite CustomerAvatar { get; private set; }
    public string ChatSummary { get; private set; }
    public Sprite ReferenceImage { get; private set; }
    public int RewardCoins { get; private set; }
    public bool IsCompleted { get; private set; }
    public BouquetOrderData BouquetOrder { get; private set; }
    public bool HasBouquetOrder => BouquetOrder != null;
    public IReadOnlyList<OrderRequirement> Requirements => requirements;

    public OrderData(int id, IEnumerable<OrderRequirement> orderRequirements, BouquetOrderData bouquetOrder = null)
        : this(
            id,
            $"Customer #{id}",
            "Please make this bouquet for me.",
            orderRequirements,
            bouquetOrder,
            null,
            null,
            0)
    {
    }

    public OrderData(
        int id,
        string customerName,
        string chatSummary,
        IEnumerable<OrderRequirement> orderRequirements,
        BouquetOrderData bouquetOrder = null,
        Sprite customerAvatar = null,
        Sprite referenceImage = null,
        int rewardCoins = 0)
    {
        Id = id;
        CustomerName = string.IsNullOrWhiteSpace(customerName) ? $"Customer #{id}" : customerName;
        ChatSummary = string.IsNullOrWhiteSpace(chatSummary) ? "Please make this bouquet for me." : chatSummary;
        CustomerAvatar = customerAvatar;
        ReferenceImage = referenceImage;
        RewardCoins = Mathf.Max(0, rewardCoins);
        BouquetOrder = bouquetOrder;
        requirements.AddRange(orderRequirements);
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
    }
}
