using UnityEngine;
using TMPro;

public class UI_HyperLaserCharge : MonoBehaviour
{
    [SerializeField] private SimpleSlider _slider;
    [SerializeField] private TextMeshProUGUI _valueUI;

    private void Start()
    {

    }

    private void Update()
    {
        var charge = HyperLaserChargeController.Instance.HyperLaserCharge;
        var ratio = (float)charge / HyperLaserChargeController.HYPER_LASER_CHARGE_MAX;

        _slider.gameObject.SetActive(charge > 0f);
        _slider.SetValue(ratio);

        _valueUI.text = $"{charge}/{HyperLaserChargeController.HYPER_LASER_CHARGE_MAX}";
    }
}
