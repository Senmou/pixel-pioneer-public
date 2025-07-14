using UnityEngine.UI;
using UnityEngine;

public class UnlockMenuButton : MonoBehaviour
{
    public static UnlockMenuButton Instance { get; private set; }

    [SerializeField] private DialogEventSO _showDialogEvent;
    [SerializeField] private DialogEventSO _hideDialogEvent;
    [SerializeField] private DialogEventSO _endLineEventButtonClicked;
    [SerializeField] private GameObject _tutorialArrow;
    [SerializeField] private Button _button;

    private void Awake()
    {
        Instance = this;

        _button.onClick.AddListener(OnButtonClicked);
    }

    private void OnButtonClicked()
    {
        _endLineEventButtonClicked.Trigger();
    }

    private void Start()
    {
        _showDialogEvent.Subscribe(this, OnShowUnlockButtonDialog);
        _hideDialogEvent.Subscribe(this, OnHideUnlockButtonDialog);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonClicked);
        _showDialogEvent.Unsubscribe(OnShowUnlockButtonDialog);
        _hideDialogEvent.Unsubscribe(OnHideUnlockButtonDialog);
    }

    private void OnShowUnlockButtonDialog(object sender, DialogEventSO.OnTriggerEventArgs e)
    {
        ShowTutorialArrow(true);
    }

    private void OnHideUnlockButtonDialog(object sender, DialogEventSO.OnTriggerEventArgs e)
    {
        ShowTutorialArrow(false);
    }

    private void ShowTutorialArrow(bool state) => _tutorialArrow.SetActive(state);
}
