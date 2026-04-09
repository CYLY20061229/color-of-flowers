using System;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    private readonly List<FlowerData> flowers = new List<FlowerData>();

    public event Action InventoryChanged;

    public IReadOnlyList<FlowerData> Flowers => flowers;

    public void AddFlower(FlowerData flower)
    {
        if (flower == null)
        {
            return;
        }

        flowers.Add(flower);
        InventoryChanged?.Invoke();
    }

    public bool TryRemoveFlower(FlowerColor color)
    {
        for (int i = 0; i < flowers.Count; i++)
        {
            if (flowers[i].Color == color)
            {
                flowers.RemoveAt(i);
                InventoryChanged?.Invoke();
                return true;
            }
        }

        return false;
    }

    public int GetCount(FlowerColor color)
    {
        int count = 0;
        for (int i = 0; i < flowers.Count; i++)
        {
            if (flowers[i].Color == color)
            {
                count++;
            }
        }

        return count;
    }
}
