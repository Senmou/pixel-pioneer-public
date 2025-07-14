using MoreMountains.Feedbacks;
using UnityEngine.UI;
using UnityEngine;
using QFSW.QC;
using System;

public class LevelSelectionMenu : Menu<LevelSelectionMenu>
{
    public event EventHandler<int> OnSlotSelected;
    public event EventHandler OnGameStarted;
    public event EventHandler OnMenuClosed;

    [SerializeField] private LevelSelectionLevelSlot _levelSelectionSlotTemplate;
    [SerializeField] private Transform _levelSlotContainer;
    [SerializeField] private Button _startButton;
    [SerializeField] private Button _unlockButton;
    [SerializeField] private Button _bonusButton;
    [SerializeField] private DisplayUnlockCondition _displayUnlockCondition;
    [SerializeField] private UI_DiscoveredItems _discoveredItems;
    [SerializeField] private MMF_Player _showFeedback;
    [SerializeField] private MMF_Player _hideFeedback;
    [SerializeField] private LevelSelectionMenu_Statistics _statistics;
    [SerializeField] private LevelSelectionMenu_GlobalStatistics _globalStatistics;
    [SerializeField] private GameObject _resetWorldContainer;

    private int _selectedLevelIndex;
    private LevelSelectionLevelSlot _selectedLevelSlot;

    private new void Awake()
    {
        base.Awake();

        _levelSelectionSlotTemplate.gameObject.SetActive(false);
    }

    public static void Show()
    {
        Open();
        Instance.Init();
    }

    public static void Hide()
    {
        Instance.OnMenuClosed?.Invoke(Instance, EventArgs.Empty);
        Close();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Z))
        {
            OnUnlockButtonClicked();
        }
#endif
    }

    private void Init()
    {
        _bonusButton.gameObject.SetActive(false);

        CreateSlots();
    }

    public static void ShowWithFeedback()
    {
        Open();
        Instance.Init();
        Instance._showFeedback.PlayFeedbacks();
    }

    public static void HideWithFeedbacks()
    {
        Instance._hideFeedback.PlayFeedbacks();
    }

    private void CreateSlots()
    {
        var slotCount = 3;

        foreach (Transform child in _levelSlotContainer)
        {
            if (child == _levelSelectionSlotTemplate.transform)
                continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            var slot = Instantiate(_levelSelectionSlotTemplate, _levelSlotContainer);
            slot.Init(i, isUnlocked: GameManager.Instance.IsLevelUnlocked(i), GameManager.Instance.UnlockConditionMet(i));
            slot.gameObject.SetActive(true);

            if (i == 0)
                OnLevelSelected(slot);
        }
    }

    public void OnStartGameButtonClicked()
    {
        OnGameStarted?.Invoke(this, EventArgs.Empty);
        GameManager.Instance.Continue(_selectedLevelIndex);
    }

    public void OnUnlockButtonClicked()
    {
        GameManager.Instance.UnlockLevel(_selectedLevelIndex);

        _startButton.gameObject.SetActive(GameManager.Instance.IsLevelUnlocked(_selectedLevelIndex));
        _unlockButton.gameObject.SetActive(!GameManager.Instance.IsLevelUnlocked(_selectedLevelIndex));
        _displayUnlockCondition.gameObject.SetActive(false);

        _selectedLevelSlot.UpdateUI(GameManager.Instance.IsLevelUnlocked(_selectedLevelIndex), GameManager.Instance.UnlockConditionMet(_selectedLevelIndex));

        var unlockCondition = GameManager.Instance.GetUnlockCondition(_selectedLevelIndex);
        GlobalStats.Instance.SubCredits(unlockCondition.creditsThreshold);
    }

    public void OnResetButtonClicked()
    {
        SaveSystem.Instance.DeleteWorld(_selectedLevelIndex);
        GlobalStats.Instance.OnResetWorld(_selectedLevelIndex);
        _statistics.UpdateUI(_selectedLevelIndex);
        _globalStatistics.UpdateUI();
    }

    public void OnGetBonusButtonClicked()
    {
        _resetWorldContainer.SetActive(true);
    }

    public void OnLevelSelected(LevelSelectionLevelSlot selectedSlot)
    {
        _resetWorldContainer.SetActive(false);

        _selectedLevelSlot = selectedSlot;
        _selectedLevelIndex = selectedSlot.LevelIndex;
        _startButton.gameObject.SetActive(GameManager.Instance.IsLevelUnlocked(_selectedLevelIndex));

        var isLevelUnlocked = GameManager.Instance.IsLevelUnlocked(_selectedLevelIndex);
        var isUnlockConditionMet = GameManager.Instance.UnlockConditionMet(_selectedLevelIndex);
        _unlockButton.gameObject.SetActive(!isLevelUnlocked && isUnlockConditionMet);
        _bonusButton.gameObject.SetActive(isLevelUnlocked);

        if (isLevelUnlocked)
            _displayUnlockCondition.gameObject.SetActive(false);
        else
        {
            _displayUnlockCondition.gameObject.SetActive(true);
            _displayUnlockCondition.UpdateUI(GameManager.Instance.GetUnlockCondition(_selectedLevelIndex));
        }

        _discoveredItems.UpdateUI(_selectedLevelIndex);

        foreach (Transform child in _levelSlotContainer)
        {
            if (child == _levelSelectionSlotTemplate.transform)
                continue;

            var slot = child.GetComponent<LevelSelectionLevelSlot>();
            slot.UpdateSelectionFrame(_selectedLevelIndex);
        }

        OnSlotSelected?.Invoke(this, _selectedLevelIndex);
    }

    public override void OnBackPressed()
    {
        MenuSwitchController.Instance.MoveCameraUp();
    }

    [Command(aliasOverride: "unlock")]
    private static void UnlockLevel()
    {
        Instance.OnUnlockButtonClicked();
    }
}
