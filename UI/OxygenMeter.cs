using UnityEngine;
using System;
using TMPro;

public class OxygenMeter : MonoBehaviour, ITooltip
{
    [SerializeField] private TextMeshProUGUI _oxygenAmountText;
    [SerializeField] private TextMeshProUGUI _oxygenRateText;
    [SerializeField] private SimpleSlider _slider;
    [SerializeField] private GameObject _container;

    private void Start()
    {
        OxygenController.Instance.OnOxygenChanged += OxygenController_OnOxygenChanged;
        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;
    }

    private void OnDestroy()
    {
        OxygenController.Instance.OnOxygenChanged -= OxygenController_OnOxygenChanged;
        PlayerSpawner.Instance.OnPlayerSpawned -= PlayerSpawner_OnPlayerSpawned;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned -= Player_OnPlayerRespawned;
    }

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
    }

    private void Player_OnPlayerDied(object sender, EventArgs e)
    {
        _slider.gameObject.SetActive(false);
    }

    private void Player_OnPlayerRespawned(object sender, EventArgs e)
    {
        _slider.gameObject.SetActive(true);
    }

    private void OxygenController_OnOxygenChanged(object sender, System.EventArgs e)
    {
        _slider.SetValue(OxygenController.Instance.CurrentOxygenRatio);
        _oxygenRateText.text = $"{(int)(100f * OxygenController.Instance.CurrentOxygenRatio)}% ({(-1f * OxygenController.Instance.OxygenUsagePerSecond).ToString("0.00")}/s)";

        _container.SetActive(OxygenController.Instance.CurrentOxygenRatio < 1f);
    }
}
