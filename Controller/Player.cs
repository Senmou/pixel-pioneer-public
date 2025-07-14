using System.Collections.Generic;
using static MyPlayerController;
using QFSW.QC.Suggestors.Tags;
using MoreMountains.Tools;
using System.Collections;
using UnityEngine.Events;
using TarodevController;
using UnityEngine.UI;
using UnityEngine;
using QFSW.QC;
using System;

public class Player : MonoBehaviour, ISingularityAffectee
{
    public static Player Instance { get; private set; }

    public event EventHandler OnPlayerDied;
    public event EventHandler OnPlayerRespawned;
    public event EventHandler OnHealthChanged;
    public EventHandler<bool> OnEquippedLaserCannon;

    [SerializeField] private float _force;
    [SerializeField] private Transform _itemAnchor;
    [SerializeField] private MyPlayerController _playerController;
    [SerializeField] private UnityEvent OnEnterBuilding;
    [SerializeField] private UnityEvent OnExitBuilding;
    [SerializeField] private UnityEvent OnEnterMineCart;
    [SerializeField] private UnityEvent OnExitMineCart;
    [SerializeField] private UI_InteractionIndicator _interactionIndicatorController;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private Inventory _equipmentInventory;
    [SerializeField] private Inventory _laserUpgradeInventory;
    [SerializeField] private AudioPlayer _itemAddedAudioPlayer;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private ActionItemSO _ladderActionItemSO;
    [SerializeField] private ActionItemSO _stoneSlabActionItemSO;
    [SerializeField] private LayerMask _groundLayerMask;
    [SerializeField] private SpriteExplosion _spriteExplosion;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private PlayerAnimator _playerAnimator;
    [SerializeField] private Canvas _staminaContainer;
    [SerializeField] private Image _staminaImage;

    public int LastMoveDir => _lastMoveDir;
    public bool IsDead { get; private set; }
    public bool IsInBuilding => _isInVehicle;
    public Inventory Inventory => _inventory;
    public Inventory EquipmentInventory => _equipmentInventory;
    public Inventory LaserUpgradeInventory => _laserUpgradeInventory;
    public MyPlayerController PlayerController => _playerController;
    public Rigidbody2D Body => _body;
    public Collider2D Collider => _collider;
    public Vector3 InitialSpawnPosition => _initialSpawnPos;
    public const int MIN_UNLOCKED_INVENTORY_STACK_COUNT = 8;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => 5;
    public float DepthLimit => _baseDepthLimit - _bonusDepthLimit;
    public float PositionY => transform.position.y - TilemapChunkSystem.Instance.World.deepestSurfaceHeight;
    public string PlayerName => $"Pioneer #{GlobalStats.Instance.DeathCounter + 1}";
    public bool IsLookingLeft => _playerAnimator.IsSpriteLookingLeft;
    public bool IsOnWall => _playerController.IsOnWall;
    public float Stamina { get; private set; }

    private int _currentHealth;
    private int _lastMoveDir;
    private int _suffocationCounter;
    private bool _isInVehicle;
    private bool _preventHealthRegen;
    private float _regenHealthTimer;
    private float _preventRegenHealthTimer;
    private const float REGEN_HEALTH_DELAY = 30f;
    private const float REGEN_HEALTH_INTERVAL = 5f;
    private float _baseDepthLimit;
    private float _bonusDepthLimit;
    private float _lastPosY;

    private Vector2Int _lastPosition;
    private Vector3 _initialSpawnPos;
    private Vector3 _stepRaycastOrigin;
    private RaycastHit2D[] _hitStepResults = new RaycastHit2D[1];

    private Rigidbody2D _body;
    private ActionItem _lastSelectedActionItem;
    private List<Func<RespawnData>> _onPlayerDiedCallbacks = new List<Func<RespawnData>>();
    private InventorySlot _lastSelectedInventorySlot;

    #region ISingularityAffectee
    public bool IsAttractable => false;
    #endregion

    public struct RespawnData
    {
        public GameObject sender;
        public bool preventRespawn;
        public bool keepInventory;
        public bool overwriteSpawnPosition;
        public Vector3 spawnPosition;
    }

    private void Awake()
    {
        Instance = this;
        _body = GetComponent<Rigidbody2D>();
        _inventory.SetFilter(InventoryFilter);
        _inventory.SetUnlockedStacksCount(8);

        _currentHealth = MaxHealth;
        _baseDepthLimit = -75f;
        AddStamina(1f);
    }

    private void Start()
    {
        Inventory.OnItemCollected += Inventory_OnItemAdded;
        OxygenController.Instance.OnPlayerSuffocated += OxygenController_OnPlayerSuffocated;
        PlayerInventoryUI.Instance.OnSlotSelected += PlayerInventoryUI_OnSlotSelected;
        ItemShooter.Instance.OnItemShot += ItemShooter_OnItemShot;

        _initialSpawnPos = TilemapChunkSystem.Instance.World.PlayerSpawnPosition;

        _playerController.OnHitGroundAfterFalling += PlayerController_OnHitGroundAfterFalling;
    }

    private void PlayerController_OnHitGroundAfterFalling(object sender, OnHitGroundAfterFallingEventArgs e)
    {
        if (e.fallHeight >= 15f)
        {
            TakeDamage((int)e.fallHeight / 10);
        }
    }

    private void Update()
    {
        DEBUG_HandleInput();

        HandleMovementDirection();
        //HandleHealthRegeneration();
        HandlePositionEvents();
    }

    private void HandlePositionEvents()
    {
        if (Mathf.Abs(_lastPosY - PositionY) > 1f)
        {
            var isBelowDepthLimit = PositionY < DepthLimit;
            MMEventManager.TriggerEvent(
                new PlayerEvent
                {
                    positionData = new PlayerEvent.PositionData
                    {
                        isBelowDepthLimit = isBelowDepthLimit
                    }
                });
            _lastPosY = PositionY;
        }
    }

    public void SetBonusDepthLimit(float bonusDepthLimit)
    {
        _bonusDepthLimit = bonusDepthLimit;

        var isBelowDepthLimit = PositionY < DepthLimit;
        MMEventManager.TriggerEvent(
            new PlayerEvent
            {
                positionData = new PlayerEvent.PositionData
                {
                    isBelowDepthLimit = isBelowDepthLimit
                }
            });
    }

    private void DEBUG_HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            ConsoleCommands.ToggleSuperman();
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            OxygenController.Instance.DEBUG_PreventOxygenUsage(true);
        }

        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            TakeDamage(1);
        }

        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            Heal(1);
        }

        //if (Input.GetKeyDown(KeyCode.F))
        //{
        //    Die();
        //}
    }

    private void HandleMovementDirection()
    {
        if (_body.linearVelocity.x < -1f)
        {
            _lastMoveDir = -1;
        }
        else if (_body.linearVelocity.x > 1f)
        {
            _lastMoveDir = 1;
        }
    }

    private void HandleHealthRegeneration()
    {
        if (IsDead)
            return;

        if (_preventHealthRegen)
        {
            _regenHealthTimer = 0f;
            _preventRegenHealthTimer += Time.deltaTime;

            if (_preventRegenHealthTimer >= REGEN_HEALTH_DELAY)
            {
                _preventRegenHealthTimer = 0f;
                _preventHealthRegen = false;
            }
        }
        else
        {
            _regenHealthTimer += Time.deltaTime;
            if (_regenHealthTimer >= REGEN_HEALTH_INTERVAL)
            {
                _regenHealthTimer = 0f;
                Heal(1);
            }
        }
    }

    private void LateUpdate()
    {
        _body.linearVelocity = Vector2.ClampMagnitude(_body.linearVelocity, MovementStatsController.Instance.MaxSpeed);
    }

    private bool InventoryFilter(ItemSO itemSO) => !itemSO.isLarge;

    private void ItemShooter_OnItemShot(object sender, ItemShooter.OnItemShotEventArgs e)
    {
        OnSelectedSlotUpdated(e.slot);
    }

    private void PlayerInventoryUI_OnSlotSelected(object sender, PlayerInventoryUI.OnSlotSelectedEventArgs e)
    {
        OnSelectedSlotUpdated(e.slot);
    }

    public void OnSelectedSlotUpdated(InventorySlot slot)
    {
        _lastSelectedInventorySlot = slot;

        if (_lastSelectedActionItem != null)
        {
            if (slot.ItemStack == null)
            {
                _lastSelectedActionItem.OnItemDeselected();
                return;
            }

            var isSameItem = _lastSelectedActionItem.ItemSO == slot.ItemStack.itemSO;
            if (isSameItem)
                return;
        }

        if (_lastSelectedActionItem != null)
            _lastSelectedActionItem.OnItemDeselected();

        if (slot.ItemStack == null) return;

        if (slot.ItemStack.itemSO is ActionItemSO)
        {
            _lastSelectedActionItem = Instantiate(slot.ItemStack.itemSO.Prefab).GetComponent<ActionItem>();
            _lastSelectedActionItem.OnItemSelected();
        }
    }

    public void DeselectLastActionItem()
    {
        if (_lastSelectedActionItem == null)
            return;

        _lastSelectedActionItem.OnItemDeselected();
    }

    public void ReselectLastActionItem()
    {
        if (_lastSelectedInventorySlot.ItemStack?.itemSO is ActionItemSO)
        {
            _lastSelectedActionItem = Instantiate(_lastSelectedInventorySlot.ItemStack.itemSO.Prefab).GetComponent<ActionItem>();
            _lastSelectedActionItem.OnItemSelected();
        }
    }

    private void OxygenController_OnPlayerSuffocated(object sender, EventArgs e)
    {
        _suffocationCounter++;
        if (_suffocationCounter == 5)
        {
            _suffocationCounter = 0;
            TakeDamage(1);
        }
    }

    public void AddPlayerDyingCallback(Func<RespawnData> callback)
    {
        _onPlayerDiedCallbacks.Add(callback);
    }

    public void RemovePlayerDyingCallback(Func<RespawnData> callback)
    {
        _onPlayerDiedCallbacks.Remove(callback);
    }

    public void SetCurrentHealth(int value) => _currentHealth = value;

    public void Heal(int amount)
    {
        if (_currentHealth == MaxHealth)
            return;

        _currentHealth += amount;
        _currentHealth = Math.Clamp(_currentHealth, 0, MaxHealth);

        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    public void TakeDamage(int amount)
    {
        if (_currentHealth <= 0)
            return;

        _preventHealthRegen = true;

        _currentHealth -= amount;
        _currentHealth = Math.Clamp(_currentHealth, 0, MaxHealth);

        if (_currentHealth == 0)
            Die();

        OnHealthChanged?.Invoke(this, EventArgs.Empty);
    }

    private void Die()
    {
        Instantiate(PrefabManager.Instance.Prefabs.GetItemSOById("Item_Tombstone").Prefab, transform.position, Quaternion.identity);

        GlobalStats.Instance.OnPlayerDied();

        _spriteExplosion.ExplodeSprite(transform.position, _spriteRenderer.sprite);
        _spriteRenderer.enabled = false;

        IsDead = true;
        Interactor.Instance.StopAllInteractions();
        OnPlayerDied?.Invoke(this, EventArgs.Empty);
        _playerController.FreezePlayer();

        Vector3 spawnPosition = _initialSpawnPos;
        //bool keepInventory = false;
        //bool shouldPlayerRespawn = true;
        //foreach (var callback in _onPlayerDiedCallbacks)
        //{
        //    var respawnData = callback();
        //    if (respawnData.preventRespawn)
        //    {
        //        shouldPlayerRespawn = false;
        //        break;
        //    }

        //    if (respawnData.keepInventory)
        //        keepInventory = true;

        //    if (respawnData.overwriteSpawnPosition)
        //        spawnPosition = respawnData.spawnPosition;
        //}

        var activeActionItems = FindObjectsByType<ActionItem>(FindObjectsSortMode.None);
        foreach (var actionItem in activeActionItems)
        {
            actionItem.OnItemDeselected();
        }

        StartCoroutine(ShowRespawnMenuCo());
    }

    private IEnumerator ShowRespawnMenuCo()
    {
        yield return new WaitForSeconds(1f);
        RespawnMenu.Show();
    }

    public void Respawn()
    {
        var portal = FindAnyObjectByType<Portal>();
        var spawnPos = portal != null ? portal.transform.position + new Vector3(1.5f, 0f) : _initialSpawnPos;

        RaycastHit2D[] results = new RaycastHit2D[1];
        if (Physics2D.Raycast(spawnPos, Vector2.down, Helper.ContactFilter_Ground, results) > 0)
        {
            spawnPos = results[0].point;
        }

        StartCoroutine(RespawnCo(keepInventory: true, spawnPos));
    }

    private IEnumerator RespawnCo(bool keepInventory, Vector3 spawnPosition)
    {
        GameManager.Instance.RespawnTransitionScreen.Show(0.2f);
        yield return new WaitForSeconds(0.2f);

        Respawn(keepInventory, spawnPosition);

        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.RespawnTransitionScreen.Hide(0.2f);
    }

    public void Respawn(bool keepInventory, Vector3 spawnPosition)
    {
        _spriteRenderer.enabled = true;
        _playerController.UnfreezePlayer();

        if (!keepInventory)
        {
            Inventory.ClearInventory(keepPreventOnDropItems: true);
        }

        _currentHealth = MaxHealth;
        OnHealthChanged?.Invoke(this, EventArgs.Empty);

        IsDead = false;
        transform.position = spawnPosition;
        OxygenController.Instance.RestoreFullOxygen();
        PlayerCamera.Instance.SnapCamera();

        OnPlayerRespawned?.Invoke(this, EventArgs.Empty);
    }

    public void FlipSpriteToMouse(bool state)
    {
        _playerAnimator.FlipSpriteToMouse(state);
    }

    public void AddStamina(float value)
    {
        Stamina = Mathf.Clamp01(Stamina + value);

        _staminaImage.fillAmount = Stamina;
        _staminaContainer.gameObject.SetActive(Stamina < 1f);
    }

    public bool ShouldStickToWall => _playerController.ShouldStickToWall;

    [Command(aliasOverride: "respawn")]
    private void DEBUG_Respawn()
    {
        Respawn(true, _initialSpawnPos);
    }

    [Command(aliasOverride: "teleport")]
    private void Teleport([Suggestions("(0,0)")] Vector2 position)
    {
        transform.position = position;
    }

    private void Inventory_OnItemAdded(object sender, Inventory.OnItemCollectedEventArgs e)
    {
        _itemAddedAudioPlayer.PlaySound(allowWhilePlaying: true);

        FloatingTextController.Instance.SpawnText($"+{e.inventoryItem.StackSize}x {e.inventoryItem.ItemSO.ItemName}", transform.position + 3f * Vector3.up, ignoreTimeRestriction: true);
    }

    public void EnterVehicle()
    {
        _isInVehicle = true;
        _playerController.FreezePlayer();
        OnEnterBuilding?.Invoke();
    }

    public void ExitVehicle()
    {
        if (!_isInVehicle)
            return;

        _isInVehicle = false;

        _playerController.UnfreezePlayer();
        OnExitBuilding?.Invoke();
    }

    public void SetNormalMovementStats()
    {
        _playerController.SetNormalMovementStats();
    }

    public void SetMovementStats(ScriptableStats movementStats)
    {
        _playerController.SetMovementStats(movementStats);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var asteroid = other.GetComponentInParent<Asteroid>();

        if (asteroid != null)
        {
            Die();
        }
    }

    [Command("fly")]
    public void ToggleFlying(bool flying)
    {
        _playerController.SetAirControl(flying);
    }

    public void OnEnterSingularityCore()
    {
        Respawn(keepInventory: true, _initialSpawnPos);
    }

    public void TryStickToWall(bool stickToWall)
    {
        _playerController.StickToWall(stickToWall);
    }
}
