using System;
using UnityEngine;

[Serializable]
public class BouquetSlotRequirement
{
    public int SlotIndex { get; private set; }
    public Vector2 LocalPosition { get; private set; }
    public FlowerColor RequiredFlowerColor { get; private set; }
    public bool IsRequired { get; private set; }

    public BouquetSlotRequirement(int slotIndex, Vector2 localPosition, FlowerColor requiredFlowerColor, bool isRequired = true)
    {
        SlotIndex = slotIndex;
        LocalPosition = localPosition;
        RequiredFlowerColor = requiredFlowerColor;
        IsRequired = isRequired;
    }
}
