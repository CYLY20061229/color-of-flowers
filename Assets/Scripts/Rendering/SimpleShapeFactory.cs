using UnityEngine;

public static class SimpleShapeFactory
{
    private const int CircleSegmentCount = 40;

    public static GameObject CreateRectangle(string name, Transform parent, Vector2 size, Color color, int sortingOrder)
    {
        GameObject shape = CreateShapeObject(name, parent, color, sortingOrder);
        shape.GetComponent<MeshFilter>().sharedMesh = CreateRectangleMesh(size);
        return shape;
    }

    public static GameObject CreateCircle(string name, Transform parent, float radius, Color color, int sortingOrder)
    {
        GameObject shape = CreateShapeObject(name, parent, color, sortingOrder);
        shape.GetComponent<MeshFilter>().sharedMesh = CreateCircleMesh(radius);
        return shape;
    }

    public static GameObject CreateTriangle(string name, Transform parent, Vector2 size, Color color, int sortingOrder)
    {
        GameObject shape = CreateShapeObject(name, parent, color, sortingOrder);
        shape.GetComponent<MeshFilter>().sharedMesh = CreateTriangleMesh(size);
        return shape;
    }

    public static void SetColor(GameObject shape, Color color)
    {
        MeshRenderer renderer = shape.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.sharedMaterial != null)
        {
            renderer.sharedMaterial.color = color;
        }
    }

    private static GameObject CreateShapeObject(string name, Transform parent, Color color, int sortingOrder)
    {
        GameObject shape = new GameObject(name);
        shape.transform.SetParent(parent, false);

        shape.AddComponent<MeshFilter>();

        MeshRenderer renderer = shape.AddComponent<MeshRenderer>();
        Material material = new Material(Shader.Find("Sprites/Default"));
        material.color = color;
        renderer.sharedMaterial = material;
        renderer.sortingOrder = sortingOrder;

        return shape;
    }

    private static Mesh CreateRectangleMesh(Vector2 size)
    {
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;

        Mesh mesh = new Mesh();
        mesh.name = "RuntimeRectangle";
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

    private static Mesh CreateTriangleMesh(Vector2 size)
    {
        float halfWidth = size.x * 0.5f;
        float halfHeight = size.y * 0.5f;

        Mesh mesh = new Mesh();
        mesh.name = "RuntimeTriangle";
        mesh.vertices = new[]
        {
            new Vector3(0f, halfHeight, 0f),
            new Vector3(-halfWidth, -halfHeight, 0f),
            new Vector3(halfWidth, -halfHeight, 0f)
        };
        mesh.triangles = new[] { 0, 1, 2 };
        mesh.RecalculateBounds();
        return mesh;
    }

    private static Mesh CreateCircleMesh(float radius)
    {
        Vector3[] vertices = new Vector3[CircleSegmentCount + 1];
        int[] triangles = new int[CircleSegmentCount * 3];

        vertices[0] = Vector3.zero;
        for (int i = 0; i < CircleSegmentCount; i++)
        {
            float angle = Mathf.PI * 2f * i / CircleSegmentCount;
            vertices[i + 1] = new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
        }

        for (int i = 0; i < CircleSegmentCount; i++)
        {
            int triangleIndex = i * 3;
            triangles[triangleIndex] = 0;
            triangles[triangleIndex + 1] = i + 1;
            triangles[triangleIndex + 2] = i == CircleSegmentCount - 1 ? 1 : i + 2;
        }

        Mesh mesh = new Mesh();
        mesh.name = "RuntimeCircle";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        return mesh;
    }
}
