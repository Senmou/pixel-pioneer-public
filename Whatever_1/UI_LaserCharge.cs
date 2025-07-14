using UnityEngine;
using TMPro;

public class UI_LaserCharge : MonoBehaviour
{
    [SerializeField] private SimpleSlider _slider;
    [SerializeField] private TextMeshProUGUI _valueUI;

    private void Update()
    {
        var charge = LaserChargeController.Instance.CurrentChargeNormalized;
        _slider.gameObject.SetActive(charge < 1f);
        _slider.SetValue(charge);

        var prefix = LaserChargeController.Instance.CurrentChargeRate > 0 ? "+" : "";
        _valueUI.text = $"{prefix}{LaserChargeController.Instance.CurrentChargeRate:0.0}/s";
    }
}
