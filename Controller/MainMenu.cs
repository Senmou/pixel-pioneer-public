using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;
using UnityEngine;

public class MainMenu : Menu<MainMenu>
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private MMF_Player _showFeedback;
    [SerializeField] private MMF_Player _hideFeedback;

    private void Start()
    {
        _canvas.worldCamera = Camera.main;
    }

    public static void Show()
    {
        Open();

        var eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem != null)
            eventSystem.SetSelectedGameObject(null);
    }

    public static void ShowWithFeedback()
    {
        Open();

        var eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem != null)
            eventSystem.SetSelectedGameObject(null);

        Instance._showFeedback.PlayFeedbacks();
    }

    public static void Hide()
    {
        Close();
    }

    public static void HideWithFeedbacks()
    {
        Instance._hideFeedback.PlayFeedbacks();
    }

    public override void OnBackPressed()
    {

    }

    #region Unity Events
    public void OnExitButtonPressed()
    {
        Application.Quit();
    }

    public void OnStartButtonClicked()
    {
        MenuSwitchController.Instance.MoveCameraDown();
    }

    public void OnCreditButtonPressed()
    {
        CreditMenu.Open();
    }
    #endregion
}
