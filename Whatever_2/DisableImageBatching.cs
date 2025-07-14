
using UnityEngine.UI;
using UnityEngine;

public class DisableImageBatching : MonoBehaviour
{
    private void Awake()
    {
        if (TryGetComponent(out Image image))
        {
            image.material = new Material(image.material);
        }
    }
}
