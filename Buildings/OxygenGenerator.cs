using UnityEngine;

public class OxygenGenerator : BaseBuilding
{
    [SerializeField] private SimpleSlider _slider;
    [SerializeField] private OxygenParticle _oxygenParticlePrefab;
    [SerializeField] private float _baseProductionTimePerParticle;
    [SerializeField] private float _productionTimeIncPercentPerBlock;
    [SerializeField] private int _maxOxygenParticleCount;

    private float _productionTimer;
    private float _actualProductionTimePerParticle;
    private int _currentOxygenParticleCount;

    protected override void Placeable_OnPlaced(object sender, System.EventArgs e)
    {
        base.Placeable_OnPlaced(sender, e);
        _actualProductionTimePerParticle = CalcActualProductionTimePerParticle();
    }

    private void Update()
    {
        UpdateProgress();
    }

    private void UpdateProgress()
    {
        if (!IsBuildingFinished) return;
        if (_currentOxygenParticleCount >= _maxOxygenParticleCount) return;

        _productionTimer += Time.deltaTime;

        _slider.SetValue(_productionTimer / _actualProductionTimePerParticle);

        if (_productionTimer > _actualProductionTimePerParticle)
        {
            _productionTimer = 0f;
            _currentOxygenParticleCount++;

            var particlePos = Random.insideUnitCircle;
            var particle = Instantiate(_oxygenParticlePrefab, transform);
            particle.Init(this);
            particle.transform.position = transform.position + (Vector3)particlePos;
        }
    }

    public void OnParticleCollected()
    {
        _currentOxygenParticleCount--;
    }

    private float CalcActualProductionTimePerParticle()
    {
        if (transform.position.y >= 0f)
            return _baseProductionTimePerParticle;

        float totalPercentInc = _productionTimeIncPercentPerBlock * Mathf.Abs(transform.position.y) / 5f;

        return _baseProductionTimePerParticle * (1f + totalPercentInc);
    }
}
