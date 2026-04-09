using System;

[Serializable]
public class BranchData
{
    public int Index { get; private set; }
    public FlowerColor InitialColor { get; private set; }
    public FlowerColor CurrentColor { get; private set; }
    public BranchState State { get; private set; }
    public int DamageLevel { get; private set; }

    public BranchData(int index, FlowerColor initialColor)
    {
        Index = index;
        InitialColor = initialColor;
        CurrentColor = initialColor;
        State = BranchState.Idle;
        DamageLevel = 0;
    }

    public void SetCurrentColor(FlowerColor color)
    {
        CurrentColor = color;
    }

    public void SetState(BranchState state)
    {
        State = state;
    }

    public void ResetToInitialState()
    {
        CurrentColor = InitialColor;
        State = BranchState.Idle;
    }

    public void IncreaseDamage(int amount = 1)
    {
        DamageLevel += amount;
    }

    public void ReduceDamage(int amount = 1)
    {
        DamageLevel = Math.Max(0, DamageLevel - amount);
    }
}
