using UnityEngine;

public class ChargeHarvestUIController : MonoBehaviour
{
    private GameObject panelBackground;
    private TextMesh titleText;
    private TextMesh hintText;
    private TextMesh valueText;
    private TextMesh resultText;
    private TextMesh debugText;
    private ChargeBarController chargeBarController;

    public void Initialize(ChargeHarvestConfig config)
    {
        EnsureVisuals();
        chargeBarController.Initialize(config);
        transform.position = config != null ? config.PanelWorldPosition : Vector3.zero;
        Hide();
    }

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void RefreshCharge(float normalizedCharge)
    {
        chargeBarController.SetChargeNormalized(normalizedCharge);
        valueText.text = $"Charge {(int)(Mathf.Clamp01(normalizedCharge) * 100f)}%";
        resultText.text = string.Empty;
    }

    public void ShowResult(HarvestResult result)
    {
        resultText.text = result.ToString();
        resultText.color = GetResultColor(result);
    }

    private void EnsureVisuals()
    {
        if (panelBackground != null)
        {
            return;
        }

        panelBackground = SimpleShapeFactory.CreateRectangle("ChargePanel", transform, new Vector2(2.6f, 4.8f), new Color(0.08f, 0.08f, 0.1f, 0.92f), 30);
        chargeBarController = new GameObject("ChargeBar").AddComponent<ChargeBarController>();
        chargeBarController.transform.SetParent(transform, false);
        chargeBarController.transform.localPosition = new Vector3(0f, -0.1f, 0f);

        titleText = CreateText("TitleText", "Charge Harvest", new Vector3(0f, 2.0f, -0.02f), 0.11f, TextAnchor.MiddleCenter);
        hintText = CreateText("HintText", "Hold LMB and release", new Vector3(0f, -2.0f, -0.02f), 0.08f, TextAnchor.MiddleCenter);
        valueText = CreateText("ValueText", "Charge 0%", new Vector3(0f, 1.7f, -0.02f), 0.08f, TextAnchor.MiddleCenter);
        resultText = CreateText("ResultText", string.Empty, new Vector3(0f, -1.65f, -0.02f), 0.1f, TextAnchor.MiddleCenter);
        debugText = CreateText("DebugText", "Debug: press H to test", new Vector3(0f, -2.25f, -0.02f), 0.07f, TextAnchor.MiddleCenter);
    }

    private Color GetResultColor(HarvestResult result)
    {
        switch (result)
        {
            case HarvestResult.Perfect:
                return new Color(0.3f, 0.95f, 0.45f, 1f);
            case HarvestResult.Good:
                return new Color(1f, 0.82f, 0.25f, 1f);
            default:
                return new Color(1f, 0.35f, 0.35f, 1f);
        }
    }

    private TextMesh CreateText(string objectName, string content, Vector3 localPosition, float characterSize, TextAnchor anchor)
    {
        GameObject textObject = new GameObject(objectName);
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = localPosition;

        TextMesh textMesh = textObject.AddComponent<TextMesh>();
        textMesh.text = content;
        textMesh.anchor = anchor;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = characterSize;
        textMesh.fontSize = 48;
        textMesh.color = Color.white;

        MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
        renderer.sortingOrder = 34;
        return textMesh;
    }
}
