using UnityEngine;

public class ParticleSystemHandler : MonoBehaviour
{
    private ParticleSystem[] _particleSystems;

    private void Awake()
    {
        _particleSystems = GetComponentsInChildren<ParticleSystem>();
    }

    private void Update()
    {
        bool autoDestroy = true;
        foreach (var p in _particleSystems)
        {
            if (p != null && p.isEmitting)
            {
                autoDestroy = false;
                break;
            }
        }

        if (autoDestroy)
        {
            Destroy(gameObject);
        }
    }
}
