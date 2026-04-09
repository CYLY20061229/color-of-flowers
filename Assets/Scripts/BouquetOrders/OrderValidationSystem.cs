using System.Collections.Generic;

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
}
