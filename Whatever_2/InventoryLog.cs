using System.Collections.Generic;
using UnityEngine;
using System;

public class InventoryLog : MonoBehaviour
{
    [SerializeField] private GameObject _container;
    [SerializeField] private InventoryLogPanel _panelTemplate;
    [SerializeField] private AudioPlayer _newItemAudioPlayer;

    private Dictionary<ItemSO, InventoryLogPanel> _activeObjects;

    private void Awake()
    {
        _activeObjects = new Dictionary<ItemSO, InventoryLogPanel>();
        _panelTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;
    }

    private void OnDestroy()
    {
        PlayerSpawner.Instance.OnPlayerSpawned -= PlayerSpawner_OnPlayerSpawned;
        Player.Instance.Inventory.OnItemCollected -= Inventory_OnItemCollected;
    }

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        Player.Instance.Inventory.OnItemCollected += Inventory_OnItemCollected;
    }

    private void Inventory_OnItemCollected(object sender, Inventory.OnItemCollectedEventArgs e)
    {
        _container.SetActive(true);
        InventoryLogPanel panel = null;
        _activeObjects.TryGetValue(e.inventoryItem.ItemSO, out panel);

        if (panel == null)
        {
            panel = Instantiate(_panelTemplate, _container.transform);
            panel.transform.SetAsLastSibling();
            _activeObjects.Add(e.inventoryItem.ItemSO, panel);
        }

        panel.gameObject.SetActive(true);
        panel.UpdateUI(e.inventoryItem.ItemSO, e.inventoryItem.StackSize, e.isNewItem);

        if (e.isNewItem)
            _newItemAudioPlayer.PlaySound(true);
    }

    public void RemoveFromActiveObjects(ItemSO item)
    {
        if (item == null)
            return;

        if (_activeObjects.ContainsKey(item))
            _activeObjects.Remove(item);
    }
}
