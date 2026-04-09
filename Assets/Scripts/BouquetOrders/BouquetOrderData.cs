using System;
using System.Collections.Generic;

[Serializable]
public class BouquetOrderData
{
    private readonly List<BouquetSlotRequirement> slots = new List<BouquetSlotRequirement>();

    public int TemplateId { get; private set; }
    public string DisplayName { get; private set; }
    public IReadOnlyList<BouquetSlotRequirement> Slots => slots;

    public BouquetOrderData(int templateId, string displayName, IEnumerable<BouquetSlotRequirement> slotRequirements)
    {
        TemplateId = templateId;
        DisplayName = displayName;
        slots.AddRange(slotRequirements);
    }
}
