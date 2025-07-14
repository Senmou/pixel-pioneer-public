using UnityEngine;
using TMPro;

public class DebugTile : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    public void SetText(string text) => _text.text = text;
}
