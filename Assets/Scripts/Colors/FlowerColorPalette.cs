using UnityEngine;

public static class FlowerColorPalette
{
    public static Color ToUnityColor(FlowerColor color)
    {
        switch (color)
        {
            case FlowerColor.Red:
                return new Color(0.92f, 0.16f, 0.14f, 1f);
            case FlowerColor.Green:
                return new Color(0.18f, 0.72f, 0.28f, 1f);
            case FlowerColor.Blue:
                return new Color(0.18f, 0.38f, 0.95f, 1f);
            case FlowerColor.Yellow:
                return new Color(0.95f, 0.82f, 0.18f, 1f);
            case FlowerColor.Cyan:
                return new Color(0.14f, 0.78f, 0.9f, 1f);
            case FlowerColor.Magenta:
                return new Color(0.72f, 0.22f, 0.86f, 1f);
            case FlowerColor.White:
                return Color.white;
            default:
                return Color.gray;
        }
    }

    public static string GetDisplayName(FlowerColor color)
    {
        switch (color)
        {
            case FlowerColor.Red:
                return "Red";
            case FlowerColor.Green:
                return "Green";
            case FlowerColor.Blue:
                return "Blue";
            case FlowerColor.Yellow:
                return "Yellow";
            case FlowerColor.Cyan:
                return "Cyan";
            case FlowerColor.Magenta:
                return "Purple";
            case FlowerColor.White:
                return "White";
            default:
                return "Unknown";
        }
    }
}
