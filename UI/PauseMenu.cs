using UnityEngine;

public class PauseMenu : Menu<PauseMenu>
{
    private static bool _isActive;
    private int _showFrameCount;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _isActive = false;
    }

    public static void Show()
    {
        if (MenuManager.Instance.OpenMenuCount > 0)
            return;

        _isActive = true;
        Open();

        Time.timeScale = 0f;
        Instance._showFrameCount = Time.frameCount;
    }

    public static void Hide()
    {
        _isActive = false;
        Close();
        Time.timeScale = 1.0f;
    }

    public static void Toggle()
    {
        if (_isActive && MenuManager.Instance.IsTopMenu(Instance))
        {
            Hide();
        }
        else
        {
            Show();
        }
    }

    public override void OnBackPressed()
    {
        if (Time.frameCount - _showFrameCount > 1)
            Hide();
    }

    #region Unity Events
    public void OnMainMenuButtonClicked()
    {
        Time.timeScale = 1.0f;
        GameManager.Instance.SaveGame();
        SceneLoader.LoadMainMenuScene();
    }

    public void OnContinueButtonClicked()
    {
        Hide();
    }

    public void OnSaveGameButtonClicked()
    {
        GameManager.Instance.SaveGame();
        Hide();
    }

    public void OnExitGameButtonClicked()
    {
        GameManager.Instance.SaveGame();
        Application.Quit();
    }

    public void OnRestartTutorialButtonClicked()
    {
        TutorialController.Instance.RestartTutorials();
        GoalController.Instance.ResetGoals();
        Hide();
    }
    #endregion
}
