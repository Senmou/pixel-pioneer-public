using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Sawmill : BaseProductionBuilding, IPowerGridEntity, IPipeNetworkEntity
{
    [SerializeField] private float _powerConsumption;
    [SerializeField] private ItemSO _requestItemSO;

    #region IPowerGridEntity
    public int PowerGridEntityId { get; set; }
    public float PowerConsumption => _powerConsumption;
    public PowerGrid PowerGrid { get; set; }
    public PowerConnections Connections { get; set; }
    #endregion

    #region IPipeNetworkEntity
    public PipeNetwork PipeNetwork { get; set; }
    public List<BuildingPipeConnector> Connectors => transform.GetComponentsInChildren<BuildingPipeConnector>().ToList();
    #endregion

    private new void Update()
    {
        base.Update();
       
    }

    protected override void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        base.BaseBuilding_OnFinishedBuilding(sender, e);

        PipeNetworkController.Instance.CreatePipeNetwork(this);
    }

    public void OnRemovedFromPowerGrid() { }

    public bool OnRequestItem(ItemSO itemSO)
    {
        return false;
    }

    public void ReceiveItem(ItemSO itemSO)
    {
        print($"{gameObject} received {itemSO.ItemName}");
    }
}
