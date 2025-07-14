using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;
using System.Linq;
using System;

public class Railgun : BaseBuilding/*, IPowerGridEntity*/
{
    //[SerializeField] private GameObject _progressVisual;
    //[SerializeField] private GameObject _baseVisual;
    //[SerializeField] private GameObject _cannonVisual;
    //[SerializeField] private ItemSO _ammoSO;
    //[SerializeField] private LayerMask _asteroidLayerMask;
    //[SerializeField] private Animator _reloadAmmoAnimator;
    //[SerializeField] private Animator _reloadWarmUpAnimator;
    //[SerializeField] private MMF_Player _reloadFeedback;
    //[SerializeField] private MMF_Player _shootFeedback;
    //[SerializeField] private MMF_Player _targetIndicatorFeedback;

    //[Space(10)]
    //[Header("Stats")]
    //[SerializeField] private float _maxRotationAngle;
    //[SerializeField] private float _rotationSpeed;
    //[SerializeField] private float _powerConsumptionMoving;
    //[SerializeField] private float _maxDistance;
    //[SerializeField] private int _attackDamage;
    //[SerializeField] private float _attackTimerMax;
    //[SerializeField] private float _targetTimerMax;

    //public int LoadedAmmo => _loadedAmmo;
    //public ItemSO AmmoSO => _ammoSO;

    //private const int MAX_AMMO = 1;
    //private int _loadedAmmo;
    //private bool _isMoving;
    //private bool _autoMode = true;
    //private bool _targetIsLargeAsteroid;
    //private float _targetTimer;
    //private float _currentAngle;
    //private float _attackTimer;
    //private RaycastHit2D _hit;
    //private Asteroid _targetAsteroid;
    //private AsteroidGameSystem _asteroidGameSystem;
    //private TickSystem _reloadTick;

    //#region IPowerGridEntity Properties
    //public int PowerGridEntityId { get; set; }
    //public float PowerConsumption => _isMoving ? _powerConsumptionMoving : 0f;
    //public PowerGrid PowerGrid { get; set; }
    //public PowerConnections Connections { get; set; }
    //#endregion

    //public override string TooltipDescription
    //{
    //    get
    //    {
    //        return string.Empty;
    //    }
    //}

    //private new void Awake()
    //{
    //    base.Awake();
    //    _attackTimerMax += UnityEngine.Random.value;
    //    OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;
    //}

    //private new void Start()
    //{
    //    base.Start();
    //    _reloadTick = new TickSystem(0.5f, OnReloadTick);
    //    _asteroidGameSystem = FindFirstObjectByType<AsteroidGameSystem>();
    //}

    //private void OnReloadTick()
    //{
    //    if (!PowerGrid.HasEnoughPower)
    //        return;

    //    if (_loadedAmmo == 0)
    //        Reload();
    //}

    //private void Update()
    //{
    //    if (!IsBuildingFinished)
    //        return;

    //    if (_autoMode && _asteroidGameSystem != null)
    //    {
    //        _reloadTick.Update();

    //        var isAtRotationLimit = IsAtRotationLimit();
    //        _targetTimer += Time.deltaTime;
    //        if (isAtRotationLimit || (_targetTimer >= _targetTimerMax && _targetAsteroid == null))
    //        {
    //            _isMoving = false;
    //            _targetTimer = 0f;
    //            _targetAsteroid = _asteroidGameSystem.Asteroids
    //                .Where(e => e.Size != Asteroid.AsteroidSize.SMALL)
    //                .Where(e => Vector3.Distance(e.transform.position, transform.position) < _maxDistance)
    //                .OrderBy(e =>
    //                {
    //                    var horizontalDistance = Mathf.Abs(transform.position.x - e.transform.position.x);
    //                    return horizontalDistance;
    //                }).FirstOrDefault();
    //            _targetIsLargeAsteroid = _targetAsteroid != null ? _targetAsteroid.Size == Asteroid.AsteroidSize.LARGE : false;
    //        }

    //        var isFocussingTarget = IsFocussingTarget();
    //        _attackTimer += Time.deltaTime;
    //        if (_attackTimer >= _attackTimerMax && _targetAsteroid != null)
    //        {
    //            _attackTimer = 0f;
    //            if (isFocussingTarget)
    //            {
    //                Shoot(_targetAsteroid);
    //            }
    //        }

    //        if (_loadedAmmo == 0 || !PowerGrid.CheckHasEnoughPower(_powerConsumptionMoving))
    //            return;

    //        if (_targetAsteroid != null)
    //        {
    //            _isMoving = true;
    //            RotateToward(_targetAsteroid.transform.position);
    //        }
    //        HandleTargetFeedback();
    //    }
    //    else
    //    {
    //        if (Interactor.IsInteracting(this))
    //        {
    //            HandleRotation();

    //            if (Input.GetKeyDown(KeyCode.Space))
    //                Shoot();

    //            if (Input.GetKeyDown(KeyCode.R))
    //                Reload();

    //            _hit = Physics2D.CircleCast(_cannonVisual.transform.position, 10f, _cannonVisual.transform.up, _maxDistance, _asteroidLayerMask);
    //            _targetIsLargeAsteroid = _hit ? _hit.collider.gameObject.GetComponent<Asteroid>().Size == Asteroid.AsteroidSize.LARGE : false;

    //            HandleTargetFeedback();
    //        }
    //    }
    //}

    //private void HandleTargetFeedback()
    //{
    //    if (_loadedAmmo > 0 && _targetIsLargeAsteroid && !_targetIndicatorFeedback.IsPlaying)
    //        _targetIndicatorFeedback.PlayFeedbacks();
    //    else if ((!_targetIsLargeAsteroid || _loadedAmmo == 0) && _targetIndicatorFeedback.IsPlaying)
    //        _targetIndicatorFeedback.StopFeedbacks();
    //}

    //private bool IsFocussingTarget()
    //{
    //    if (_targetAsteroid == null)
    //        return false;

    //    var dir = _targetAsteroid.transform.position - _cannonVisual.transform.position;
    //    var angle = Vector3.Angle(_cannonVisual.transform.up, dir);

    //    return angle < 1f;
    //}

    //private bool IsAtRotationLimit()
    //{
    //    var angle = Mathf.Abs(Vector3.Angle(transform.up, _cannonVisual.transform.up));
    //    return angle >= _maxRotationAngle;
    //}

    //public void SetAutoMode(bool autoMode) => _autoMode = autoMode;

    //private void Reload()
    //{
    //    if (Inventory.HasItem(_ammoSO, out int availableAmmo) && _loadedAmmo < MAX_AMMO && !_reloadFeedback.IsPlaying)
    //    {
    //        _loadedAmmo++;
    //        Inventory.RemoveItem(_ammoSO);
    //        StartCoroutine(ReloadAnimationCo());
    //    }
    //}

    //private IEnumerator ReloadAnimationCo()
    //{
    //    _reloadAmmoAnimator.SetTrigger("Load");
    //    _reloadFeedback.PlayFeedbacks();

    //    yield return new WaitForSeconds(0.8f);

    //    _reloadWarmUpAnimator.SetTrigger("Load");
    //}

    //private void Shoot()
    //{
    //    if (_loadedAmmo > 0)
    //    {
    //        var isWarmUpReady = _reloadAmmoAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f;

    //        if (!isWarmUpReady)
    //            return;

    //        _shootFeedback.PlayFeedbacks();
    //        _reloadWarmUpAnimator.SetTrigger("Reset");
    //        _loadedAmmo--;
    //        if (_targetIsLargeAsteroid)
    //        {
    //            _hit.collider.GetComponent<Asteroid>().TakeDamage(_attackDamage);
    //        }
    //    }
    //}

    //private void Shoot(Asteroid targetAsteroid)
    //{
    //    if (_loadedAmmo > 0)
    //    {
    //        var isWarmUpReady = _reloadAmmoAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.99f;

    //        if (!isWarmUpReady)
    //            return;

    //        _shootFeedback.PlayFeedbacks();
    //        _reloadWarmUpAnimator.SetTrigger("Reset");
    //        _loadedAmmo--;
    //        if (_targetIsLargeAsteroid)
    //        {
    //            targetAsteroid.TakeDamage(_attackDamage);
    //        }
    //    }
    //}

    //private void HandleRotation()
    //{
    //    if (!PowerGrid.CheckHasEnoughPower(_powerConsumptionMoving))
    //    {
    //        _isMoving = false;
    //        return;
    //    }

    //    if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
    //    {
    //        _isMoving = true;
    //        _currentAngle += _rotationSpeed * Time.deltaTime;
    //        if (_currentAngle > _maxRotationAngle)
    //        {
    //            _isMoving = false;
    //            _currentAngle = _maxRotationAngle;
    //        }
    //    }
    //    else if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
    //    {
    //        _isMoving = true;
    //        _currentAngle -= _rotationSpeed * Time.deltaTime;
    //        if (_currentAngle < -_maxRotationAngle)
    //        {
    //            _isMoving = false;
    //            _currentAngle = -_maxRotationAngle;
    //        }
    //    }
    //    else
    //        _isMoving = false;

    //    _cannonVisual.transform.rotation = Quaternion.Euler(0f, 0f, _currentAngle);
    //}

    //public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    //{
    //    if (!IsBuildingFinished)
    //        return;

    //    if (interactionType == Interactor.InteractionType.START)
    //    {
    //        Interactor.Instance.StopAllInteractions();

    //        RailgunMenu.Show(this);
    //        Player.Instance.PlayerController.FreezePlayer();
    //    }
    //    else if (interactionType == Interactor.InteractionType.STOP)
    //    {
    //        _targetIndicatorFeedback.StopFeedbacks();
    //        RailgunMenu.Hide();
    //        Player.Instance.PlayerController.UnfreezePlayer();
    //    }
    //}

    //private void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    //{
    //    OnFinishedBuilding -= BaseBuilding_OnFinishedBuilding;

    //    _progressVisual.SetActive(false);
    //    _baseVisual.SetActive(true);
    //    _cannonVisual.SetActive(true);
    //    Inventory.SetFilter((ItemSO itemSO) => itemSO == _ammoSO);
    //}

    //private void RotateToward(Vector3 targetPoint)
    //{
    //    targetPoint.z = transform.position.z;

    //    var dirToTarget = (targetPoint - _cannonVisual.transform.position).normalized;
    //    var angleToTarget = Vector3.SignedAngle(transform.up, dirToTarget, transform.forward);
    //    _currentAngle = Mathf.MoveTowards(_currentAngle, angleToTarget, _rotationSpeed * Time.deltaTime);
    //    _currentAngle = Mathf.Clamp(_currentAngle, -_maxRotationAngle, _maxRotationAngle);
    //    _cannonVisual.transform.up = Quaternion.Euler(0f, 0f, _currentAngle) * Vector3.up;
    //}

    //public void OnRemovedFromPowerGrid()
    //{

    //}
}
