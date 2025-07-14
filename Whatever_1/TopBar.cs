using UnityEngine.Localization;
using MoreMountains.Tools;
using UnityEngine.UI;
using UnityEngine;
using Febucci.UI;
using System;
using TMPro;

public class TopBar : MonoBehaviour, MMEventListener<LaserCannonShootEvent>
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _seedUI;
    [SerializeField] private TextMeshProUGUI _versionUI;
    [SerializeField] private TextMeshProUGUI _lastSaveTimerUI;
    [SerializeField] private Button _unlockRecipeMenuButton;
    [SerializeField] private Button _saveButton;
    [SerializeField] private TypewriterByCharacter _saveNotification;
    [SerializeField] private LocalizedString _localizedSaveNotification;

    private TickSystem _updateLastSaveTickSystem;

    private void OnDestroy()
    {
        MMEventManager.RemoveListener(this);

        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned -= Player_OnPlayerRespawned;
        PlayerSpawner.Instance.OnPlayerSpawned -= PlayerSpawner_OnPlayerSpawned;

        if (!GameManager.IsInstanceNull)
            GameManager.Instance.OnGameSaved -= GameManager_OnGameSaved;
    }

    private void Start()
    {
        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;

        MMEventManager.AddListener(this);

        _seedUI.text = $"Seed: {TilemapChunkSystem.Instance.Seed}";
        _versionUI.text = $"v{Application.version}";

        _unlockRecipeMenuButton.onClick.RemoveListener(OnUnlockMenuButtonClicked);
        _unlockRecipeMenuButton.onClick.AddListener(OnUnlockMenuButtonClicked);

        _updateLastSaveTickSystem = new TickSystem(1f, () =>
        {
            UpdateLastSaveTimeUI(GameManager.Instance.LastSaveTime);
        });

        _saveButton.onClick.RemoveAllListeners();
        _saveButton.onClick.AddListener(() =>
        {
            GameManager.Instance.SaveGame();
            OnGameSaved();
        });

        GameManager.Instance.OnGameSaved += GameManager_OnGameSaved;
    }

    private void OnUnlockMenuButtonClicked()
    {
        RecipeUnlockMenu.Show();
    }

    private void GameManager_OnGameSaved(object sender, EventArgs e)
    {
        OnGameSaved();
    }

    private void OnGameSaved()
    {
        _saveButton.interactable = false;

        UpdateLastSaveTimeUI(GameManager.Instance.LastSaveTime);

        _saveNotification.gameObject.SetActive(true);
        _saveNotification.ShowText($"{{fade d=0.25}}{{#fade d=0.25}}<wave a=0.2 w=0.5>{_localizedSaveNotification.GetLocalizedString()}");
        Helper.RepeatAction(1f, 0f, 1, () =>
        {
            _saveNotification.onTextDisappeared.RemoveAllListeners();
            _saveNotification.onTextDisappeared.AddListener(() =>
            {
                _saveButton.interactable = true;
                _saveNotification.gameObject.SetActive(false);
                _saveNotification.onTextDisappeared.RemoveAllListeners();
            });

            _saveNotification.StartDisappearingText();
        });
    }

    private void Update()
    {
        _updateLastSaveTickSystem.Update();
    }

    public void UpdateLastSaveTimeUI(float lastSaveTime)
    {
        var diff = Time.time - lastSaveTime;
        _lastSaveTimerUI.text = $"{Helper.GetFormattedTime(diff)}";
    }

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
    }

    private void Player_OnPlayerRespawned(object sender, EventArgs e)
    {
        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }

    private void Player_OnPlayerDied(object sender, EventArgs e)
    {
        _canvasGroup.alpha = 0f;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnMMEvent(LaserCannonShootEvent laserCannonEvent)
    {
        _canvasGroup.blocksRaycasts = !laserCannonEvent.isLaserActive;
    }
}
