using UnityEngine;
using TMPro;

public class ActionItemInfoText : MonoBehaviour
{
    public static ActionItemInfoText Instance { get; private set; }

    [SerializeField] private GameObject _container;
    [SerializeField] private TextMeshProUGUI _text;

    private void Awake()
    {
        Instance = this;
    }

    public void Show(string text)
    {
        _container.SetActive(true);
        _text.text = text;
    }

    public void Hide()
    {
        _container.SetActive(false);
        _text.text = string.Empty;
    }
}
