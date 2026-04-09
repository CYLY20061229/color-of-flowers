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
        if (branch == null || branch.Data.State != BranchState.Idle || growthRoutine != null)
        {
            return false;
        }

        growthRoutine = StartCoroutine(Grow(flowerColor));
        return true;
    }

    private IEnumerator Grow(FlowerColor flowerColor)
    {
        branch.SetGrowing(flowerColor);
        yield return new WaitForSeconds(growthDurationSeconds);
        branch.SetMature(flowerColor);
        growthRoutine = null;
    }
}
