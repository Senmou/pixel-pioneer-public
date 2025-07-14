using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class LevelSelectionLevelSlot : MonoBehaviour
{
    [SerializeField] private Image _lock;
    [SerializeField] private TextMeshProUGUI _numberText;
    [SerializeField] private Sprite _lockedLockSprite;
    [SerializeField] private Sprite _unlockedLockSprite;
    [SerializeField] private GameObject _selectionFrame;

    private int _levelIndex;
    public int LevelIndex => _levelIndex;

    public void Init(int levelIndex, bool isUnlocked, bool isUnlockConditionMet)
    {
        _selectionFrame.gameObject.SetActive(false);

        _levelIndex = levelIndex;

        var unlockCondition = GameManager.Instance.GetUnlockCondition(levelIndex);
        if (unlockCondition == null)
            _lock.color = Color.black;

        _lock.gameObject.SetActive(!isUnlocked);
        _lock.sprite = isUnlockConditionMet ? _unlockedLockSprite : _lockedLockSprite;

        _numberText.gameObject.SetActive(isUnlocked);
        _numberText.text = $"{levelIndex + 1}";
    }

    public void UpdateUI(bool isUnlocked, bool isUnlockConditionMet)
    {
        _lock.gameObject.SetActive(!isUnlocked);
        _numberText.gameObject.SetActive(isUnlocked);
        _lock.sprite = isUnlockConditionMet ? _unlockedLockSprite : _lockedLockSprite;
    }

    public void UpdateSelectionFrame(int selectedSlotIndex)
    {
        _selectionFrame.SetActive(_levelIndex == selectedSlotIndex);
    }
}
