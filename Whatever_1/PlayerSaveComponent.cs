using Newtonsoft.Json;
using UnityEngine;
using System;

public class PlayerSaveComponent : MonoBehaviour
{
    [SerializeField] private PrefabSO _prefabSO;

    private SaveData _saveData;
    private bool _isPlayerSpawned;
    private bool _isInventoryLoaded;

    private void Awake()
    {
        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;
        SaveSystem.Instance.onDataPassed += OnDataPassed;
    }

    private void Start()
    {
        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;
    }

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        _isPlayerSpawned = true;

        if (_saveData != null && !_isInventoryLoaded)
        {
            LoadPlayerInventory();

            Player.Instance.SetCurrentHealth(_saveData.currentHealth);
        }

        DebugController.Instance.AddPlayerDebugItems();
    }

    private void LoadPlayerInventory()
    {
        _isInventoryLoaded = true;
        Player.Instance.Inventory.CurrentSlotBarIndex = _saveData.currentSlotBarIndex;
        PlayerInventoryUI.Instance.UpdateSelectedSlotBar(_saveData.currentSlotBarIndex);

        Player.Instance.Inventory.LoadInventoryData(_saveData.inventoryData);
        Player.Instance.EquipmentInventory.LoadInventoryData(_saveData.equipmentInventoryData);
        Player.Instance.LaserUpgradeInventory.LoadInventoryData(_saveData.laserInventoryData);

        PlayerInventoryUI.Instance.InitSlots();
    }

    private void OnDataPassed()
    {
        if (_saveData != null)
        {
            if (_isPlayerSpawned && !_isInventoryLoaded)
            {
                LoadPlayerInventory();
            }
        }
    }

    public class SaveData
    {
        public int currentHealth;
        public int currentSlotBarIndex;

        public InventoryData inventoryData;
        public InventoryData laserInventoryData;
        public InventoryData equipmentInventoryData;
    }

    private string OnSave()
    {
        if (Player.Instance == null)
            return string.Empty;

        var saveData = new SaveData();
        saveData.currentSlotBarIndex = Player.Instance.Inventory.CurrentSlotBarIndex;
        saveData.currentHealth = Player.Instance.CurrentHealth;
        saveData.inventoryData = Player.Instance.Inventory.GetInventoryData();
        saveData.laserInventoryData = Player.Instance.LaserUpgradeInventory.GetInventoryData();
        saveData.equipmentInventoryData = Player.Instance.EquipmentInventory.GetInventoryData();
        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        _saveData = JsonConvert.DeserializeObject<SaveData>(json);
    }
}
