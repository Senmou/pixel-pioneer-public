using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;

public class InventorySlot : MonoBehaviour, ITooltip, IPointerEnterHandler, IPointerExitHandler
{
    public event EventHandler<ItemSO> OnSlotChanged;

    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _amountText;
    [SerializeField] private Color _defaultColor;
    [SerializeField] private Color _lockedColor;
    [SerializeField] private Color _selectedColor;
    [SerializeField] private Color _legalTargetColor;
    [SerializeField] private Color _illegalTargetColor;
    [SerializeField] private Image _background;
    [SerializeField] private bool _isFuelSlot;
    [SerializeField] private SimpleSlider _slider;
    [SerializeField] private Image _fuelSlotIndicator;
    [SerializeField] private InventorySlotDragHandler _dragHandler;
    [SerializeField] private InventorySlotDropHandler _dropHandler;
    [SerializeField] private GameObject _lockedIndicator;

    [Space(10)]
    [Header("Optional")]
    [SerializeField] private List<ItemSO> _allowedItems;
    [SerializeField] private bool _preventItemDragIntoWorld;
    [SerializeField] private bool _showBackground;
    [SerializeField] private UnityEvent<ItemSO> _onItemChanged;
    [SerializeField] private LocalizedString _fuelString;

    #region ITooltip
    public string TooltipTitle
    {
        get
        {
            if (_itemStack == null && _isFuelSlot)
                return $"{_fuelString.GetLocalizedString()}";
            return _itemStack != null ? _itemStack.itemSO.ItemName : "";
        }
    }
    public string TooltipDescription
    {
        get
        {
            if (_itemStack != null && _itemStack.itemSO is FuelItemSO)
                return $"{(_itemStack.itemSO as FuelItemSO).kwh} kWh";
            else if (_itemStack != null && _itemStack.itemSO._showDescriptionInInventory)
                return _itemStack.itemSO.Description;
            else
                return string.Empty;
        }
    }
    #endregion

    public Inventory.ItemStack ItemStack => _itemStack;
    public Image Icon => _icon;
    public Inventory Inventory => _inventory;
    public int Index => _index;
    public bool IsLegalDropTarget => _isLegalDropTarget && !IsLocked;
    public bool PreventItemDragIntoWorld => _preventItemDragIntoWorld;
    public bool IsLocked => _isLocked;

    private bool _isLocked;
    private bool _pointerOverSlot;
    private Color _lastColor;
    private Inventory.ItemStack _itemStack;
    private Inventory _inventory;
    private int _index;
    protected bool _isLegalDropTarget;

    private void OnValidate()
    {
        _slider.gameObject.SetActive(_isFuelSlot);
        _fuelSlotIndicator.gameObject.SetActive(_isFuelSlot || _showBackground);
    }

    private void OnEnable()
    {
        _slider.gameObject.SetActive(_isFuelSlot);
        _fuelSlotIndicator.gameObject.SetActive(_isFuelSlot || _showBackground);
        _slider.SetValue(0f);

        var dragController = DragController.Instance;
        if (dragController == null)
            dragController = FindAnyObjectByType<DragController>();

        dragController.OnDragEnded += DragController_OnDragEnded;
        dragController.OnDragStarted += DragController_OnDragStarted;
        dragController.OnPickUpItemsEnded += DragController_OnPickUpItemsEnded;
        dragController.OnPickUpItemsChanged += DragController_OnPickUpItemsStarted;
        InputController.Instance.OnDoubleClicked += InputController_OnDoubleClicked;
        InputController.Instance.OnShiftLeftClicked += InputController_OnShiftLeftClicked;
    }

    private void OnDisable()
    {
        Terminate();
    }

    private void Awake()
    {
        _lastColor = _defaultColor;
    }

    public void UpdateFuelSlider(float ratio, bool show)
    {
        //_slider.SetValue(ratio);
        //_slider.gameObject.SetActive(show);
    }

    private void DragController_OnPickUpItemsEnded(object sender, DragController.OnTryDepositPickUpItemsEventArgs e)
    {
        ResetBackgroundColor();
    }

    private void DragController_OnDragEnded(object sender, DragController.OnDragEventArgs e)
    {
        ResetBackgroundColor();
    }

    public void Terminate()
    {
        var dragController = FindAnyObjectByType<DragController>();
        if (dragController != null)
        {
            dragController.OnDragEnded -= DragController_OnDragEnded;
            dragController.OnDragStarted -= DragController_OnDragStarted;
            dragController.OnPickUpItemsEnded -= DragController_OnPickUpItemsEnded;
            dragController.OnPickUpItemsChanged -= DragController_OnPickUpItemsStarted;

            if (dragController.IsDragging && dragController.DragSource == this)
            {
                dragController.CancelDrag();
            }
        }

        if (_inventory != null)
            _inventory.OnItemCountChanged -= Inventory_OnItemCountChanged;

        InputController.Instance.OnDoubleClicked -= InputController_OnDoubleClicked;
        InputController.Instance.OnShiftLeftClicked -= InputController_OnShiftLeftClicked;
    }

    private void DragController_OnDragStarted(object sender, DragController.OnDragEventArgs e)
    {
        CheckLegality(e.sourceSlot);
    }

    private void DragController_OnPickUpItemsStarted(object sender, DragController.OnTryDepositPickUpItemsEventArgs e)
    {
        CheckLegality(e.itemSO);
    }

    protected void CheckLegality(InventorySlot sourceSlot)
    {
        bool swapIsLegal = true;
        if (ItemStack != null)
            swapIsLegal = (ItemStack.itemSO == sourceSlot.ItemStack.itemSO || sourceSlot.Inventory.MaxStackSize >= ItemStack.amount) && Inventory.MaxStackSize >= sourceSlot.ItemStack.amount && sourceSlot.Inventory.CheckFilter(ItemStack.itemSO) && (sourceSlot._allowedItems == null || sourceSlot._allowedItems.Count == 0 || sourceSlot._allowedItems.Contains(ItemStack.itemSO));

        _isLegalDropTarget = swapIsLegal && Inventory.CheckFilter(sourceSlot.ItemStack.itemSO) && (_allowedItems == null || _allowedItems.Count == 0 || _allowedItems.Contains(sourceSlot.ItemStack.itemSO));
        UpdateLegalBackgroundColor(_isLegalDropTarget);
    }

    protected void CheckLegality(ItemSO sourceItemSO)
    {
        _isLegalDropTarget = Inventory.CheckFilter(sourceItemSO) && (_allowedItems == null || _allowedItems.Count == 0 || _allowedItems.Contains(sourceItemSO));
        UpdateLegalBackgroundColor(_isLegalDropTarget);
    }

    public void Init(Inventory.ItemStack itemStack, Inventory inventory)
    {
        _inventory = inventory;
        UpdateSlot(itemStack);

        if (DragController.Instance != null && DragController.Instance.IsDragging)
        {
            CheckLegality(DragController.Instance.DragSource);
        }

        inventory.OnItemCountChanged -= Inventory_OnItemCountChanged;
        inventory.OnItemCountChanged += Inventory_OnItemCountChanged;
    }

    private void Inventory_OnItemCountChanged(object sender, System.EventArgs e)
    {
        UpdateSlot(_itemStack);
    }

    public GameObject RemoveAndDropItem(bool addForceTowardsPlayer = true)
    {
        if (ItemStack == null || ItemStack.amount == 0) return null;

        var droppedGameObject = WorldItemController.Instance.DropItem(transform.position, ItemStack.itemSO, WorldItemController.ItemSpawnSource.INVENTORY);
        if (addForceTowardsPlayer && droppedGameObject != null)
        {
            var body = droppedGameObject.GetComponent<Rigidbody2D>();
            if (body != null)
            {
                var dir = (Player.Instance.transform.position - transform.position).normalized;
                body.bodyType = RigidbodyType2D.Dynamic;
                body.linearVelocity = Vector3.zero;
                body.AddForce(dir * 10, ForceMode2D.Impulse);
            }
        }

        Inventory.DropItem(this);
        return droppedGameObject;
    }

    public void UpdateSlot(Inventory.ItemStack itemStack)
    {
        if (itemStack == null || itemStack.amount == 0)
        {
            Inventory.DeleteStack(itemStack);

            _itemStack = null;
            Hide();

            if (this == PlayerInventoryUI.Instance.GetSelectedSlot())
            {
                Player.Instance.OnSelectedSlotUpdated(this);
            }

            _onItemChanged?.Invoke(null);
            OnSlotChanged?.Invoke(this, null);
            return;
        }

        Show();
        _itemStack = itemStack;
        _icon.sprite = itemStack.itemSO.sprite;

        if (_amountText != null)
            _amountText.text = $"{itemStack.amount}x";

        if (this == PlayerInventoryUI.Instance.GetSelectedSlot())
        {
            Player.Instance.OnSelectedSlotUpdated(this);
        }

        _onItemChanged?.Invoke(_itemStack.itemSO);
        OnSlotChanged?.Invoke(this, _itemStack.itemSO);
    }

    public void SetLocked(bool isLocked)
    {
        _isLocked = isLocked;
        //_dropHandler.enabled = !isLocked;
        SetBackgroundColor(isLocked ? _lockedColor : _defaultColor);
        _lockedIndicator.SetActive(isLocked);
    }

    public void UpdateLegalBackgroundColor(bool isLegalDropTarget)
    {
        if (_isLocked)
            return;
        SetBackgroundColor(isLegalDropTarget ? _legalTargetColor : _illegalTargetColor);
    }

    public void UpdateBackgroundColor(bool isSelected)
    {
        SetBackgroundColor(isSelected ? _selectedColor : _defaultColor);
    }

    private void ResetBackgroundColor()
    {
        if (_isLocked)
            return;
        SetBackgroundColor(_lastColor);
    }

    private void SetBackgroundColor(Color color)
    {
        _background.color = color;
    }

    public void Show()
    {
        _icon.gameObject.SetActive(true);

        if (_amountText != null)
            _amountText.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _icon.gameObject.SetActive(false);
        if (_amountText != null)
            _amountText.gameObject.SetActive(false);
        _fuelSlotIndicator.gameObject.SetActive(_isFuelSlot || _showBackground);
    }

    private void InputController_OnDoubleClicked(object sender, InputController.InteractEventArgs e)
    {
        if (_pointerOverSlot && !(DragController.Instance.IsDragging || DragController.Instance.PreventDoubleClick))
        {
            TransferItemStack();
        }
    }

    private void InputController_OnShiftLeftClicked(object sender, InputController.InteractEventArgs e)
    {
        if (!_pointerOverSlot)
            return;

        if (InputController.Instance.IsPressed_ShiftLeftClick)
        {
            TransferItemStack();
        }
    }

    private void TransferItemStack()
    {
        var targetInventory = Inventory.GetQuickTransferTargetInventory(_inventory);
        if (targetInventory != null)
        {
            Inventory.TryQuickTransferItemStack(sourceSlot: this, targetInventory);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _pointerOverSlot = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _pointerOverSlot = false;
    }
}
