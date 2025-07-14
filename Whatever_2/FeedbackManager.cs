using MoreMountains.Feedbacks;
using UnityEngine;

public class FeedbackManager : MonoBehaviour
{
    public static FeedbackManager Instance { get; private set; }

    [Header("Toggles")]
    [SerializeField] private MMF_Player _toggleOnFeedback;
    [SerializeField] private MMF_Player _toggleOffFeedback;

    private void Awake()
    {
        Instance = this;
    }

    public void PlayToggleOnOff(bool isOn)
    {
        if (isOn)
            _toggleOnFeedback.PlayFeedbacks();
        else
            _toggleOffFeedback.PlayFeedbacks();
    }
}
