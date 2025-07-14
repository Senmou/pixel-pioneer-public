using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class LifetimeCreditsUI : MonoBehaviour
{
    private TextMeshProUGUI _textUI;

    private void OnEnable()
    {
        _textUI.text = $"{GlobalStats.Instance.LifetimeCredits}";
    }

    private void Awake()
    {
        _textUI = GetComponent<TextMeshProUGUI>();
    }
}
