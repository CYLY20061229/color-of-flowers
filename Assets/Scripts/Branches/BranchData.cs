using System;

[Serializable]
public class BranchData
{
    public int Index { get; private set; }
    public FlowerColor InitialColor { get; private set; }
    public FlowerColor CurrentColor { get; private set; }
    public BranchState State { get; private set; }

    public BranchData(int index, FlowerColor initialColor)
    {
        Index = index;
        InitialColor = initialColor;
        CurrentColor = initialColor;
        State = BranchState.Idle;
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
}
