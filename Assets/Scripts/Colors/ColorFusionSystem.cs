using System.Collections.Generic;
using UnityEngine;

public static class ColorFusionSystem
{
    private static readonly Dictionary<ColorPair, FlowerColor> FusionRules = new Dictionary<ColorPair, FlowerColor>
    {
        { new ColorPair(FlowerColor.Red, FlowerColor.Green), FlowerColor.Yellow },
        { new ColorPair(FlowerColor.Red, FlowerColor.Blue), FlowerColor.Magenta },
        { new ColorPair(FlowerColor.Green, FlowerColor.Blue), FlowerColor.Cyan },
        { new ColorPair(FlowerColor.Red, FlowerColor.Cyan), FlowerColor.White },
        { new ColorPair(FlowerColor.Green, FlowerColor.Magenta), FlowerColor.White },
        { new ColorPair(FlowerColor.Blue, FlowerColor.Yellow), FlowerColor.White }
    };

    public static FlowerColor Fuse(FlowerColor source, FlowerColor target)
    {
        if (source == target)
        {
            return target;
        }

        ColorPair pair = new ColorPair(source, target);
        if (FusionRules.TryGetValue(pair, out FlowerColor result))
        {
            return result;
        }

        return GetNearestPaletteColor(MixAsRgb(source, target));
    }

    private static Color MixAsRgb(FlowerColor source, FlowerColor target)
    {
        Color sourceColor = FlowerColorPalette.ToUnityColor(source);
        Color targetColor = FlowerColorPalette.ToUnityColor(target);
        return new Color(
            Mathf.Clamp01(sourceColor.r + targetColor.r),
            Mathf.Clamp01(sourceColor.g + targetColor.g),
            Mathf.Clamp01(sourceColor.b + targetColor.b),
            1f);
    }

    private static FlowerColor GetNearestPaletteColor(Color mixedColor)
    {
        FlowerColor bestColor = FlowerColor.White;
        float bestDistance = float.MaxValue;

        foreach (FlowerColor candidate in System.Enum.GetValues(typeof(FlowerColor)))
        {
            Color candidateColor = FlowerColorPalette.ToUnityColor(candidate);
            float distance = GetColorDistance(mixedColor, candidateColor);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestColor = candidate;
            }
        }

        return bestColor;
    }

    private static float GetColorDistance(Color a, Color b)
    {
        float redDistance = a.r - b.r;
        float greenDistance = a.g - b.g;
        float blueDistance = a.b - b.b;
        return redDistance * redDistance + greenDistance * greenDistance + blueDistance * blueDistance;
    }

    private readonly struct ColorPair
    {
        private readonly FlowerColor first;
        private readonly FlowerColor second;

        public ColorPair(FlowerColor a, FlowerColor b)
        {
            if (a <= b)
            {
                first = a;
                second = b;
            }
            else
            {
                first = b;
                second = a;
            }
        }

        public override bool Equals(object obj)
        {
            return obj is ColorPair other && first == other.first && second == other.second;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int)first * 397) ^ (int)second;
            }
        }
    }
}
