using UnityEngine;

public class PowerIndicator : MonoBehaviour
{
    [SerializeField] private Placeable _placeable;
    [SerializeField] private GameObject _indicator;

    private IPowerGridEntity _powerEntity;
    private BaseBuilding _building;

    private void Awake()
    {
        _building = _placeable.GetComponent<BaseBuilding>();
        _powerEntity = _placeable.GetComponent<IPowerGridEntity>();
        _indicator.SetActive(false);
    }

    private void Update()
    {
        if (!_building.IsBuildingFinished)
            return;

        _indicator.SetActive(_powerEntity.NeedsPower && _powerEntity.PowerConsumption == 0f);
    }
}
