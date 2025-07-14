using System.Collections.Generic;
using MoreMountains.Feedbacks;
using UnityEngine;
using System.Linq;
using System;

public class LaserTurrent : BaseBuilding/*, IPowerGridEntity*/
{
    //[SerializeField] private GameObject _progressVisual;
    //[SerializeField] private GameObject _baseVisual;
    //[SerializeField] private GameObject _cannonVisual;
    //[SerializeField] private GameObject _laser;
    //[SerializeField] private MMF_Player _positionShakeFeedback;

    //[Space(10)]
    //[Header("Stats")]
    //[SerializeField] private float _maxRotationAngle;
    //[SerializeField] private float _rotationSpeed;
    //[SerializeField] private float _powerConsumptionShooting;
    //[SerializeField] private int _attackDamage;
    //[SerializeField] private float _attackTimerMax;
    //[SerializeField] private float _targetTimerMax;
    //[SerializeField] private float _maxDistance;

    //private bool _canShoot;
    //private bool _isShooting;
    //private float _currentAngle;
    //private float _targetTimer;
    //private float _attackTimer;
    //private AsteroidGameSystem _asteroidGameSystem;
    //private Asteroid _targetAsteroid = null;
    //private TickSystem _checkPowerTick;

    //#region IPowerGridEntity Properties
    //public int PowerGridEntityId { get; set; }
    //public float PowerConsumption => _isShooting ? _powerConsumptionShooting : 0f;
    //public PowerGrid PowerGrid { get; set; }
    //public PowerConnections Connections { get; set; }
    //#endregion

    //#region IInteractable
    //public override bool AllowIndicator() => false;
    //public override List<KeyCode> InteractionKeys => new List<KeyCode>();
    //#endregion

    //public override string TooltipDescription
    //{
    //    get
    //    {
    //        return string.Empty;
    //    }
    //}

    //private new void Start()
    //{
    //    base.Start();
    //    OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;

    //    _asteroidGameSystem = FindAnyObjectByType<AsteroidGameSystem>();
    //    _checkPowerTick = new TickSystem(0.5f, OnPowerTick);
    //}

    //private void OnPowerTick()
    //{
    //    _canShoot = PowerGrid.CheckHasEnoughPower(_powerConsumptionShooting);
    //}

    //private void Update()
    //{
    //    if (!IsBuildingFinished)
    //        return;

    //    if (_asteroidGameSystem == null)
    //        return;

    //    _checkPowerTick.Update();

    //    if (!_canShoot)
    //    {
    //        if (_isShooting)
    //        {
    //            _isShooting = false;
    //            _laser.SetActive(false);
    //            _positionShakeFeedback.StopFeedbacks();
    //        }
    //        return;
    //    }

    //    var isAtRotationLimit = IsAtRotationLimit();

    //    _targetTimer += Time.deltaTime;
    //    if (isAtRotationLimit || (_targetTimer >= _targetTimerMax && _targetAsteroid == null))
    //    {
    //        _targetTimer = 0f;
    //        _targetAsteroid = _asteroidGameSystem.Asteroids
    //            .Where(e => Vector3.Distance(e.transform.position, transform.position) < _maxDistance)
    //            .OrderBy(e =>
    //            {
    //                var horizontalDistance = Mathf.Abs(transform.position.x - e.transform.position.x);
    //                return horizontalDistance;
    //            }).FirstOrDefault();
    //    }

    //    var isFocussingTarget = IsFocussingTarget();

    //    _attackTimer += Time.deltaTime;
    //    if (_attackTimer >= _attackTimerMax)
    //    {
    //        _isShooting = true;
    //        _attackTimer = 0f;
    //        if (isFocussingTarget)
    //        {
    //            _targetAsteroid.TakeDamage(_attackDamage);
    //        }
    //    }

    //    _laser.SetActive(isFocussingTarget);

    //    if (isFocussingTarget)
    //        _positionShakeFeedback.PlayFeedbacks();
    //    else
    //    {
    //        _positionShakeFeedback.StopFeedbacks();
    //        _isShooting = false;
    //    }

    //    if (_targetAsteroid != null)
    //    {
    //        RotateToward(_targetAsteroid.transform.position);
    //    }
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

    //private void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    //{
    //    OnFinishedBuilding -= BaseBuilding_OnFinishedBuilding;

    //    _progressVisual.SetActive(false);
    //    _baseVisual.SetActive(true);
    //    _cannonVisual.SetActive(true);
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
