using UnityEngine.UI;
using UnityEngine;

public class SimpleSlider : MonoBehaviour
{
    [SerializeField] private Image _fillImage;

    public float Value => _fillImage.fillAmount;

    public void SetValue(float value)
    {
        _fillImage.fillAmount = value;
    }
}
