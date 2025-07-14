using UnityEngine;
using TMPro;

public class LevelSelectionMenu_Statistics : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _totalCredits;
    [SerializeField] private TextMeshProUGUI _creditsBonus;
    [SerializeField] private TextMeshProUGUI _artifactsCreditsBonus;

    private void Start()
    {
        LevelSelectionMenu.Instance.OnSlotSelected += LevelSelectionMenu_OnSlotSelected;

        UpdateUI(0);
    }

    private void LevelSelectionMenu_OnSlotSelected(object sender, int levelIndex)
    {
        UpdateUI(levelIndex);
    }

    public void UpdateUI(int levelIndex)
    {
        _totalCredits.text = $"{GlobalStats.Instance.GetTotalCreditsByLevel(levelIndex)}";
        _creditsBonus.text = $"{100f * GlobalStats.Instance.GetCurrentBonusPercentage(levelIndex):0}%  ( {100f * GlobalStats.Instance.GetNextBonusPercentage(levelIndex):0}% )";
        _artifactsCreditsBonus.text = $"{100f * GlobalStats.Instance.TotalArtifactCreditsBonus:0}%";
    }
}
