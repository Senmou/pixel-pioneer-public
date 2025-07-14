using MoreMountains.Feedbacks;
using static MenuManager;
using UnityEngine;
using System;

public class DragController : MonoBehaviour
{
    public event EventHandler<OnDragEventArgs> OnDragEnded;
    public event EventHandler<OnDragEventArgs> OnDragStarted;
    public event EventHandler<OnTryDepositPickUpItemsEventArgs> OnTryDepositPickUpItems;
    public event EventHandler<OnTryDepositPickUpItemsEventArgs> OnPickUpItemsChanged;
    public event EventHandler<OnTryDepositPickUpItemsEventArgs> OnPickUpItemsEnded;
    public class OnDragEventArgs : EventArgs
    {
        public InventorySlot sourceSlot;
    }

    public class OnTryDepositPickUpItemsEventArgs : EventArgs
    {
        public InventorySlot sourceSlot;
        public ItemSO itemSO;
        public int amount;
    }

    public static DragController Instance { get; private set; }

    [SerializeField] private RectTransform _previewPrefab;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private ContactFilter2D _terrainContactFilter;

    [Header("Feedbacks")]
    [SerializeField] private MMF_Player _startDragFeedback;
    [SerializeField] private MMF_Player _endDragFeedback;
    [SerializeField] private MMF_Player _illegalDropTargetFeedback;

    public bool IsDragging => _isDragging;
    public bool PreventDoubleClick => Time.time - _lastDepositTime < 0.75f;
    public InventorySlot DragSource => _dragSource;
    public ItemSO PickUpItemSO => _pickUpItemSO;
    public int PickUpCount => _pickUpCount;

    private RectTransform _preview;
    private bool _isDragging;
    private Vector3 _lastMousePosition;
    private InventorySlot _dragSource;
    private Vector2 _delta;
    private int _pickUpCount;
    private ItemSO _pickUpItemSO;
    private float _lastDepositTime;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        MenuManager.Instance.OnMenuToggled += MenuManager_OnMenuToggled;
    }

    private void MenuManager_OnMenuToggled(object sender, OnMenuToggledEventArgs e)
    {
        if (_isDragging)
            CancelDrag();
    }

    private void Update()
    {
        if (_isDragging && InputController.Instance.IsReleased_LeftMouseButton)
        {
            EndDrag();
        }

        if (_pickUpCount > 0 && InputController.Instance.WasPressed_LeftMouseButton)
        {
            TryDepositPickUpItems();
        }

        if (!_isDragging && _pickUpCount == 0)
            return;

        if (_preview == null)
        {
            print("Null");
        }

        _delta = Input.mousePosition - _lastMousePosition;
        _preview.anchoredPosition += _delta / _canvas.scaleFactor;
        _lastMousePosition = Input.mousePosition;
    }

    public void StartDrag(InventorySlot sourceSlot)
    {
        if (_pickUpCount > 0)
            return;

        if (sourceSlot.ItemStack == null)
            return;

        _lastMousePosition = Input.mousePosition;
        _isDragging = true;
        _dragSource = sourceSlot;

        if (_preview == null)
            _preview = Instantiate(_previewPrefab, _canvas.transform);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.GetComponent<RectTransform>(), Input.mousePosition, null, out var canvasMousePos);

        _preview.anchoredPosition = canvasMousePos;

        var previewObject = _preview.GetComponent<InventorySlotDragPreview>();
        previewObject.Init(sourceSlot, sourceSlot.ItemStack.amount);

        OnDragStarted?.Invoke(this, new OnDragEventArgs { sourceSlot = sourceSlot });

        var squash = _startDragFeedback.GetFeedbackOfType<MMF_SquashAndStretch>();
        squash.SquashAndStretchTarget = sourceSlot.transform;
        _startDragFeedback.PlayFeedbacks();
    }

    public bool TryPickUpItem(InventorySlot sourceSlot)
    {
        if (_isDragging)
            return false;

        bool hasPickedUpItem = false;
        if (sourceSlot.ItemStack == null)
            return hasPickedUpItem;

        if (_pickUpItemSO != null && _pickUpItemSO != sourceSlot.ItemStack.itemSO)
        {
            return hasPickedUpItem;
        }

        _dragSource = sourceSlot;
        _pickUpItemSO = sourceSlot.ItemStack.itemSO;
        _pickUpCount++;
        _lastMousePosition = Input.mousePosition;

        if (_preview == null)
            _preview = Instantiate(_previewPrefab, _canvas.transform);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.GetComponent<RectTransform>(), Input.mousePosition, null, out var canvasMousePos);

        _preview.anchoredPosition = canvasMousePos;

        var previewObject = _preview.GetComponent<InventorySlotDragPreview>();
        previewObject.Init(sourceSlot, _pickUpCount);

        OnPickUpItemsChanged?.Invoke(this, new OnTryDepositPickUpItemsEventArgs { itemSO = _pickUpItemSO, amount = _pickUpCount });

        var squash = _startDragFeedback.GetFeedbackOfType<MMF_SquashAndStretch>();
        squash.SquashAndStretchTarget = sourceSlot.transform;
        _startDragFeedback.PlayFeedbacks();

        hasPickedUpItem = true;
        return hasPickedUpItem;
    }

    public void SwapCurrentPickUp(InventorySlot sourceSlot, ItemSO itemSO, int amount)
    {
        _pickUpItemSO = itemSO;
        _pickUpCount = amount;

        var previewObject = _preview.GetComponent<InventorySlotDragPreview>();
        previewObject.Init(sourceSlot, _pickUpCount);

        OnPickUpItemsChanged?.Invoke(this, new OnTryDepositPickUpItemsEventArgs { itemSO = _pickUpItemSO, amount = _pickUpCount });
    }

    public void CancelDrag()
    {
        _isDragging = false;
        Destroy(_preview.gameObject);
        _dragSource = null;
    }

    private void EndDrag()
    {
        var mouseOverUI = Helper.IsPointerOverUIElement();
        if (!mouseOverUI)
        {
            if (!_dragSource.PreventItemDragIntoWorld && _dragSource.ItemStack != null)
            {
                var itemsSpawned = SpawnItemsInWorld(_dragSource.ItemStack.itemSO, _dragSource.ItemStack.amount);
                if (itemsSpawned)
                    _dragSource.Inventory.RemoveAllItemsFromStack(_dragSource.ItemStack);
            }
        }

        _isDragging = false;
        Destroy(_preview.gameObject);

        OnDragEnded?.Invoke(this, new OnDragEventArgs { sourceSlot = _dragSource });
        _dragSource = null;
    }

    private void TryDepositPickUpItems()
    {
        var mouseOverUI = Helper.IsPointerOverUIElement();
        if (!mouseOverUI)
        {
            var itemsSpawned = SpawnItemsInWorld(_pickUpItemSO, _pickUpCount);

            if (itemsSpawned)
                RemovePickedUpItems(_pickUpCount);
        }
        else
            OnTryDepositPickUpItems?.Invoke(this, new OnTryDepositPickUpItemsEventArgs { sourceSlot = _dragSource, itemSO = _pickUpItemSO, amount = _pickUpCount });

        _lastDepositTime = Time.time;
    }

    private bool SpawnItemsInWorld(ItemSO itemSO, int itemCount)
    {
        var maxDistance = 4f;
        var playerPos = Player.Instance.transform.position;
        var dir = Helper.MousePos - playerPos;
        var distance = Mathf.Min(dir.magnitude, maxDistance);
        var origin = playerPos + new Vector3(0f, 0.5f);
        var hit = Physics2D.Raycast(origin, dir, distance, _terrainContactFilter.layerMask);

        if (hit)
            return false;

        var droppedItem = WorldItemController.Instance.DropItem(Helper.MousePos, itemSO);
        droppedItem.GetComponent<WorldItem>().SetStackSize(itemCount);
        droppedItem.transform.position = origin + dir.normalized * distance;

        return true;
    }

    public void RemovePickedUpItems(int amount)
    {
        _pickUpCount -= amount;

        if (_pickUpCount > 0)
            _preview.GetComponent<InventorySlotDragPreview>().Init(_dragSource, _pickUpCount);
        else
        {
            _pickUpCount = 0;
            _pickUpItemSO = null;
            OnPickUpItemsEnded?.Invoke(this, new OnTryDepositPickUpItemsEventArgs { itemSO = null, amount = 0 });
            Destroy(_preview.gameObject);
        }
    }

    public void PlayDropFeedback(Transform slotTransform)
    {
        if (slotTransform == null)
            return;

        var squash = _endDragFeedback.GetFeedbackOfType<MMF_SquashAndStretch>();
        squash.SquashAndStretchTarget = slotTransform;
        _endDragFeedback.PlayFeedbacks();
    }

    public void PlayIllegalDropTargetFeedback(Transform slotTransform)
    {
        var rotate = _illegalDropTargetFeedback.GetFeedbackOfType<MMF_Rotation>();
        rotate.AnimateRotationTarget = slotTransform;
        _illegalDropTargetFeedback.PlayFeedbacks();
    }
}
