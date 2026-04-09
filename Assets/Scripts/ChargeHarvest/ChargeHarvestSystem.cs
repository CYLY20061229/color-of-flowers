using UnityEngine;
using System;

public class ChargeHarvestSystem : MonoBehaviour
{
    private ChargeHarvestConfig config;
    private ChargeHarvestUIController uiController;
    private ChargeHarvestSession session;
    private float currentChargeTime;
    private bool isAwaitingChargePress;
    private bool isChargingInputActive;
    private bool isShowingResult;
    private float resultHideTimer;

    public event Action<BranchController, HarvestResult> ChargeHarvestFinished;

    public bool IsChargeHarvestActive => session != null && session.IsRunning;

    public void Initialize(ChargeHarvestConfig harvestConfig)
    {
        config = harvestConfig;
        session = new ChargeHarvestSession();
        EnsureUiController();
        uiController.Initialize(config);
    }

    private void Update()
    {
        if (config == null)
        {
            return;
        }

        if (!IsChargeHarvestActive && config.EnableDebugHotkey && Input.GetKeyDown(config.DebugStartKey))
        {
            StartChargeHarvest(null);
        }

        if (!IsChargeHarvestActive)
        {
            return;
        }

        if (isShowingResult)
        {
            resultHideTimer -= Time.deltaTime;
            if (resultHideTimer <= 0f)
            {
                EndCurrentSession();
            }

            return;
        }

        if (isAwaitingChargePress)
        {
            if (Input.GetMouseButtonDown(0))
            {
                isAwaitingChargePress = false;
                isChargingInputActive = true;
            }

            return;
        }

        if (isChargingInputActive && Input.GetMouseButton(0))
        {
            currentChargeTime += Time.deltaTime;
            float normalizedCharge = Mathf.Clamp01(currentChargeTime / config.ChargeDurationSeconds);
            session.SetNormalizedCharge(normalizedCharge);
            uiController.RefreshCharge(normalizedCharge);
        }

        if (isChargingInputActive && Input.GetMouseButtonUp(0))
        {
            ResolveCurrentSession();
        }
    }

    public bool StartChargeHarvest(BranchController branch)
    {
        if (IsChargeHarvestActive)
        {
            return false;
        }

        currentChargeTime = 0f;
        isAwaitingChargePress = true;
        isChargingInputActive = false;
        isShowingResult = false;
        resultHideTimer = 0f;
        session.Begin(branch);
        uiController.Show();
        uiController.RefreshCharge(0f);
        return true;
    }

    private void ResolveCurrentSession()
    {
        isChargingInputActive = false;

        HarvestResult result = HarvestResultEvaluator.Evaluate(session.NormalizedCharge, config);
        uiController.ShowResult(result);
        ChargeHarvestFinished?.Invoke(session.TargetBranch, result);

        isShowingResult = true;
        resultHideTimer = config != null ? config.ResultDisplaySeconds : 0.5f;
    }

    private void EndCurrentSession()
    {
        isAwaitingChargePress = false;
        isChargingInputActive = false;
        isShowingResult = false;
        resultHideTimer = 0f;
        session.Complete();
        uiController.Hide();
    }

    private void EnsureUiController()
    {
        if (uiController != null)
        {
            return;
        }

        GameObject uiObject = GameObject.Find("ChargeHarvestUI");
        if (uiObject == null)
        {
            uiObject = new GameObject("ChargeHarvestUI");
        }

        uiController = uiObject.GetComponent<ChargeHarvestUIController>();
        if (uiController == null)
        {
            uiController = uiObject.AddComponent<ChargeHarvestUIController>();
        }
    }
}
