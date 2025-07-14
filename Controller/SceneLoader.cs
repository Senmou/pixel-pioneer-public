using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    public event EventHandler OnGameSceneLoadedEvent;

    private void Start()
    {
        Instance = this;
    }

    private static void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.Equals("Game"))
            return;

        SceneManager.sceneLoaded -= OnGameSceneLoaded;

        Instance.OnGameSceneLoadedEvent?.Invoke(Instance, EventArgs.Empty);
    }

    private static void OnMainMenuSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!scene.name.Equals("MainMenu"))
            return;

        SceneManager.sceneLoaded -= OnMainMenuSceneLoaded;

        LevelSelectionMenu.Show();
    }

    public static void LoadGameScene()
    {
        Helper.CreateProceduralWorld = false;
        SceneManager.sceneLoaded += OnGameSceneLoaded;
        LoadScene("Game");
    }

    public static void StartNewGameScene()
    {
        SceneManager.sceneLoaded += OnGameSceneLoaded;
        LoadScene("Game");
    }

    public static void LoadMainMenuWithLevelSelectionMenu()
    {
        SceneManager.sceneLoaded += OnMainMenuSceneLoaded;
        LoadScene("MainMenu");
    }

    public static void LoadEditorScene()
    {
        LoadScene("LevelEditor");
    }

    public static void LoadMainMenuScene()
    {
        LoadScene("MainMenu");
    }

    private static void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
