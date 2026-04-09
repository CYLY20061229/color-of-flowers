using UnityEngine;

public class BackgroundView : MonoBehaviour
{
    private const string DefaultBackgroundResourcePath = "Backgrounds/Tree";
    private const int BackgroundSortingOrder = -100;

    private SpriteRenderer spriteRenderer;
    private Camera targetCamera;

    public void Initialize(Camera cameraToCover)
    {
        targetCamera = cameraToCover != null ? cameraToCover : Camera.main;
        EnsureRenderer();
        FitToCamera();
    }

    private void EnsureRenderer()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
        }

        spriteRenderer.sprite = LoadBackgroundSprite();
        spriteRenderer.sortingOrder = BackgroundSortingOrder;
        spriteRenderer.color = Color.white;
    }

    private Sprite LoadBackgroundSprite()
    {
        Sprite sprite = Resources.Load<Sprite>(DefaultBackgroundResourcePath);
        if (sprite != null)
        {
            return sprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(DefaultBackgroundResourcePath);
        if (texture == null)
        {
            Debug.LogWarning($"Background image not found at Resources/{DefaultBackgroundResourcePath}.");
            return null;
        }

        return Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            512f);
    }

    private void FitToCamera()
    {
        if (targetCamera == null || spriteRenderer.sprite == null)
        {
            return;
        }

        float cameraHeight = targetCamera.orthographicSize * 2f;
        float cameraWidth = cameraHeight * targetCamera.aspect;
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;

        if (spriteSize.x <= 0f || spriteSize.y <= 0f)
        {
            return;
        }

        float scale = Mathf.Max(cameraWidth / spriteSize.x, cameraHeight / spriteSize.y);
        transform.localScale = new Vector3(scale, scale, 1f);
        transform.position = new Vector3(targetCamera.transform.position.x, targetCamera.transform.position.y, 1f);
    }
}
