using UnityEngine;
using TMPro;

public class SciencePointsOverview : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _scienceProgressText;
    [SerializeField] private TextMeshProUGUI _sciencePointsText;

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        _scienceProgressText.text = ScienceController.Instance.ScienceProgressText;
        _sciencePointsText.text = $"{ScienceController.Instance.SciencePoints}";
    }
}
