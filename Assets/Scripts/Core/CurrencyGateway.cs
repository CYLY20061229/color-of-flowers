using System;
using UnityEngine;

public class CurrencyGateway : MonoBehaviour
{
    [SerializeField] private int startingCoins = 180;
    [SerializeField] private Vector3 hudPosition = new Vector3(0f, 5.25f, 0f);

    private bool initialized;
    private TextMesh coinsLabel;
    private GameObject background;

    public event Action<int> CoinsChanged;

    public int CurrentCoins { get; private set; }

    public void Initialize()
    {
        if (!initialized)
        {
            CurrentCoins = Mathf.Max(0, startingCoins);
            initialized = true;
        }

        EnsureVisuals();
        RefreshView();
    }

    public bool CanAfford(int amount)
    {
        return CurrentCoins >= Mathf.Max(0, amount);
    }

    public bool TrySpend(int amount)
    {
        int cost = Mathf.Max(0, amount);
        if (CurrentCoins < cost)
        {
            return false;
        }

        CurrentCoins -= cost;
        RefreshView();
        CoinsChanged?.Invoke(CurrentCoins);
        return true;
    }

    public void AddCoins(int amount)
    {
        int gain = Mathf.Max(0, amount);
        if (gain <= 0)
        {
            return;
        }

        CurrentCoins += gain;
        RefreshView();
        CoinsChanged?.Invoke(CurrentCoins);
    }

    private void EnsureVisuals()
    {
        transform.position = hudPosition;

        if (background == null)
        {
            background = SimpleShapeFactory.CreateRectangle(
                "CoinsBackground",
                transform,
                new Vector2(1.7f, 0.46f),
                new Color(0.12f, 0.16f, 0.2f, 0.86f),
                45);
        }

        if (coinsLabel != null)
        {
            return;
        }

        GameObject labelObject = new GameObject("CoinsLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = new Vector3(0f, 0f, -0.01f);

        coinsLabel = labelObject.AddComponent<TextMesh>();
        coinsLabel.anchor = TextAnchor.MiddleCenter;
        coinsLabel.alignment = TextAlignment.Center;
        coinsLabel.characterSize = 0.043f;
        coinsLabel.fontSize = 48;
        coinsLabel.color = Color.white;

        MeshRenderer renderer = coinsLabel.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 46;
    }

    private void RefreshView()
    {
        if (coinsLabel == null)
        {
            return;
        }

        coinsLabel.text = $"金币 {CurrentCoins}";
    }
}
