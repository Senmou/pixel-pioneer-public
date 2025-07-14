using UnityEngine;
using System;

public class PowerConnectionController : MonoBehaviour
{
    public event EventHandler<State> OnStateChanged;
    public event EventHandler OnStartedPlacingPowerLine;
    public event EventHandler<OnPowerLineEventArgs> OnSetPowerLineEndPosition;
    public event EventHandler<OnPowerLineEventArgs> OnSelectedFirstSnappingPointToRemove;
    public event EventHandler<OnPowerLineEventArgs> OnSetPowerLineStartPosition;
    public class OnPowerLineEventArgs : EventArgs
    {
        public PowerLineSnappingPoint startSnappingPoint;
        public PowerLineSnappingPoint endSnappingPoint;
        public Rope rope;
        public Vector3 ropeStartPosition;
        public float maxRopeLength;
    }

    public static PowerConnectionController Instance { get; private set; }

    [SerializeField] private Rope _ropePrefab;
    [SerializeField] private LayerMask _buildingLayerMask;
    [SerializeField] private float _maxDistanceBetweenSnappingPoints;
    [SerializeField] private float _ropeLengthFactor = 1f;

    public State CurrentState => _state;
    public float RopeLengthFactor => _ropeLengthFactor;

    private State _state;
    private Rope _spawnedRope;
    private Vector2 _ropeStartPos;
    private Vector2 _ropeEndPos;
    private PowerLineSnappingPoint _lastSnappingPoint;
    private PowerLineSnappingPoint _startSnappingPoint;
    private PowerLineSnappingPoint _firstRemoveSnappingPoint;
    private PowerLineSnappingPoint _secondRemoveSnappingPoint;

    public enum State
    {
        None,
        StartConnection,
        EndConnection,
        RemoveConnection
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            SetState(State.RemoveConnection);
        }

        switch (_state)
        {
            case State.None:
                break;
            case State.StartConnection:
                {
                    if (_lastSnappingPoint ?? false)
                        _lastSnappingPoint.ResetColor();

                    _spawnedRope.SetStartPoint(Player.Instance.transform.position + new Vector3(0f, 1f, -4f));
                    _spawnedRope.SetEndPoint(Helper.MousePos);

                    Ray ray = new Ray(Helper.MousePos.WithZ(-10f), Vector3.forward);
                    var hit = Physics2D.GetRayIntersection(ray, 20f, _buildingLayerMask);
                    if (hit)
                    {
                        var snappingPoint = hit.collider.gameObject.GetComponentInChildren<PowerLineSnappingPoint>();
                        if (snappingPoint != null && snappingPoint.IsSnappable)
                        {
                            _lastSnappingPoint = snappingPoint;
                            snappingPoint.SetSelectedColor();
                            _spawnedRope.SetEndPoint(snappingPoint.transform.position);

                            if (Input.GetMouseButtonDown(0))
                            {
                                _startSnappingPoint = snappingPoint;
                                OnSetPowerLineStartPosition?.Invoke(this, new OnPowerLineEventArgs
                                {
                                    maxRopeLength = _maxDistanceBetweenSnappingPoints,
                                    ropeStartPosition = snappingPoint.transform.position,
                                    startSnappingPoint = snappingPoint
                                });
                                _ropeStartPos = snappingPoint.transform.position;
                                SetState(State.EndConnection);
                            }
                        }
                    }
                }
                break;
            case State.EndConnection:
                {
                    _lastSnappingPoint?.ResetColor();

                    _spawnedRope.SetStartPoint(Helper.MousePos);

                    var currentRopeLength = Vector2.Distance(_ropeStartPos, Helper.MousePos);
                    _spawnedRope.UpdateSegmentLength(_ropeLengthFactor * currentRopeLength);

                    Ray ray = new Ray(Helper.MousePos.WithZ(-10f), Vector3.forward);
                    var hit = Physics2D.GetRayIntersection(ray, 20f, _buildingLayerMask);
                    if (hit)
                    {
                        var snappingPoint = hit.collider.gameObject.GetComponentInChildren<PowerLineSnappingPoint>();
                        if (snappingPoint != null && snappingPoint.IsSnappable)
                        {
                            _lastSnappingPoint = snappingPoint;
                            snappingPoint.SetSelectedColor();

                            _spawnedRope.SetStartPoint(snappingPoint.transform.position);

                            if (Input.GetMouseButtonDown(0))
                            {
                                _ropeEndPos = snappingPoint.transform.position;

                                var ropeLength = Vector2.Distance(_ropeStartPos, _ropeEndPos);
                                if (ropeLength < _maxDistanceBetweenSnappingPoints)
                                {
                                    _spawnedRope.TurnOffSimulationDelayed(10f);
                                    _startSnappingPoint.OnConnectPowerLine(snappingPoint, _spawnedRope);
                                    _spawnedRope = null;
                                    OnSetPowerLineEndPosition?.Invoke(this, new OnPowerLineEventArgs
                                    {
                                        startSnappingPoint = _startSnappingPoint,
                                        endSnappingPoint = snappingPoint
                                    });
                                    SetState(State.None);
                                }
                            }
                        }
                    }
                }
                break;
            case State.RemoveConnection:
                {
                    Ray ray = new Ray(Helper.MousePos.WithZ(-10f), Vector3.forward);
                    var hit = Physics2D.GetRayIntersection(ray, 20f, _buildingLayerMask);
                    if (hit)
                    {
                        var snappingPoint = hit.collider.gameObject.GetComponentInChildren<PowerLineSnappingPoint>();
                        if (snappingPoint != null)
                        {
                            if (Input.GetMouseButtonDown(0))
                            {
                                if (_firstRemoveSnappingPoint == null)
                                {
                                    _firstRemoveSnappingPoint = snappingPoint;

                                    // Delete single connection without selecting the other point
                                    if (_firstRemoveSnappingPoint.ConnectedPoints.Count == 1)
                                    {
                                        _firstRemoveSnappingPoint.OnRemovePowerLine(_firstRemoveSnappingPoint.ConnectedPoints[0].otherSnappingPoint);
                                        _firstRemoveSnappingPoint = null;
                                        SetState(State.None);
                                        return;
                                    }

                                    OnSelectedFirstSnappingPointToRemove?.Invoke(this, new OnPowerLineEventArgs { startSnappingPoint = snappingPoint });
                                }
                                else if (_firstRemoveSnappingPoint != snappingPoint)
                                {
                                    _secondRemoveSnappingPoint = snappingPoint;
                                }

                                if (_firstRemoveSnappingPoint != null && _secondRemoveSnappingPoint != null)
                                {
                                    _firstRemoveSnappingPoint.OnRemovePowerLine(_secondRemoveSnappingPoint);
                                    _firstRemoveSnappingPoint = null;
                                    _secondRemoveSnappingPoint = null;
                                    SetState(State.None);
                                }
                            }
                        }
                    }
                }
                break;
        }
    }

    public void CancelConnection()
    {
        bool cancelPlacing = (_state == State.StartConnection || _state == State.EndConnection) && _spawnedRope != null;
        bool cancelRemoving = _state == State.RemoveConnection;
        if (cancelPlacing || cancelRemoving)
        {
            _firstRemoveSnappingPoint = null;
            _secondRemoveSnappingPoint = null;
            if (_spawnedRope != null)
                Destroy(_spawnedRope.gameObject);
            SetState(State.None);
        }
    }

    private void SetState(State state)
    {
        if (_state == state)
            return;

        if (state == State.StartConnection)
        {
            _spawnedRope = Instantiate(_ropePrefab);
            _spawnedRope.transform.position = _spawnedRope.transform.position.WithZ(0.1f);
            _spawnedRope.CreateSegments(Player.Instance.transform.position, Helper.MousePos);
            OnStartedPlacingPowerLine?.Invoke(this, EventArgs.Empty);
        }

        _state = state;

        OnStateChanged?.Invoke(this, _state);
    }

    public void StartPlacingPowerConnection()
    {
        SetState(State.StartConnection);
    }

    public void StartRemovingPowerConnection()
    {
        SetState(State.RemoveConnection);
    }
}
