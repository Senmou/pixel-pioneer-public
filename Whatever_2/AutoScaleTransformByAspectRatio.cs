using UnityEngine;

public class AutoScaleTransformByAspectRatio : MonoBehaviour
{
    private void OnEnable()
    {
        if (Camera.main.aspect.IsApprox(16f / 10f, 0.05f))
            transform.localScale = Vector3.one * 0.8f;
        else
            transform.localScale = Vector3.one;
    }
}
