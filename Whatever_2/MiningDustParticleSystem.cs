using UnityEngine;

public class MiningDustParticleSystem : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;

    private void OnParticleSystemStopped()
    {
        ParticleManager.Instance.ReturnToPool_Dust(_particleSystem);
    }
}
