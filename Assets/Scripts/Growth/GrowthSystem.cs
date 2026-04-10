using System.Collections;
using UnityEngine;

public class GrowthSystem : MonoBehaviour
{
    [SerializeField] private float growthDurationSeconds = GameConstants.DefaultGrowthDurationSeconds;

    private Coroutine growthRoutine;
    private BranchController branch;

    public bool IsGrowing => growthRoutine != null;

    public void Initialize(BranchController branchController)
    {
        branch = branchController;
    }

    public bool TryStartGrowth(FlowerColor flowerColor)
    {
        return TryStartGrowth(flowerColor, BranchState.Growing);
    }

    public bool TryStartGrowth(FlowerColor flowerColor, BranchState growthState)
    {
        if (branch == null || branch.Data.State != BranchState.Idle || growthRoutine != null)
        {
            return false;
        }

        growthRoutine = StartCoroutine(Grow(flowerColor, growthState));
        return true;
    }

    private IEnumerator Grow(FlowerColor flowerColor, BranchState growthState)
    {
        float totalDurationSeconds = GetGrowthDurationSeconds();
        float elapsedSeconds = 0f;

        branch.SetGrowthState(flowerColor, growthState);
        branch.BeginGrowthProgress(totalDurationSeconds);

        while (elapsedSeconds < totalDurationSeconds)
        {
            elapsedSeconds += Time.deltaTime;
            branch.UpdateGrowthProgress(elapsedSeconds, totalDurationSeconds);
            yield return null;
        }

        branch.EndGrowthProgress();
        branch.SetMature(flowerColor);
        growthRoutine = null;
    }

    private float GetGrowthDurationSeconds()
    {
        float damagePenalty = 0f;
        if (GameManager.Instance != null && GameManager.Instance.ChargeHarvestConfig != null && branch != null && branch.Data != null)
        {
            damagePenalty = branch.Data.DamageLevel * GameManager.Instance.ChargeHarvestConfig.DamageGrowthPenaltySeconds;
        }

        return growthDurationSeconds + damagePenalty;
    }
}
