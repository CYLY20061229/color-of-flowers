using UnityEngine;

public static class HarvestResultEvaluator
{
    public static HarvestResult Evaluate(float normalizedCharge, ChargeHarvestConfig config)
    {
        if (config == null)
        {
            return HarvestResult.Bad;
        }

        if (IsInsideRange(normalizedCharge, config.PerfectRange))
        {
            return HarvestResult.Perfect;
        }

        if (IsInsideRange(normalizedCharge, config.GoodRange))
        {
            return HarvestResult.Good;
        }

        return HarvestResult.Bad;
    }

    private static bool IsInsideRange(float value, Vector2 range)
    {
        return value >= range.x && value <= range.y;
    }
}
