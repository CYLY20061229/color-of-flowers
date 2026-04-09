public class BouquetSlotState
{
    public BouquetSlotRequirement Requirement { get; private set; }
    public FlowerData CurrentFlowerData { get; private set; }

    public int SlotIndex => Requirement.SlotIndex;
    public bool IsFilled => CurrentFlowerData != null;
    public bool IsCorrect => IsFilled && CurrentFlowerData.Color == Requirement.RequiredFlowerColor;

    public BouquetSlotState(BouquetSlotRequirement requirement)
    {
        Requirement = requirement;
    }

    public void SetFlower(FlowerData flowerData)
    {
        CurrentFlowerData = flowerData;
    }

    public FlowerData ClearFlower()
    {
        FlowerData flower = CurrentFlowerData;
        CurrentFlowerData = null;
        return flower;
    }
}
