using System;

[Serializable]
public class OrderRequirement
{
    public FlowerColor Color { get; private set; }
    public int RequiredCount { get; private set; }

    public OrderRequirement(FlowerColor color, int requiredCount)
    {
        Color = color;
        RequiredCount = requiredCount;
    }
}
