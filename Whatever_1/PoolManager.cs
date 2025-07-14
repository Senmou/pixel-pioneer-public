using UnityEngine;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }
}
