public class FlowerSpeciesState
{
    public FlowerSpeciesState(FlowerSpeciesDefinition definition, bool isUnlocked)
    {
        Definition = definition;
        IsUnlocked = isUnlocked;
    }

    public FlowerSpeciesDefinition Definition { get; }
    public bool IsUnlocked { get; private set; }

    public void Unlock()
    {
        IsUnlocked = true;
    }
}
