using UnityEngine;
using TMPro;

public class DisplayUnlockCondition : MonoBehaviour
{
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private ItemSlot _slotTemplate;
    [SerializeField] private GameObject _creditsCheckmark;
    [SerializeField] private TextMeshProUGUI _creditsThresholdUI;
    [SerializeField] private TextMeshProUGUI _currentCredits;

    private void Awake()
    {
        _slotTemplate.gameObject.SetActive(false);
    }

    public void UpdateUI(LevelUnlockConditionSO unlockCondition)
    {
        foreach (Transform child in _slotContainer)
        {
            if (child.transform != _slotTemplate.transform)
                Destroy(child.gameObject);
        }

        if (unlockCondition == null)
        {
            Debug.LogWarning("Unlock condition is null");
            return;
        }

        foreach (var item in unlockCondition.itemsToCraft)
        {
            var slot = Instantiate(_slotTemplate, _slotContainer);
            slot.gameObject.SetActive(true);

            GlobalStats.Instance.CraftedItems.TryGetValue(item.Key, out int availableAmount);

            slot.UpdateUI(item.Key, $"{availableAmount}/{item.Value}");
        }

        _currentCredits.text = $"{GlobalStats.Instance.Credits}";
        _creditsThresholdUI.text = $"/ {unlockCondition.creditsThreshold}";
        _creditsCheckmark.SetActive(unlockCondition.IsCreditConditionMet());
    }
}
