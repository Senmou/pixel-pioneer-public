using UnityEngine;

public class LiftMusicController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;

    public void Play()
    {
        if (!_audioSource.isPlaying)
            _audioSource.Play();
    }

    public void Pause()
    {
        _audioSource.Pause();
    }
}
