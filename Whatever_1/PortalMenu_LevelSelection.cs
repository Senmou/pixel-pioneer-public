using UnityEngine.UI;
using UnityEngine;

public class PortalMenu_LevelSelection : MonoBehaviour
{
    [SerializeField] private LevelSelectionLevelSlot _levelSelectionSlotTemplate;
    [SerializeField] private Transform _levelSlotContainer;
    [SerializeField] private Button _unlockButton;
    [SerializeField] private Button _changeWorldButton;
    [SerializeField] private DisplayUnlockCondition _displayUnlockCondition;
    [SerializeField] private UI_DiscoveredItems _discoveredItems;

    private int _selectedLevelIndex;
    private LevelSelectionLevelSlot _selectedLevelSlot;

    private void OnEnable()
    {
        _levelSelectionSlotTemplate.gameObject.SetActive(false);
        CreateSlots();
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

    public void OnLevelSelected(LevelSelectionLevelSlot selectedSlot)
    {
        _selectedLevelSlot = selectedSlot;
        _selectedLevelIndex = selectedSlot.LevelIndex;
        
        _changeWorldButton.gameObject.SetActive(GameManager.Instance.IsLevelUnlocked(_selectedLevelIndex) && GameManager.Instance.CurrentLevelIndex != _selectedLevelIndex);

        var isLevelUnlocked = GameManager.Instance.IsLevelUnlocked(_selectedLevelIndex);
        var isUnlockConditionMet = GameManager.Instance.UnlockConditionMet(_selectedLevelIndex);
        _unlockButton.gameObject.SetActive(!isLevelUnlocked && isUnlockConditionMet);

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
    }

    public void OnChangeWorldButtonClicked()
    {
        GameManager.Instance.Continue(_selectedLevelIndex);
    }

    public void OnUnlockButtonClicked()
    {
        GameManager.Instance.UnlockLevel(_selectedLevelIndex);

        _changeWorldButton.gameObject.SetActive(GameManager.Instance.IsLevelUnlocked(_selectedLevelIndex));
        _unlockButton.gameObject.SetActive(!GameManager.Instance.IsLevelUnlocked(_selectedLevelIndex));
        _displayUnlockCondition.gameObject.SetActive(false);

        _selectedLevelSlot.UpdateUI(GameManager.Instance.IsLevelUnlocked(_selectedLevelIndex), GameManager.Instance.UnlockConditionMet(_selectedLevelIndex));

        var unlockCondition = GameManager.Instance.GetUnlockCondition(_selectedLevelIndex);
        GlobalStats.Instance.SubCredits(unlockCondition.creditsThreshold);
    }
}
