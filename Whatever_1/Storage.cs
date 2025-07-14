using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class Storage : BaseBuilding, IPipeNetworkEntity
{
    #region IPipeNetworkEntity
    public PipeNetwork PipeNetwork { get; set; }
    public List<Vector3Int> DirectlyConnectedPipesList { get; set; }
    public List<BuildingPipeConnector> Connectors => transform.GetComponentsInChildren<BuildingPipeConnector>().ToList();
    #endregion

    private new void Awake()
    {
        base.Awake();
        OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;
    }

    private void OnDestroy()
    {
        OnFinishedBuilding -= BaseBuilding_OnFinishedBuilding;
    }

    private void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        Inventory.SetFilter((itemSO) => !itemSO.isLarge);
        PipeNetworkController.Instance.CreatePipeNetwork(this);
    }

    public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (!IsBuildingFinished)
            return;

        if (interactionType == Interactor.InteractionType.START)
        {
            Player.Instance.PlayerController.FreezePlayer();
            StorageMenu.Show(this);
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            Player.Instance.PlayerController.UnfreezePlayer();
            StorageMenu.Hide();
        }
    }

    public bool OnRequestItem(ItemSO itemSO)
    {
        if (Inventory.HasItem(itemSO, out int amount))
        {
            Inventory.RemoveItem(itemSO, 1);
            return true;
        }
        return false;
    }

    public void ReceiveItem(ItemSO itemSO)
    {
        print($"{gameObject} received {itemSO.ItemName}");
    }
}
