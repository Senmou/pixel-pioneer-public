using UnityEngine;

public class TilePlacementParticleSystem : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particleSystem;

    private void OnParticleSystemStopped()
    {
        ParticleManager.Instance.ReturnToPool_TilePlacement(_particleSystem);
    }
}
