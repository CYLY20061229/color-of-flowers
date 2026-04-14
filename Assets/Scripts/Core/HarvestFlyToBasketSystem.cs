using System.Collections;
using UnityEngine;

public class HarvestFlyToBasketSystem : MonoBehaviour
{
    private const string FlowerSpriteResourcePath = "Flowers/grown";

    [SerializeField] private float flyDurationSeconds = 0.55f;
    [SerializeField] private float arcHeight = 0.7f;
    [SerializeField] private Vector3 startScale = new Vector3(0.34f, 0.34f, 1f);
    [SerializeField] private Vector3 endScale = new Vector3(0.22f, 0.22f, 1f);
    [SerializeField] private int sortingOrder = 60;

    private Sprite flowerSprite;

    public void PlayFly(FlowerColor color, Vector3 startWorldPosition)
    {
        BasketDisplay basketDisplay = GameManager.Instance != null ? GameManager.Instance.BasketDisplay : null;
        if (basketDisplay == null)
        {
            return;
        }

        Vector3 targetPosition = basketDisplay.GetHarvestTargetPosition();
        StartCoroutine(PlayFlyRoutine(color, startWorldPosition, targetPosition));
    }

    private IEnumerator PlayFlyRoutine(FlowerColor color, Vector3 startPosition, Vector3 targetPosition)
    {
        GameObject flyObject = new GameObject($"HarvestFly_{color}");
        SpriteRenderer renderer = flyObject.AddComponent<SpriteRenderer>();
        renderer.sprite = LoadFlowerSprite();
        renderer.color = FlowerColorPalette.ToUnityColor(color);
        renderer.sortingOrder = sortingOrder;

        flyObject.transform.position = startPosition;
        flyObject.transform.localScale = startScale;

        float elapsed = 0f;
        while (elapsed < flyDurationSeconds)
        {
            elapsed += Time.deltaTime;
            float normalized = flyDurationSeconds <= 0f ? 1f : Mathf.Clamp01(elapsed / flyDurationSeconds);

            Vector3 position = Vector3.Lerp(startPosition, targetPosition, normalized);
            position.y += arcHeight * 4f * normalized * (1f - normalized);

            flyObject.transform.position = position;
            flyObject.transform.localScale = Vector3.Lerp(startScale, endScale, normalized);
            flyObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 12f, normalized));

            yield return null;
        }

        flyObject.transform.position = targetPosition;
        Destroy(flyObject);
    }

    private Sprite LoadFlowerSprite()
    {
        if (flowerSprite != null)
        {
            return flowerSprite;
        }

        flowerSprite = Resources.Load<Sprite>(FlowerSpriteResourcePath);
        if (flowerSprite != null)
        {
            return flowerSprite;
        }

        Texture2D texture = Resources.Load<Texture2D>(FlowerSpriteResourcePath);
        if (texture == null)
        {
            return null;
        }

        flowerSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            512f);
        return flowerSprite;
    }
}
