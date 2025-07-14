using MoreMountains.Feedbacks;
using UnityEngine;

public class LifeUI_Heart : MonoBehaviour
{
    [SerializeField] private MMF_Player _breakFeedback;
    [SerializeField] private MMF_Player _restoreFeedback;
    [SerializeField] private CanvasGroup _canvasGroup;

    private bool _isFull = true;

    public void UpdateUI(bool isFull, bool showAnimation = true)
    {
        if (_isFull == isFull)
            return;

        _isFull = isFull;

        if (isFull)
        {
            if (showAnimation)
                _restoreFeedback.PlayFeedbacks();
            else
                _canvasGroup.alpha = 1f;
        }
        else
        {
            if (showAnimation)
                _breakFeedback.PlayFeedbacks();
            else
                _canvasGroup.alpha = 0f;
        }
    }
}
