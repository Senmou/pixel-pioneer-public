using UnityEngine;

public class DialogHandler_UnlockMenu : MonoBehaviour
{
    [SerializeField] private DialogSO _unlockMenuDialog;

    private void Start()
    {
        GlobalStats.Instance.OnCreditsChanged += GlobalStats_OnCreditsChanged;
    }

    private void OnDestroy()
    {
        GlobalStats.Instance.OnCreditsChanged -= GlobalStats_OnCreditsChanged;
    }

    private void GlobalStats_OnCreditsChanged(object sender, float e)
    {
        DialogController.Instance.EnqueueDialog(_unlockMenuDialog);
    }
}
