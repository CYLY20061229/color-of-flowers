using UnityEngine;

public class FlowerEntity : MonoBehaviour
{
    public FlowerData Data { get; private set; }

    public void Initialize(FlowerData data)
    {
        Data = data;
    }
}
