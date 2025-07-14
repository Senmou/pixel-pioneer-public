using UnityEngine.UI;
using UnityEngine;

[RequireComponent(typeof(Button))]
public class CloseMenuButton : MonoBehaviour
{
    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        _button.onClick.AddListener(() =>
        {
            if (FeedbackManager.Instance != null)
                FeedbackManager.Instance.PlayToggleOnOff(false);

            if (Interactor.Instance != null)
                Interactor.Instance.StopAllInteractions();
        });
    }
}
