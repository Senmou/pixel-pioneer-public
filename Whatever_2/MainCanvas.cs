using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    public static MainCanvas Instance { get; private set; }

    public Canvas Canvas => _canvas;
    public CanvasGroup CanvasGroup => _canvasGroup;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        Instance = this;
        _canvas = GetComponent<Canvas>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }
}
