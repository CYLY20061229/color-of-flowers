using UnityEngine;

public static class PrototypeBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsurePrototypeEntryPoint()
    {
        if (Object.FindFirstObjectByType<GameManager>() != null)
        {
            return;
        }

        GameObject gameManagerObject = new GameObject("GameManager");
        gameManagerObject.AddComponent<GameManager>();
    }
}
