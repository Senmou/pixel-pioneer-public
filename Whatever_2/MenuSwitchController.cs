using MoreMountains.Feedbacks;
using UnityEngine;

public class MenuSwitchController : MonoBehaviour
{
    public static MenuSwitchController Instance { get; private set; }

    [SerializeField] private MMF_Player _moveUpFeedback;
    [SerializeField] private MMF_Player _moveDownFeedback;

    private void Awake()
    {
        Instance = this;
    }

    public void MoveCameraUp()
    {
        _moveUpFeedback.PlayFeedbacks();
    }

    public void MoveCameraDown()
    {
        _moveDownFeedback.PlayFeedbacks();
    }

    public void ShowMainMenu(bool state)
    {
        if (state)
            MainMenu.ShowWithFeedback();
        else
            MainMenu.HideWithFeedbacks();
    }

    public void ShowLevelSelection(bool state)
    {
        if (state)
            LevelSelectionMenu.ShowWithFeedback();
        else
            LevelSelectionMenu.HideWithFeedbacks();
    }
}