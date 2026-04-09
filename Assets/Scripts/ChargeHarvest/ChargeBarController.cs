using UnityEngine;

public class ChargeBarController : MonoBehaviour
{
    private const float BarWidth = 0.42f;
    private const float BarHeight = 3.2f;

    private GameObject badZoneLow;
    private GameObject goodZoneLow;
    private GameObject perfectZone;
    private GameObject goodZoneHigh;
    private GameObject badZoneHigh;
    private GameObject fillBar;
    private GameObject frame;

    public void Initialize(ChargeHarvestConfig config)
    {
        EnsureVisuals();
        ApplyZones(config);
        SetChargeNormalized(0f);
    }

    public void SetChargeNormalized(float normalizedCharge)
    {
        float clampedCharge = Mathf.Clamp01(normalizedCharge);
        float fillHeight = BarHeight * clampedCharge;
        fillBar.transform.localScale = new Vector3(1f, clampedCharge, 1f);
        fillBar.transform.localPosition = new Vector3(0f, -BarHeight * 0.5f + fillHeight * 0.5f, -0.01f);
    }

    private void EnsureVisuals()
    {
        if (frame != null)
        {
            return;
        }

        frame = SimpleShapeFactory.CreateRectangle("Frame", transform, new Vector2(BarWidth + 0.12f, BarHeight + 0.12f), new Color(0.95f, 0.95f, 0.95f, 1f), 31);
        CreateZoneVisuals();
        fillBar = SimpleShapeFactory.CreateRectangle("FillBar", transform, new Vector2(BarWidth - 0.12f, BarHeight), new Color(1f, 1f, 1f, 0.55f), 33);
    }

    private void CreateZoneVisuals()
    {
        badZoneLow = SimpleShapeFactory.CreateRectangle("BadZoneLow", transform, new Vector2(BarWidth, 0.25f), new Color(0.76f, 0.24f, 0.2f, 0.85f), 32);
        goodZoneLow = SimpleShapeFactory.CreateRectangle("GoodZoneLow", transform, new Vector2(BarWidth, 0.25f), new Color(0.9f, 0.67f, 0.21f, 0.85f), 32);
        perfectZone = SimpleShapeFactory.CreateRectangle("PerfectZone", transform, new Vector2(BarWidth, 0.25f), new Color(0.25f, 0.76f, 0.38f, 0.95f), 32);
        goodZoneHigh = SimpleShapeFactory.CreateRectangle("GoodZoneHigh", transform, new Vector2(BarWidth, 0.25f), new Color(0.9f, 0.67f, 0.21f, 0.85f), 32);
        badZoneHigh = SimpleShapeFactory.CreateRectangle("BadZoneHigh", transform, new Vector2(BarWidth, 0.25f), new Color(0.76f, 0.24f, 0.2f, 0.85f), 32);
    }

    private void ApplyZones(ChargeHarvestConfig config)
    {
        Vector2 goodRange = config != null ? config.GoodRange : new Vector2(0.35f, 0.9f);
        Vector2 perfectRange = config != null ? config.PerfectRange : new Vector2(0.6f, 0.78f);

        SetZoneRect(badZoneLow, 0f, goodRange.x);
        SetZoneRect(goodZoneLow, goodRange.x, perfectRange.x);
        SetZoneRect(perfectZone, perfectRange.x, perfectRange.y);
        SetZoneRect(goodZoneHigh, perfectRange.y, goodRange.y);
        SetZoneRect(badZoneHigh, goodRange.y, 1f);
    }

    private void SetZoneRect(GameObject zone, float minNormalized, float maxNormalized)
    {
        float height = Mathf.Max(0.05f, (maxNormalized - minNormalized) * BarHeight);
        float centerY = -BarHeight * 0.5f + (minNormalized * BarHeight) + height * 0.5f;

        MeshFilter filter = zone.GetComponent<MeshFilter>();
        filter.sharedMesh = CreateZoneMesh(height);
        zone.transform.localPosition = new Vector3(0f, centerY, 0f);
    }

    private Mesh CreateZoneMesh(float height)
    {
        float halfWidth = BarWidth * 0.5f;
        float halfHeight = height * 0.5f;

        Mesh mesh = new Mesh();
        mesh.name = "ChargeZone";
        mesh.vertices = new[]
        {
            new Vector3(-halfWidth, -halfHeight, 0f),
            new Vector3(halfWidth, -halfHeight, 0f),
            new Vector3(-halfWidth, halfHeight, 0f),
            new Vector3(halfWidth, halfHeight, 0f)
        };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateBounds();
        return mesh;
    }
}
