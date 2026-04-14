using System;
using UnityEngine;

[Serializable]
public class BasketDisplaySlot
{
    [SerializeField] private Transform anchor;

    [NonSerialized] private FlowerData flowerData;
    [NonSerialized] private BasketFlowerView flowerView;

    public Transform Anchor => anchor;
    public FlowerData FlowerData => flowerData;
    public BasketFlowerView FlowerView => flowerView;
    public bool IsOccupied => flowerData != null && flowerView != null;

    public BasketDisplaySlot(Transform slotAnchor)
    {
        anchor = slotAnchor;
    }

    public void SetAnchor(Transform slotAnchor)
    {
        anchor = slotAnchor;
    }

    public void Occupy(FlowerData flower, BasketFlowerView view)
    {
        flowerData = flower;
        flowerView = view;
    }

    public void Release()
    {
        flowerData = null;
        flowerView = null;
    }
}
