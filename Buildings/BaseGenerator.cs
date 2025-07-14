
public class BaseGenerator : BaseBuilding, IPowerGridEntity
{
    #region IPowerGridEntity Properties
    public int PowerGridEntityId { get; set; }
    public float TotalFuelKWh => GetTotalFuelKWh();
    public float PowerProduction => _currentPowerProduction;
    public GeneratorType GeneratorType { get => GeneratorType.Dynamic; }
    public PowerGrid PowerGrid { get; set; }
    public PowerConnections Connections { get; set; }
    #endregion

    protected float _currentPowerProduction;

    public virtual float GetTotalFuelKWh()
    {
        return 0f;
    }
}
