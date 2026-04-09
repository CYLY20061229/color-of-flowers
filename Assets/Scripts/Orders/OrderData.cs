using System;
using System.Collections.Generic;

[Serializable]
public class OrderData
{
    private readonly List<OrderRequirement> requirements = new List<OrderRequirement>();

    public int Id { get; private set; }
    public bool IsCompleted { get; private set; }
    public BouquetOrderData BouquetOrder { get; private set; }
    public bool HasBouquetOrder => BouquetOrder != null;
    public IReadOnlyList<OrderRequirement> Requirements => requirements;

    public OrderData(int id, IEnumerable<OrderRequirement> orderRequirements, BouquetOrderData bouquetOrder = null)
    {
        Id = id;
        BouquetOrder = bouquetOrder;
        requirements.AddRange(orderRequirements);
    }

    public void MarkCompleted()
    {
        IsCompleted = true;
    }
}
