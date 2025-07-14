using System.Collections;
using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Localization;

public interface IPayload
{
    public ItemSO ItemSO { get; }
    public Sprite Sprite { get; }
    public GameObject GameObject { get; }
    public void OnPayloadCollected(IPayloadContainer payloadContainer);
    public void OnPayloadDiscarded();
}

public interface IPayloadContainer
{
    public GameObject GameObject { get; }
    public void SetPayload(IPayload payload);
    public IPayload Payload { get; }
}

public class MineCart : MonoBehaviour, ISaveable, IInteractable, ITooltip, IPayloadContainer, ISingularityAffectee
{
    [SerializeField] private float _distanceToPlayer;
    [SerializeField] private ItemSO _itemSO;
    [SerializeField] private float _uphillForce;
    [SerializeField] private float _normalForce;
    [SerializeField] private float _catchUpForce;
    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private TargetJoint2D _joint;
    [SerializeField] private LaserEmitter _laser;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private PrefabSO _prefabSO;
    [SerializeField] private AudioPlayer _startInteractionAudioPlayer;
    [SerializeField] private AudioPlayer _stopInteractionAudioPlayer;
    [SerializeField] private MMF_Player _interactionFeedback;

    [Space(10)]
    [SerializeField] private float _wheelSpeed;
    [SerializeField] private Transform _wheel1;
    [SerializeField] private Transform _wheel2;
    [SerializeField] private MMF_Player _movingFeedback;
    [SerializeField] private LaserEmitter _rotationLaserLeft;
    [SerializeField] private LaserEmitter _rotationLaserRight;
    [SerializeField] private Transform _visualContainer;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _maxRotationDegree;

    [Space(10)]
    [Header("Localization")]
    [SerializeField] private LocalizedString _indicatorPushString;
    [SerializeField] private LocalizedString _indicatorDropString;

    [Space(10)]
    [Header("Payload")]
    [SerializeField] private Transform _payloadPivot;

    #region IPayloadContainer
    public GameObject GameObject => gameObject;
    #endregion

    #region ITooltip
    public string TooltipTitle
    {
        get
        {
            var payloadTooltipText = string.Empty;

            if (_payload != null)
            {
                var payloadTooltip = _payload.GameObject.GetComponent<ITooltip>();
                if (payloadTooltip != null)
                    payloadTooltipText = $" + {payloadTooltip.TooltipTitle}";
            }
            return $"{_itemSO.ItemName}{payloadTooltipText}";
        }
    }

    public Inventory TooltipInventory => _inventory;
    #endregion

    #region IInteractable
    public int Priority => 10;
    public Vector3 IndicatorPosition
    {
        get
        {
            if (this == null)
                return default;

            return transform.position + 4f * Vector3.up;
        }
    }

    public string IndicatorTextOverride
    {
        get
        {
            var payloadIndicatorText = string.Empty;
            if (_payload != null)
            {
                payloadIndicatorText = $"\n{_indicatorDropString.GetLocalizedString()}";
            }

            return $"{_indicatorPushString.GetLocalizedString()}{payloadIndicatorText}";
        }
    }

    public Transform Transform => transform;
    public List<KeyCode> InteractionKeys => new List<KeyCode> { KeyCode.E, KeyCode.F };
    public List<KeyCode> ButtonInteractionKeys => new List<KeyCode> { KeyCode.F };
    #endregion

    #region ISingularityAffectee
    public Rigidbody2D Body => _body;
    #endregion

    public ItemSO ItemSO => _itemSO;

    private Vector3 _laserHitPoint;
    private MMF_Sound _mmfSound;
    private int _moveDir;
    private IPayload _payload;
    private bool _tooFarAway;
    private bool _preventCollectingPayload;

    private void Awake()
    {
        _mmfSound = _movingFeedback.GetFeedbackOfType<MMF_Sound>();
        _joint.enabled = false;

        _inventory.SetFilter((itemSO) => !itemSO.isLarge);
    }

    private void Start()
    {
        transform.rotation = Quaternion.identity;

        TilemapChunkSystem.Instance.OnPlayerMovedChunk += TilemapChunkSystem_OnPlayerMovedChunk;
    }

    private void OnDestroy()
    {
        if (Interactor.IsInteracting(this))
        {
            MovementStatsController.Instance.ResetMaxSpeed();
            MovementStatsController.Instance.UpdateJumpForce();
            Interactor.Instance.StopInteraction(this);
        }

        TilemapChunkSystem.Instance.OnPlayerMovedChunk -= TilemapChunkSystem_OnPlayerMovedChunk;
    }

    private void Update()
    {
        if (Player.Instance == null) return;

        PlaySound();
        WheelAnimation();
        HandleRotation();

        _moveDir = Player.Instance.LastMoveDir;

        if (Interactor.IsInteracting(this))
            HandleMovement();
    }

    private void HandleMovement()
    {
        if (Player.Instance == null) return;

        var leftPos = Player.Instance.transform.position + new Vector3(-_distanceToPlayer, 0f);
        var rightPos = Player.Instance.transform.position + new Vector3(_distanceToPlayer, 0f);

        var targetPos = _moveDir == -1 ? leftPos : _moveDir == 1 ? rightPos : Vector3.zero;

        if (_moveDir != 0)
        {
            _laser.transform.position = targetPos + new Vector3(0f, 2f);
            _laser.Raycast(Vector3.down, 5f, out float hitDistanceDown);

            // uphill
            if (_laserHitPoint.y > transform.position.y)
            {
                _joint.target = targetPos.WithY(_laser.transform.position.y);
                _joint.maxForce = _uphillForce;

                if (transform.position.x > Player.Instance.transform.position.x && _moveDir == -1)
                {
                    _joint.maxForce = _catchUpForce;
                    _joint.target = targetPos.WithY(_laser.transform.position.y);
                }
                else if (transform.position.x < Player.Instance.transform.position.x && _moveDir == 1)
                {
                    _joint.maxForce = _catchUpForce;
                    _joint.target = targetPos.WithY(_laser.transform.position.y);
                }
            }
            else // downhill
            {
                _joint.target = targetPos.WithY(_laserHitPoint.y);
                _joint.maxForce = _normalForce;
            }
        }
        else
        {
            _joint.maxForce = _normalForce;

            if (_moveDir == -1)
                _joint.target = leftPos;
            else if (_moveDir == 1)
                _joint.target = rightPos;
        }

        if (Vector3.Distance(Player.Instance.transform.position, transform.position) > 3f)
        {
            _tooFarAway = true;
            Interactor.Instance.StopInteraction(this);
        }
    }

    private void TilemapChunkSystem_OnPlayerMovedChunk(object sender, TilemapChunkSystem.OnPlayerMovedChunkEventArgs e)
    {
        if (_body == null)
            return;

        Helper.SetKinematicWhenFarAway(_body);
    }

    #region IPayloadContainer
    public void SetPayload(IPayload payload)
    {
        _payload = payload;

        if (payload == null)
        {
            _payload?.OnPayloadDiscarded();
        }
        else
        {
            _payload.GameObject.transform.SetParent(_payloadPivot);
            _payload.GameObject.transform.localPosition = Vector3.zero;
        }
    }

    public IPayload Payload => _payload;
    #endregion

    private void HandleRotation()
    {
        _rotationLaserLeft.Raycast(Vector3.down, maxDistance: 5f, out float hitDistanceLeft, out Vector2 hitPointLeft);
        _rotationLaserRight.Raycast(Vector3.down, maxDistance: 5f, out float hitDistanceRight, out Vector2 hitPointRight);

        var sign = hitPointLeft.y < hitPointRight.y ? 1f : -1f;
        var v = hitPointRight - hitPointLeft;
        var a = sign * Vector2.Angle(Vector3.right, v);
        a = Mathf.Clamp(a, -_maxRotationDegree, _maxRotationDegree);

        _visualContainer.rotation = Quaternion.Slerp(_visualContainer.rotation, Quaternion.Euler(0f, 0f, a), Time.deltaTime * _rotationSpeed);
    }

    private void WheelAnimation()
    {
        _wheel1.Rotate(new Vector3(0f, 0f, -1 * _body.linearVelocity.x * _wheelSpeed * Time.deltaTime));
        _wheel2.Rotate(new Vector3(0f, 0f, -1 * _body.linearVelocity.x * _wheelSpeed * Time.deltaTime));
    }

    private void PlaySound()
    {
        var absVel = Mathf.Abs(_body.linearVelocity.x);
        if (absVel > 1f && !_movingFeedback.IsPlaying)
            _movingFeedback.PlayFeedbacks();
        else if (absVel < 1f)
            _movingFeedback.StopFeedbacks();

        _mmfSound.MaxVolume = Mathf.Lerp(0f, 0.2f, absVel / 5f);
    }

    private void SetInteraction(bool startInteraction)
    {
        _joint.enabled = startInteraction;

        if (startInteraction)
        {
            MovementStatsController.Instance.SetMaxSpeed(8f);
            MovementStatsController.Instance.SetJumpForce(10f, allowBonus: false);
            StartLaser();
        }
        else if (!startInteraction)
        {
            MovementStatsController.Instance.ResetMaxSpeed();
            MovementStatsController.Instance.UpdateJumpForce();
            StopLaser();
        }
    }

    private void StartLaser()
    {
        _laser.transform.parent = null;
        _laser.gameObject.SetActive(true);
        _laser.OnHit += Laser_OnHit;
    }

    private void Laser_OnHit(object sender, LaserEmitter.OnHitEventArgs e)
    {
        _laserHitPoint = e.hitPoint;
    }

    private void StopLaser()
    {
        _laser.OnHit -= Laser_OnHit;
        _laser.transform.parent = transform;
        _laser.gameObject.SetActive(false);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(_laser.transform.position, 0.2f);

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(_laserHitPoint, 0.2f);
    }

    public void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (interactionType == Interactor.InteractionType.START)
        {
            if (keyCode == KeyCode.E)
            {
                _tooFarAway = false;
                SetInteraction(true);
                _startInteractionAudioPlayer.PlaySound();
                _interactionFeedback.PlayFeedbacks();
            }
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            if (keyCode == KeyCode.E || _tooFarAway)
                SetInteraction(false);
            if (keyCode == KeyCode.E)
            {
                _interactionFeedback.PlayFeedbacks();
                _stopInteractionAudioPlayer.PlaySound();
            }
        }
        else if (interactionType == Interactor.InteractionType.BUTTON)
        {
            if (keyCode == KeyCode.F)
            {
                DiscardPayload();
            }
        }
    }

    private void ShowInventoryMenu(bool show)
    {
        if (show)
            MineCartInventoryMenu.Show(_inventory);
        else
            MineCartInventoryMenu.Hide();
    }

    public void ForceCancelInteraction()
    {
        SetInteraction(false);
    }

    #region ISaveable
    public string GetCustomJson()
    {
        var inventoryData = _inventory.GetInventoryData();
        return JsonConvert.SerializeObject(inventoryData);
    }

    public void Load(string json)
    {
        var inventoryData = JsonConvert.DeserializeObject<InventoryData>(json);
        _inventory.LoadInventoryData(inventoryData);
    }
    #endregion
    #region IInteractable
    public bool AllowIndicator() => true;
    #endregion

    private void OnCollisionEnter2D(Collision2D other)
    {
        CollectPayload(other.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CollectPayload(other.gameObject);
    }

    private void CollectPayload(GameObject other)
    {
        if (_payload != null || _preventCollectingPayload)
            return;

        var payload = other.gameObject.GetComponent<IPayload>();
        if (payload != null)
        {
            SetPayload(payload);
            payload.OnPayloadCollected(this);
        }
    }

    private void DiscardPayload()
    {
        if (_payload == null)
            return;

        _preventCollectingPayload = true;
        _payload.OnPayloadDiscarded();

        _payload.GameObject.transform.SetParent(null);

        var forceDir = (Helper.MousePos - transform.position).WithZ(0f).normalized;
        _payload.GameObject.GetComponent<Rigidbody2D>().AddForce(forceDir * 100f, ForceMode2D.Impulse);

        _payload = null;

        StartCoroutine(AllowCollectingPayloadCo());
    }

    private IEnumerator AllowCollectingPayloadCo()
    {
        yield return new WaitForSeconds(0.2f);
        _preventCollectingPayload = false;
    }

    #region ISingularityAffectee
    public void OnEnterSingularityCore()
    {
        Destroy(gameObject);
    }
    #endregion
}
