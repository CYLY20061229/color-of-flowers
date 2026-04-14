using System.Collections.Generic;
using System.Text;

public class OrderValidationResult
{
    private readonly List<string> failureReasons = new List<string>();

    public bool IsSuccess => failureReasons.Count == 0;
    public IReadOnlyList<string> FailureReasons => failureReasons;
    public string Summary
    {
        get
        {
            if (IsSuccess)
            {
                return "已经可以提交订单";
            }

            if (failureReasons.Count == 1)
            {
                return failureReasons[0];
            }

            StringBuilder builder = new StringBuilder();
            for (int i = 0; i < failureReasons.Count; i++)
            {
                if (i > 0)
                {
                    builder.Append("\n");
                }

                builder.Append(failureReasons[i]);
            }

            return builder.ToString();
        }
    }

    public void AddFailure(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            failureReasons.Add(message);
        }
    }
}

public static class OrderValidationSystem
{
    public static bool IsBouquetComplete(IReadOnlyList<BouquetSlotState> slotStates)
    {
        if (slotStates == null || slotStates.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < slotStates.Count; i++)
        {
            BouquetSlotState slot = slotStates[i];
            if (!slot.Requirement.IsRequired)
            {
                continue;
            }

            if (!slot.IsCorrect)
            {
                return false;
            }
        }

        return true;
    }

    public static OrderValidationResult ValidateBouquet(IReadOnlyList<BouquetSlotState> slotStates)
    {
        OrderValidationResult result = new OrderValidationResult();
        if (slotStates == null || slotStates.Count == 0)
        {
            result.AddFailure("当前没有可用的花束槽位");
            return result;
        }

        Dictionary<FlowerColor, int> colorCounts = new Dictionary<FlowerColor, int>();

        for (int i = 0; i < slotStates.Count; i++)
        {
            BouquetSlotState slot = slotStates[i];
            if (slot == null || slot.Requirement == null || !slot.Requirement.IsRequired)
            {
                continue;
            }

            if (!slot.IsFilled)
            {
                result.AddFailure($"槽位 {slot.SlotIndex + 1} 还是空的");
                continue;
            }

            FlowerColor placedColor = slot.CurrentFlowerData.Color;
            if (!colorCounts.ContainsKey(placedColor))
            {
                colorCounts.Add(placedColor, 0);
            }

            colorCounts[placedColor]++;

            if (!slot.IsCorrect)
            {
                result.AddFailure($"槽位 {slot.SlotIndex + 1} 需要{FlowerColorPalette.GetDisplayName(slot.Requirement.RequiredFlowerColor)}");
            }
        }

        return result;
    }
}
