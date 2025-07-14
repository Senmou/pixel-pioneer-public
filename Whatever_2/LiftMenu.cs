using UnityEngine.UI;
using UnityEngine;

public class LiftMenu : Menu<LiftMenu>
{
    [SerializeField] private Toggle _musicToggle;

    private KinematicLift _lift;

    public static void Show(KinematicLift lift)
    {
        Open();
        Instance.Init(lift);
    }

    public static void Hide()
    {
        Close();
    }

    private void Init(KinematicLift lift)
    {
        _lift = lift;
        _musicToggle.SetIsOnWithoutNotify(_lift.PlayMusic);
        _musicToggle.onValueChanged.AddListener((e) =>
        {
            FeedbackManager.Instance.PlayToggleOnOff(e);
            _lift.SetPlayMusic(e, stopGameMusic: !e);
        });
    }

    public override void OnBackPressed()
    {
        Close();
    }
}
