using UnityEngine.Localization;
using MoreMountains.Feedbacks;
using UnityEngine;
using Febucci.UI;
using System;

public class OxygenWarning : MonoBehaviour
{
    public static OxygenWarning Instance { get; private set; }

    [SerializeField] private TextAnimator_TMP _textAnimator;
    [SerializeField] private MMF_Player _warningFeedback1;
    [SerializeField] private MMF_Player _warningFeedback2;
    [SerializeField] private MMF_Player _warningFeedback3;
    [SerializeField] private ItemSO _oxygenTankSO;
    [SerializeField] private GameObject _textGameObject;

    [Header("Localization")]
    [SerializeField] private LocalizedString _oxygenString;

    private float _lastRatio = 1f;

    private void Awake()
    {
        Instance = this;
    }

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
    }

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
    }

    private void Player_OnPlayerDied(object sender, EventArgs e)
    {
        Hide();
    }

    private void OxygenController_OnOxygenChanged(object sender, System.EventArgs e)
    {
        if (Player.Instance.IsDead)
            return;

        var ratio = OxygenController.Instance.CurrentOxygenRatio;

        if (OxygenController.Instance.OxygenUsagePerSecond < 0f || ratio > 0.3f)
        {
            _warningFeedback1.StopFeedbacks();
            _warningFeedback2.StopFeedbacks();
            _warningFeedback3.StopFeedbacks();
        }

        if (ratio < 0.1f)
        {
            if ((ratio < _lastRatio || ratio == 0f) && !_warningFeedback3.IsPlaying)
            {
                _warningFeedback1.StopFeedbacks();
                _warningFeedback2.StopFeedbacks();
                _warningFeedback3.PlayFeedbacks();
            }
            Show($"{_oxygenString.GetSmartString("oxygenUsageRate", $"{-1f * OxygenController.Instance.OxygenUsagePerSecond}/s")} <shake d=0.2 a=1.7><br><size=55>{(int)(ratio * 100f)}%</size></shake>");
        }
        else if (ratio < 0.2f)
        {
            _warningFeedback1.StopFeedbacks();
            if (ratio < _lastRatio && !_warningFeedback2.IsPlaying)
            {
                _warningFeedback1.StopFeedbacks();
                _warningFeedback2.PlayFeedbacks();
                _warningFeedback3.StopFeedbacks();
            }
            Show($"{_oxygenString.GetSmartString("oxygenUsageRate", $"{-1f * OxygenController.Instance.OxygenUsagePerSecond}/s")} <shake d=0.5 a=0.7><br><size=55>{(int)(ratio * 100f)}%</size></shake>");
        }
        else if (ratio < 0.3f)
        {
            if (ratio < _lastRatio && !_warningFeedback1.IsPlaying)
            {
                _warningFeedback1.PlayFeedbacks();
                _warningFeedback2.StopFeedbacks();
                _warningFeedback3.StopFeedbacks();
            }
            Show($"{_oxygenString.GetSmartString("oxygenUsageRate", $"{-1f * OxygenController.Instance.OxygenUsagePerSecond}/s")} <shake d=0.75 a=0.25><br><size=55>{(int)(ratio * 100f)}%</size></shake>");
        }
        else if (ratio < 1f)
        {
            Show($"{_oxygenString.GetSmartString("oxygenUsageRate", $"{-1f * OxygenController.Instance.OxygenUsagePerSecond}/s")} <shake a=0.1><br><size=55>{(int)(ratio * 100f)}%</size></shake>");
        }
        else if (ratio >= 1f)
        {
            Hide();
        }

        _lastRatio = ratio;
    }

    public void Show(string message)
    {
        _textGameObject.SetActive(true);
        _textAnimator.SetText(message);
        _textAnimator.SetVisibilityEntireText(true);
    }

    public void Hide()
    {
        _textAnimator.SetVisibilityEntireText(false);

        _warningFeedback1.StopFeedbacks();
        _warningFeedback2.StopFeedbacks();
        _warningFeedback3.StopFeedbacks();
    }
}
