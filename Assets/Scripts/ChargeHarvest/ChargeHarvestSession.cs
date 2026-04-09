public class ChargeHarvestSession
{
    public BranchController TargetBranch { get; private set; }
    public float NormalizedCharge { get; private set; }
    public bool IsRunning { get; private set; }

    public void Begin(BranchController targetBranch)
    {
        TargetBranch = targetBranch;
        NormalizedCharge = 0f;
        IsRunning = true;
    }

    public void SetNormalizedCharge(float normalizedCharge)
    {
        NormalizedCharge = normalizedCharge;
    }

    public void Complete()
    {
        IsRunning = false;
        TargetBranch = null;
    }
}
