using System;

[Serializable]
public class FlowerData
{
    public FlowerColor Color { get; private set; }

    public FlowerData(FlowerColor color)
    {
        Color = color;
    }
}
