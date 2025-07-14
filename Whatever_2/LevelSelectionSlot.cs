using UnityEngine;
using TMPro;

public class LevelSelectionSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    public bool IsLevelUnlocked => _isUnlocked;

    private bool _isUnlocked;

    public void Init(string name, bool isUnlocked)
    {
        _text.text = name;
        _isUnlocked = isUnlocked;
    }
}
