using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class PlayerInventoryUI : MonoBehaviour, MMEventListener<LaserCannonShootEvent>
{
    public EventHandler<OnSlotSelectedEventArgs> OnSlotSelected;
    public class OnSlotSelectedEventArgs : EventArgs
    {
        public InventorySlot slot;
    }

    public static PlayerInventoryUI Instance { get; private set; }

    [SerializeField] private GameObject _container;
    [SerializeField] private List<InventorySlot> _slots;
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _currentSlotBarText;

    public InventorySlot GetSelectedSlot()
    {
        return _slots[_selectedSlot];
    }

    private int _selectedSlot;
    private int _lastSelectedSlot = -1;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MMEventManager.AddListener(this);

        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;
        InputController.Instance.OnScrollWheel += InputController_OnScrollWheel;
        InputController.Instance.OnCtrlScrollWheel += InputController_OnCtrlScrollWheel;
    }

    private void OnDestroy()
    {
        MMEventManager.RemoveListener(this);

        PlayerSpawner.Instance.OnPlayerSpawned -= PlayerSpawner_OnPlayerSpawned;
        InputController.Instance.OnScrollWheel -= InputController_OnScrollWheel;
        InputController.Instance.OnCtrlScrollWheel -= InputController_OnCtrlScrollWheel;
        Player.Instance.Inventory.OnItemCountChanged -= Inventory_OnItemAdded;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned -= Player_OnPlayerRespawned;
    }

    private void Update()
    {
        if (Player.Instance == null || Player.Instance.IsDead)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
            SelectSlot(0);
        else if (Input.GetKeyDown(KeyCode.Alpha2))
            SelectSlot(1);
        else if (Input.GetKeyDown(KeyCode.Alpha3))
            SelectSlot(2);
        else if (Input.GetKeyDown(KeyCode.Alpha4))
            SelectSlot(3);
        else if (Input.GetKeyDown(KeyCode.Alpha5))
            SelectSlot(4);
        else if (Input.GetKeyDown(KeyCode.Alpha6))
            SelectSlot(5);
        else if (Input.GetKeyDown(KeyCode.Alpha7))
            SelectSlot(6);
        else if (Input.GetKeyDown(KeyCode.Alpha8))
            SelectSlot(7);
    }

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        SelectSlot(0);
        Player.Instance.Inventory.OnItemCountChanged += Inventory_OnItemAdded;
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
        InitSlots();
    }

    private void Player_OnPlayerRespawned(object sender, EventArgs e)
    {
        Show();
        SelectSlot(0);
    }

    private void Player_OnPlayerDied(object sender, EventArgs e)
    {
        Hide();
    }

    private void Show()
    {
        _container.SetActive(true);
    }

    private void Hide()
    {
        _container.SetActive(false);
    }

    private void Inventory_OnItemAdded(object sender, EventArgs e)
    {
        UpdateUI();
    }

    private void InputController_OnScrollWheel(object sender, InputController.InteractEventArgs e)
    {
        if (Player.Instance == null || Player.Instance.IsDead)
            return;

        var scrollDir = e.context.ReadValue<Vector2>().y;
        if (scrollDir < 0f)
        {
            _selectedSlot = _selectedSlot < _slots.Count - 1 ? _selectedSlot + 1 : _selectedSlot;
        }
        else if (scrollDir > 0f)
        {
            _selectedSlot = _selectedSlot > 0 ? _selectedSlot - 1 : _selectedSlot;
        }

        if (_lastSelectedSlot != _selectedSlot)
        {
            SelectSlot(_selectedSlot);
        }
    }

    private void InputController_OnCtrlScrollWheel(object sender, InputController.InteractEventArgs e)
    {
        if (Player.Instance == null || Player.Instance.IsDead)
            return;

        var scrollDir = e.context.ReadValue<Vector2>().y;
        if (scrollDir > 0f)
            SwitchToNextSlotBar();
        else if (scrollDir < 0f)
            SwitchToPreviousSlotBar();
    }

    public void SwitchToNextSlotBar()
    {
        Player.Instance.Inventory.SwitchToNextSlotBar(true, out int currentSlotBarIndex);
        _currentSlotBarText.text = $"{currentSlotBarIndex + 1}";
    }

    public void SwitchToPreviousSlotBar()
    {
        Player.Instance.Inventory.SwitchToNextSlotBar(false, out int currentSlotBarIndex);
        _currentSlotBarText.text = $"{currentSlotBarIndex + 1}";
    }

    public void UpdateSelectedSlotBar(int currentSlotBarIndex)
    {
        _currentSlotBarText.text = $"{currentSlotBarIndex + 1}";
    }

    private void SelectSlot(int id)
    {
        _lastSelectedSlot = id;
        _selectedSlot = id;
        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].UpdateBackgroundColor(i == id);
        }

        if ((_slots[id].ItemStack != null && !_slots[id].ItemStack.itemSO.preventThrowing) || _slots[id].ItemStack == null)
            MouseCursorController.Instance.SetCursor_Default();

        OnSlotSelected?.Invoke(this, new OnSlotSelectedEventArgs { slot = _slots[id] });
    }

    public void InitSlots()
    {
        Player.Instance.Inventory.AddSlots(_slots.Take(8).ToList());
    }

    public void UpdateUI(ItemSO addedItem)
    {
        var stacks = Player.Instance.Inventory.Stacks;

        for (int i = 0; i < _slots.Count; i++)
        {
            var stack = stacks[i];

            if (addedItem != null && stack != null && stack.itemSO != addedItem)
                continue;

            _slots[i].UpdateSlot(stack);
        }
    }

    public void UpdateUI()
    {
        var stacks = Player.Instance.Inventory.Stacks;

        for (int i = 0; i < _slots.Count; i++)
        {
            _slots[i].UpdateSlot(stacks[i]);
        }
    }

    public void OnMMEvent(LaserCannonShootEvent laserCannonEvent)
    {
        _canvasGroup.blocksRaycasts = !laserCannonEvent.isLaserActive;
        _canvasGroup.alpha = laserCannonEvent.isLaserActive ? 0f : 1f;
    }
}
