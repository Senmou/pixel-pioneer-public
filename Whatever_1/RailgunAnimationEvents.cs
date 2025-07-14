using MoreMountains.Feedbacks;
using UnityEngine;

public class RailgunAnimationEvents : MonoBehaviour
{
    [SerializeField] private MMF_Player _warmUpFeedback;

    public void PlayWarmUpFeedback()
    {
        _warmUpFeedback.PlayFeedbacks();
    }
}
