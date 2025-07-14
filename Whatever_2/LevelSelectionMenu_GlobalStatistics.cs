using UnityEngine;
using TMPro;

public class LevelSelectionMenu_GlobalStatistics : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _totalCredits;
    [SerializeField] private TextMeshProUGUI _totalCreditsBonus;

    private void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        _totalCredits.text = $"{GlobalStats.Instance.GetGlobalTotalCredits()}";
        _totalCreditsBonus.text = $"{100f * GlobalStats.Instance.GetGlobalBonusPercentage():0}%";
    }
}
