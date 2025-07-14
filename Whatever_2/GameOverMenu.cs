using UnityEngine;

public class GameOverMenu : Menu<GameOverMenu>
{
    public static void Show()
    {
        Open();
    }

    public void OnMainMenuButtonClicked()
    {
        SceneLoader.LoadMainMenuScene();
        GameMusicController.Instance.ResetMusicVolume();
    }

    public void OnExitGameButtonClicked()
    {
        Application.Quit();
    }

    public override void OnBackPressed()
    {

    }
}
