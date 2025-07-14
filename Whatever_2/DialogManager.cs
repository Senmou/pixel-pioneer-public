using UnityEngine;

public class DialogManager : MonoBehaviour
{
    [SerializeField] private TransitionScreen _loadingScreen;

    [Header("Intro")]
    [SerializeField] private DialogSO _intro;
    [SerializeField] private DialogEventSO _introFinishedEvent;

    private void Start()
    {
        HandleIntroDialog();
    }

    private void OnDestroy()
    {
        _introFinishedEvent.UnsubscribeAll();
    }

    private void HandleIntroDialog()
    {
        if (GameManager.Instance.IsFirstGameStart())
        {
            ShowDialog(_intro);
            _introFinishedEvent.Subscribe(this, HideLoadingScreen);
        }

        void HideLoadingScreen(object sender, DialogEventSO.OnTriggerEventArgs e)
        {
            _loadingScreen.Hide(time: 3f);
            _introFinishedEvent.Unsubscribe(HideLoadingScreen);
        }
    }

    private void ShowDialog(DialogSO dialog)
    {
        DialogController.Instance.EnqueueDialog(dialog);
    }
}
