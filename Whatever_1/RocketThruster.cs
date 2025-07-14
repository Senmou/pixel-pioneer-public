using UnityEngine;

public class RocketThruster : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particlesStage1;
    [SerializeField] private ParticleSystem _particlesStage2;
    [SerializeField] private ParticleSystem _particlesStage3;

    public void ActivateStage1()
    {
        _particlesStage1.gameObject.SetActive(true);
        _particlesStage1.Play();
    }

    public void ActivateStage2()
    {
        _particlesStage2.gameObject.SetActive(true);
        _particlesStage2.Play();
    }

    public void ActivateStage3()
    {
        _particlesStage3.gameObject.SetActive(true);
        _particlesStage3.Play();
    }
}
