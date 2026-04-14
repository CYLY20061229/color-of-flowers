using UnityEngine;

public class ChargeHarvestConfig : MonoBehaviour
{
    [Header("Timing")]
    [SerializeField] private float chargeDurationSeconds = 1.2f;
    [SerializeField] private float resultDisplaySeconds = 0.5f;
    [SerializeField] private float damageGrowthPenaltySeconds = 2f;

    [Header("Judge Zones")]
    [SerializeField] private Vector2 goodRange = new Vector2(0.35f, 0.9f);
    [SerializeField] private Vector2 perfectRange = new Vector2(0.6f, 0.78f);
    [SerializeField] private int perfectDamageRecovery = 1;
    [SerializeField] private int badDamageIncrease = 1;

    [Header("Debug")]
    [SerializeField] private bool enableDebugHotkey = true;
    [SerializeField] private KeyCode debugStartKey = KeyCode.H;

    [Header("World UI")]
    [SerializeField] private Vector3 panelWorldPosition = new Vector3(-2.15f, -0.15f, 0f);

    public float ChargeDurationSeconds => chargeDurationSeconds;
    public float ResultDisplaySeconds => resultDisplaySeconds;
    public float DamageGrowthPenaltySeconds => damageGrowthPenaltySeconds;
    public Vector2 GoodRange => goodRange;
    public Vector2 PerfectRange => perfectRange;
    public int PerfectDamageRecovery => perfectDamageRecovery;
    public int BadDamageIncrease => badDamageIncrease;
    public bool EnableDebugHotkey => enableDebugHotkey;
    public KeyCode DebugStartKey => debugStartKey;
    public Vector3 PanelWorldPosition => panelWorldPosition;
}
