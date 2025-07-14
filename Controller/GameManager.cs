using UnityEngine.SceneManagement;
using System.Collections.Generic;
using System.Collections;
using TarodevController;
using Newtonsoft.Json;
using UnityEngine;
using System.Linq;
using System.IO;
using System;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                new GameObject("GameManager", typeof(GameManager));
                Debug.Log("Created new GameManager");
            }
            return _instance;
        }
    }
    public static bool IsInstanceNull => _instance == null;

    public event EventHandler OnGameSaved;
    public event EventHandler<int> OnLevelStarted;

    [SerializeField] private List<LevelUnlockConditionSO> _unlockConditionList;
    [SerializeField] private TransitionScreen _respawnTransitionScreen;

    public int CurrentLevelIndex => _currentLevelIndex;
    public float LastSaveTime => _lastSaveTime;
    public TransitionScreen RespawnTransitionScreen => _respawnTransitionScreen;

    private SaveData _saveData;
    private MyPlayerInputActions _playerInputActions;
    private List<int> _unlockedLevelList = new();
    private int _currentLevelIndex;
    private float _lastSaveTime;
    private TickSystem _autoSaveTickSystem;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            Debug.Log("Destroyed obsolete GameManager");
            return;
        }
        _instance = this;
        _playerInputActions = new MyPlayerInputActions();
        _playerInputActions.Enable();

        _autoSaveTickSystem = new(600f, () => SaveGame());

        DontDestroyOnLoad(gameObject);

        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;
    }

    public bool UnlockConditionMet(int levelIndex)
    {
        var unlockConditionSO = _unlockConditionList.FirstOrDefault(e => e.levelIndex == levelIndex);

        if (unlockConditionSO != null)
        {
            return unlockConditionSO.IsItemConditionMet(GlobalStats.Instance.CraftedItems) && unlockConditionSO.IsCreditConditionMet();
        }

        //Debug.LogWarning($"No unlock condition for level index {levelIndex} found!");
        return false;
    }

    public LevelUnlockConditionSO GetUnlockCondition(int levelIndex)
    {
        return _unlockConditionList.FirstOrDefault(e => e.levelIndex == levelIndex);
    }

    private void Start()
    {
        SaveSystem.Instance.Load();

        if (_unlockedLevelList.Count == 0)
            _unlockedLevelList.Add(0);
    }

    private void Update()
    {
        _autoSaveTickSystem.Update();

        if (InputController.Instance.WasPressed_Escape)
        {
            PauseMenu.Toggle();
        }

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            Continue(0);
        }

        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            Continue(1);
        }

        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            Continue(2);
        }
    }

    public void UnlockLevel(int levelIndex)
    {
        if (!_unlockedLevelList.Contains(levelIndex))
            _unlockedLevelList.Add(levelIndex);

        SaveSystem.Instance.Save();
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        return _unlockedLevelList.Contains(levelIndex);
    }

    public int LevelCount() => _unlockedLevelList.Count;

    public void Continue(int levelIndex)
    {
        _currentLevelIndex = levelIndex;

        SceneLoader.Instance.OnGameSceneLoadedEvent += SceneLoader_OnGameSceneLoaded;
        SceneLoader.LoadGameScene();

        OnLevelStarted?.Invoke(this, levelIndex);
    }

    public bool IsFirstGameStart()
    {
        var levelLoadedCounter = GlobalStats.Instance.LoadedLevelCounterDict[0];
        return levelLoadedCounter == 1;
    }

    private void SceneLoader_OnGameSceneLoaded(object sender, EventArgs e)
    {
        SceneLoader.Instance.OnGameSceneLoadedEvent -= SceneLoader_OnGameSceneLoaded;
        StartCoroutine(LoadWorldCo());
    }

    private IEnumerator LoadWorldCo()
    {
        yield return null;
        TilemapChunkSystem.Instance.CreateOrLoadWorld(_currentLevelIndex, out bool failedToLoadWorld);

        if (!failedToLoadWorld)
        {
            SaveSystem.Instance.Load();
        }
    }

    public void SaveGame()
    {
        if (!SceneManager.GetActiveScene().name.Equals("Game"))
            return;

        SaveSystem.Instance.Save();

        OnGameSaved?.Invoke(this, EventArgs.Empty);
    }

    public string GetGlobalSavePath() => Application.persistentDataPath;
    public string GetCurrentLevelSavePath() => Path.Combine(Application.persistentDataPath, $"Level_{_currentLevelIndex}");

    public void SaveAndShowLevelSelection()
    {
        SceneLoader.LoadMainMenuWithLevelSelectionMenu();
    }

    public class SaveData
    {
        public List<int> unlockedLevelList = new();
    }

    private string OnSave()
    {
        _lastSaveTime = Time.time;

        var saveData = new SaveData();
        saveData.unlockedLevelList = _unlockedLevelList;
        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        _saveData = JsonConvert.DeserializeObject<SaveData>(json);
        _unlockedLevelList = _saveData.unlockedLevelList;
    }
}
