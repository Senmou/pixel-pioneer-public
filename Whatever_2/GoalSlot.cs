using UnityEngine;
using TMPro;

public class GoalSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private TextMeshProUGUI _progressText;

    public void UpdateUI(string progress, string description)
    {
        _descriptionText.text = description;
        _progressText.text = progress;
    }
}
