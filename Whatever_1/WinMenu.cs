using UnityEngine;

public class WinMenu : Menu<WinMenu>
{
    public static void Show()
    {
        Open();
    }

    public void OnContinueButtonClick()
    {
        Close();

        GameManager.Instance.SaveAndShowLevelSelection();
    }

    public void OnExitGameButtonClicked()
    {
        Application.Quit();
    }

    public override void OnBackPressed()
    {

    }
}
