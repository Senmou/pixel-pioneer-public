using static UnityEngine.ParticleSystem;
using UnityEngine;

public class MiningParticleSystem : MonoBehaviour
{
    [SerializeField] private ParticleSystem _particles;

    [Header("Colors by GroundType")]
    [SerializeField] private Color _dirtColor;
    [SerializeField] private Color _stoneColor;
    [SerializeField] private Color _coalColor;
    [SerializeField] private Color _copperColor;
    [SerializeField] private Color _ironColor;
    [SerializeField] private Color _goldColor;
    [SerializeField] private Color _woodColor;
    [SerializeField] private Color _iceColor;
    [SerializeField] private Color _coalDirtColor;
    [SerializeField] private Color _grassColor;
    [SerializeField] private Color _blastStoneColor;
    [SerializeField] private Color _graniteGroundColor;
    [SerializeField] private Color _treeTrunkColor;
    [SerializeField] private Color _treeLeafColor;
    [SerializeField] private Color _sandColor;

    private MainModule _mainModule;

    private void Awake()
    {
        _mainModule = _particles.main;
    }

    private void OnParticleSystemStopped()
    {
        ParticleManager.Instance.ReturnToPool(this);
    }

    public void SetRadius(float radius)
    {
        var shape = _particles.shape;
        shape.radius = radius;
    }

    public void SetEmissionCount(int count)
    {
        var emission = _particles.emission;
        var burst = new Burst();
        burst.count = count;
        emission.SetBurst(0, burst);
    }

    public void SetColor(BlockType blockType)
    {
        switch (blockType)
        {
            case BlockType.Dirt:
                _mainModule.startColor = _dirtColor;
                break;
            case BlockType.Stone:
                _mainModule.startColor = _stoneColor;
                break;
            case BlockType.TreeTrunk:
                _mainModule.startColor = _treeTrunkColor;
                break;
            case BlockType.TreeLeaf:
                _mainModule.startColor = _treeLeafColor;
                break;
            case BlockType.Coal:
                _mainModule.startColor = _coalColor;
                break;
            case BlockType.Sand:
                _mainModule.startColor = _sandColor;
                break;
            case BlockType.Gold:
                _mainModule.startColor = _goldColor;
                break;
            case BlockType.BlastStone:
                _mainModule.startColor = _blastStoneColor;
                break;
            case BlockType.Copper:
                _mainModule.startColor = _copperColor;
                break;
        }
    }

    public void Play()
    {
        _particles.Play();
    }

    public void Stop()
    {
        _particles.Stop();
    }
}
