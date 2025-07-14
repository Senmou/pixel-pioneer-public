using System.Collections;
using UnityEngine;
using TMPro;

public class TransitionScreen : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _loadingText;

    public void Show(float time = 0f, bool showLoadingText = true)
    {
        if (_loadingText != null)
            _loadingText.gameObject.SetActive(showLoadingText);

        StartCoroutine(ShowCo(time));
    }

    public void Hide(float time = 0f)
    {
        StartCoroutine(HideCo(time));
    }

    private IEnumerator ShowCo(float time)
    {
        _canvasGroup.gameObject.SetActive(true);
        _canvasGroup.alpha = 0f;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            _canvasGroup.alpha = t / time;
            yield return null;
        }
        _canvasGroup.alpha = 1f;
    }

    private IEnumerator HideCo(float time)
    {
        _canvasGroup.alpha = 1f;
        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            _canvasGroup.alpha = 1f - t / time;
            yield return null;
        }
        _canvasGroup.alpha = 0f;
        _canvasGroup.gameObject.SetActive(false);
    }
}
