using UnityEngine.Pool;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
    public static ParticleManager Instance { get; private set; }

    [SerializeField] private MiningParticleSystem _miningParticleSystemPrefab;
    [SerializeField] private ParticleSystem _miningDustParticleSystemPrefab;
    [SerializeField] private ParticleSystem _tilePlacementParticleSystemPrefab;
    [SerializeField] private UraniumRadiationParticleSystem _uraniumRadiationParticleSystemPrefab;

    private ObjectPool<MiningParticleSystem> _miningParticleSystemPool;
    private ObjectPool<ParticleSystem> _miningDustParticleSystemPool;
    private ObjectPool<ParticleSystem> _tilePlacementParticleSystemPool;
    private ObjectPool<UraniumRadiationParticleSystem> _uraniumRadiationParticleSystemPool;

    private UraniumRadiationParticleSystem _uraniumRadiationParticleSystem;

    private void Awake()
    {
        Instance = this;
        _miningParticleSystemPool = new ObjectPool<MiningParticleSystem>
            (
                createFunc: () => Instantiate(_miningParticleSystemPrefab),
                actionOnGet: (ps) =>
                {
                    ps.Stop();
                    ps.gameObject.SetActive(true);
                },
                actionOnRelease: (ps) =>
                {
                    ps.Stop();
                    ps.gameObject.SetActive(false);
                },
                actionOnDestroy: (ps) => Destroy(ps.gameObject),
                collectionCheck: true
            );

        _miningDustParticleSystemPool = new ObjectPool<ParticleSystem>
            (
                createFunc: () => Instantiate(_miningDustParticleSystemPrefab),
                actionOnGet: (ps) =>
                {
                    ps.Stop();
                    ps.gameObject.SetActive(true);
                },
                actionOnRelease: (ps) =>
                {
                    ps.Stop();
                    ps.gameObject.SetActive(false);
                },
                actionOnDestroy: (ps) => Destroy(ps.gameObject),
                collectionCheck: true
            );

        _tilePlacementParticleSystemPool = new ObjectPool<ParticleSystem>
            (
                createFunc: () => Instantiate(_tilePlacementParticleSystemPrefab),
                actionOnGet: (ps) =>
                {
                    ps.Stop();
                    ps.gameObject.SetActive(true);
                },
                actionOnRelease: (ps) =>
                {
                    ps.Stop();
                    ps.gameObject.SetActive(false);
                },
                actionOnDestroy: (ps) => Destroy(ps.gameObject),
                collectionCheck: true
            );

        _uraniumRadiationParticleSystemPool = new ObjectPool<UraniumRadiationParticleSystem>
            (
                createFunc: () => Instantiate(_uraniumRadiationParticleSystemPrefab),
                actionOnGet: (ps) =>
                {
                    ps.Stop();
                    ps.gameObject.SetActive(true);
                },
                actionOnRelease: (ps) =>
                {
                    ps.Stop();
                    ps.gameObject.SetActive(false);
                },
                maxSize: 1,
                actionOnDestroy: (ps) => Destroy(ps.gameObject),
                collectionCheck: true
            );

        _uraniumRadiationParticleSystem = Instantiate(_uraniumRadiationParticleSystemPrefab);
    }

    public MiningParticleSystem GetParticleSystem(BlockType blockType, int emissionCount = 5, float radius = 0.0001f)
    {
        var ps = _miningParticleSystemPool.Get();
        ps.SetRadius(radius);
        ps.SetEmissionCount(emissionCount);
        ps.SetColor(blockType);
        return ps;
    }

    public ParticleSystem GetDustParticleSystem()
    {
        return _miningDustParticleSystemPool.Get();
    }

    public ParticleSystem GetTilePlacementParticleSystem()
    {
        return _tilePlacementParticleSystemPool.Get();
    }

    public UraniumRadiationParticleSystem GetUraniumRadiationParticleSystem()
    {
        return _uraniumRadiationParticleSystem;
    }

    public void ReturnToPool(MiningParticleSystem particleSystem)
    {
        _miningParticleSystemPool.Release(particleSystem);
    }

    public void ReturnToPool_Dust(ParticleSystem particleSystem)
    {
        _miningDustParticleSystemPool.Release(particleSystem);
    }

    public void ReturnToPool_TilePlacement(ParticleSystem particleSystem)
    {
        _tilePlacementParticleSystemPool.Release(particleSystem);
    }

    public void ReturnToPool_UraniumRadiation(UraniumRadiationParticleSystem particleSystem)
    {
        //_uraniumRadiationParticleSystemPool.Release(particleSystem);
    }
}
