using System;

[Serializable]
public enum OrderRequirementType
{
    TotalCount,
    SlotColor
}

[Serializable]
public class OrderRequirement
{
    public OrderRequirementType RequirementType { get; private set; }
    public FlowerColor Color { get; private set; }
    public int RequiredCount { get; private set; }
    public int SlotIndex { get; private set; }

    public OrderRequirement(FlowerColor color, int requiredCount)
        : this(OrderRequirementType.TotalCount, color, requiredCount, -1)
    {
    }

    public OrderRequirement(FlowerColor color, int requiredCount, int slotIndex)
        : this(OrderRequirementType.SlotColor, color, requiredCount, slotIndex)
    {
    }

    public OrderRequirement(OrderRequirementType requirementType, FlowerColor color, int requiredCount, int slotIndex = -1)
    {
        RequirementType = requirementType;
        Color = color;
        RequiredCount = requiredCount;
        SlotIndex = slotIndex;
    }
}
