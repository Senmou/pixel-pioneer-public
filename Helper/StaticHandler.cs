using MoreMountains.Tools;
using UnityEngine;

public class StaticHandler : MonoBehaviour
{
    private void Awake()
    {
        MMEventManager.ResetStaticListOnGameStart();
    }
}
