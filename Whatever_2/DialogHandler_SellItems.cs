using UnityEngine;
using System;

public class DialogHandler_SellItems : MonoBehaviour
{
    [SerializeField] private DialogSO _dialog;

    private int _itemsCollected;

    private void Start()
    {
        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;
    }

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        Player.Instance.Inventory.OnItemCollected += Inventory_OnItemCollected;
    }

    private void OnDestroy()
    {
        PlayerSpawner.Instance.OnPlayerSpawned -= PlayerSpawner_OnPlayerSpawned;
        Player.Instance.Inventory.OnItemCollected -= Inventory_OnItemCollected;
    }

    private void Inventory_OnItemCollected(object sender, Inventory.OnItemCollectedEventArgs e)
    {
        _itemsCollected++;

        if (_itemsCollected == 10)
            DialogController.Instance.EnqueueDialog(_dialog);
    }
}
