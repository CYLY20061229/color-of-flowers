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
        valueText.text = $"蓄力 {(int)(Mathf.Clamp01(normalizedCharge) * 100f)}%";
        resultText.text = string.Empty;
    }

    public void ShowResult(HarvestResult result)
    {
        resultText.text = GetResultLabel(result);
        resultText.color = GetResultColor(result);
    }

    private void EnsureVisuals()
    {
        if (panelBackground != null)
        {
            return;
        }

        panelBackground = SimpleShapeFactory.CreateRectangle("ChargePanel", transform, new Vector2(1.95f, 3.7f), new Color(0.08f, 0.08f, 0.1f, 0.92f), 30);
        chargeBarController = new GameObject("ChargeBar").AddComponent<ChargeBarController>();
        chargeBarController.transform.SetParent(transform, false);
        chargeBarController.transform.localPosition = new Vector3(0f, -0.05f, 0f);

        titleText = CreateText("TitleText", "蓄力采摘", new Vector3(0f, 1.45f, -0.02f), 0.065f, TextAnchor.MiddleCenter);
        hintText = CreateText("HintText", "按住鼠标左键并松开", new Vector3(0f, -1.5f, -0.02f), 0.05f, TextAnchor.MiddleCenter);
        valueText = CreateText("ValueText", "蓄力 0%", new Vector3(0f, 1.18f, -0.02f), 0.05f, TextAnchor.MiddleCenter);
        resultText = CreateText("ResultText", string.Empty, new Vector3(0f, -1.2f, -0.02f), 0.06f, TextAnchor.MiddleCenter);
        debugText = CreateText("DebugText", "调试：按 H 测试", new Vector3(0f, -1.75f, -0.02f), 0.04f, TextAnchor.MiddleCenter);
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

    private string GetResultLabel(HarvestResult result)
    {
        switch (result)
        {
            case HarvestResult.Perfect:
                return "完美";
            case HarvestResult.Good:
                return "不错";
            default:
                return "偏差";
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
