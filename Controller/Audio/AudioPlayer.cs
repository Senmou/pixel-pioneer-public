using MoreMountains.Feedbacks;
using UnityEngine;

public class AudioPlayer : MonoBehaviour
{
    [SerializeField] private MMF_Player _feedbacks;

    private MMF_Sound _feedbackAudioSourceVolume;

    private void Awake()
    {
        _feedbackAudioSourceVolume = _feedbacks.GetFeedbackOfType<MMF_Sound>();
    }

    public void PlaySound(bool allowWhilePlaying = false)
    {
        if (!_feedbacks.IsPlaying || allowWhilePlaying)
            _feedbacks.PlayFeedbacks();
    }

    public void SetVolume(float volume)
    {
        _feedbackAudioSourceVolume.MinVolume = volume;
        _feedbackAudioSourceVolume.MaxVolume = volume;
    }

    public void StopSound()
    {
        _feedbacks.StopFeedbacks();
    }
}
