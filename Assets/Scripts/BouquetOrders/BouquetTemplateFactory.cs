using System.Collections.Generic;
using UnityEngine;

public static class BouquetTemplateFactory
{
    public static BouquetOrderData CreateThreeFlowerTemplate(int templateId, FlowerColor top, FlowerColor bottomLeft, FlowerColor bottomRight)
    {
        return new BouquetOrderData(
            templateId,
            "Three Flower Bouquet",
            new[]
            {
                new BouquetSlotRequirement(0, new Vector2(0f, 0.45f), top),
                new BouquetSlotRequirement(1, new Vector2(-0.48f, -0.28f), bottomLeft),
                new BouquetSlotRequirement(2, new Vector2(0.48f, -0.28f), bottomRight)
            });
    }

    public static BouquetOrderData CreateFiveFlowerTemplate(int templateId, FlowerColor center, FlowerColor top, FlowerColor left, FlowerColor right, FlowerColor bottom)
    {
        return new BouquetOrderData(
            templateId,
            "Five Flower Bouquet",
            new[]
            {
                new BouquetSlotRequirement(0, new Vector2(0f, 0f), center),
                new BouquetSlotRequirement(1, new Vector2(0f, 0.58f), top),
                new BouquetSlotRequirement(2, new Vector2(-0.58f, 0f), left),
                new BouquetSlotRequirement(3, new Vector2(0.58f, 0f), right),
                new BouquetSlotRequirement(4, new Vector2(0f, -0.58f), bottom)
            });
    }

    public static List<OrderRequirement> BuildCountRequirements(BouquetOrderData bouquetOrder)
    {
        Dictionary<FlowerColor, int> counts = new Dictionary<FlowerColor, int>();
        for (int i = 0; i < bouquetOrder.Slots.Count; i++)
        {
            BouquetSlotRequirement slot = bouquetOrder.Slots[i];
            if (!slot.IsRequired)
            {
                continue;
            }

            if (!counts.ContainsKey(slot.RequiredFlowerColor))
            {
                counts.Add(slot.RequiredFlowerColor, 0);
            }

            counts[slot.RequiredFlowerColor]++;
        }

        List<OrderRequirement> requirements = new List<OrderRequirement>();
        foreach (KeyValuePair<FlowerColor, int> pair in counts)
        {
            requirements.Add(new OrderRequirement(pair.Key, pair.Value));
        }

        return requirements;
    }
}
