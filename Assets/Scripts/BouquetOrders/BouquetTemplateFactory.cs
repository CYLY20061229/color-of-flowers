using System.Collections.Generic;
using UnityEngine;

public static class BouquetTemplateFactory
{
    public static BouquetSlotRequirement[] CreateSevenSlotBoardLayout()
    {
        return new[]
        {
            new BouquetSlotRequirement(0, new Vector2(-0.18f, 0.42f), FlowerColor.White, false),
            new BouquetSlotRequirement(1, new Vector2(0.18f, 0.42f), FlowerColor.White, false),
            new BouquetSlotRequirement(2, new Vector2(-0.34f, 0.14f), FlowerColor.White, false),
            new BouquetSlotRequirement(3, new Vector2(0f, 0.18f), FlowerColor.White, false),
            new BouquetSlotRequirement(4, new Vector2(0.34f, 0.14f), FlowerColor.White, false),
            new BouquetSlotRequirement(5, new Vector2(-0.18f, -0.14f), FlowerColor.White, false),
            new BouquetSlotRequirement(6, new Vector2(0.18f, -0.14f), FlowerColor.White, false)
        };
    }

    public static BouquetOrderData CreateThreeFlowerTemplate(int templateId, FlowerColor top, FlowerColor bottomLeft, FlowerColor bottomRight)
    {
        return new BouquetOrderData(
            templateId,
            "Three Flower Bouquet",
            new[]
            {
                new BouquetSlotRequirement(2, new Vector2(-0.34f, 0.14f), bottomLeft),
                new BouquetSlotRequirement(3, new Vector2(0f, 0.18f), top),
                new BouquetSlotRequirement(4, new Vector2(0.34f, 0.14f), bottomRight)
            });
    }

    public static BouquetOrderData CreateFiveFlowerTemplate(int templateId, FlowerColor center, FlowerColor top, FlowerColor left, FlowerColor right, FlowerColor bottom)
    {
        return new BouquetOrderData(
            templateId,
            "Five Flower Bouquet",
            new[]
            {
                new BouquetSlotRequirement(0, new Vector2(-0.18f, 0.42f), left),
                new BouquetSlotRequirement(1, new Vector2(0.18f, 0.42f), right),
                new BouquetSlotRequirement(2, new Vector2(-0.34f, 0.14f), top),
                new BouquetSlotRequirement(3, new Vector2(0f, 0.18f), center),
                new BouquetSlotRequirement(4, new Vector2(0.34f, 0.14f), bottom)
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
