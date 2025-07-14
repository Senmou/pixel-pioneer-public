using System.Collections.Generic;
using UnityEngine.Localization;
using MoreMountains.Feedbacks;
using Newtonsoft.Json;
using static Helper;
using UnityEngine;
using System;

public class KinematicLift : BaseBuilding, IPowerGridEntity, ISaveable
{
    [SerializeField] private Rigidbody2D _platform;
    [SerializeField] private Transform _cameraAnchor;
    [SerializeField] private float _normalSpeed;
    [SerializeField] private float _descendingLimit;
    [SerializeField] private float _blockDestructionTime;
    [SerializeField] private Detector _platformGroundDetector;
    [SerializeField] private LiftControlBox _controlBox;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private float _powerConsumptionNormal;
    [SerializeField] private Transform _tileDestructionAnchor;
    [SerializeField] private MMF_Player _tileDestructionFeedback;
    [SerializeField] private MMF_Player _movingFeedback;
    [SerializeField] private AudioPlayer _movingAudioPlayer;
    [SerializeField] private AudioPlayer _tileDestructionAudioPlayer;
    [SerializeField] private ParticleSystem _musicParticleSystem;

    [Space(10)]
    [Header("Ropes")]
    [SerializeField] private Rope _leftRope;
    [SerializeField] private Rope _rightRope;
    [SerializeField] private Transform _leftRopeAnchor;
    [SerializeField] private Transform _rightRopeAnchor;
    [SerializeField] private Transform _leftWheel;
    [SerializeField] private Transform _rightWheel;
    [SerializeField] private float _wheelRotationSpeed;

    [Header("Localization")]
    [SerializeField] private LocalizedString _noPowerString;
    [SerializeField] private LocalizedString _obstacleString;
    [SerializeField] private LocalizedString _maxDepthString;

    #region IPowerGridEntity Properties
    public int PowerGridEntityId { get; set; }
    public int Priority { get => -1; }
    public bool NeedsPower { get; set; }
    public float PowerConsumption => _currentPowerConsumption;
    public float MaxPowerConsumption => _powerConsumptionNormal;
    public PowerGrid PowerGrid { get; set; }
    public PowerConnections Connections { get; set; }
    #endregion

    #region IInteractable
    // The lift control box controls the interaction
    public override bool AllowIndicator() => false;
    public override List<KeyCode> InteractionKeys => new List<KeyCode>();
    #endregion

    #region ITooltip
    public override IPowerGridEntity TooltipPowerGridEntity => this;
    #endregion

    public bool DrillMode => _drillMode;
    public bool PlayMusic => _playMusic;

    private const float MAX_LOCAL_POSITION = -0.2f;
    private bool _drillMode;
    private bool _playMusic;
    private float MinDepth => transform.position.y - _descendingLimit;
    private float _currentPowerConsumption;
    private float _blockDestructionTimer;
    private Direction _currentDirection;
    private float RopeLength => Mathf.Abs(_leftRope.transform.position.y - _leftRopeAnchor.position.y);

    public enum Direction
    {
        UP,
        DOWN,
        IDLE,
        DRILL
    }

    private new void Awake()
    {
        base.Awake();
        OnFinishedBuilding += BaseBuilding_OnFinishedBuilding;

        _platform.gameObject.SetActive(false);
        _platform.transform.localPosition = _platform.transform.localPosition.WithY(MAX_LOCAL_POSITION);
    }

    private void Update()
    {
        if (!IsBuildingFinished) return;

        _leftRope.SetEndPoint(_leftRopeAnchor.position);
        _rightRope.SetEndPoint(_rightRopeAnchor.position);
    }

    public void Move(Direction direction)
    {
        NeedsPower = direction != Direction.IDLE;

        if (!PowerGrid.HasPowerForEntity(this, updatePrioList: true))
        {
            if (direction != Direction.IDLE)
                FloatingTextController.Instance.SpawnText($"{_noPowerString.GetLocalizedString()}!", _platform.transform.position);

            _tileDestructionFeedback.StopFeedbacks();
            _movingFeedback.StopFeedbacks();
            _movingAudioPlayer.StopSound();
            _platform.linearVelocity = Vector2.zero;
            _currentPowerConsumption = 0f;
            return;
        }

        _currentPowerConsumption = direction == Direction.IDLE ? 0f : _powerConsumptionNormal;

        if (Interactor.IsInteracting(_controlBox))
            direction = Direction.IDLE;

        _currentDirection = direction;

        if (direction != Direction.DOWN)
            _blockDestructionTimer = 0f;

        if (direction != Direction.IDLE)
        {
            _movingFeedback.PlayFeedbacks();
            _movingAudioPlayer.PlaySound();
        }

        if (direction == Direction.DOWN)
        {
            var hitGround = _platformGroundDetector.IsColliding(out bool hitObstacle);
            if (_platform.transform.localPosition.y <= -_descendingLimit - 0.05f)
            {
                FloatingTextController.Instance.SpawnText($"{_maxDepthString.GetLocalizedString()}!", _platform.transform.position);

                _platform.transform.localPosition = _platform.transform.localPosition.WithY(-_descendingLimit);
                _platform.linearVelocity = Vector2.zero;
                return;
            }

            if (hitObstacle)
            {
                FloatingTextController.Instance.SpawnText($"{_obstacleString.GetLocalizedString()}!", _platform.transform.position);

                _platform.linearVelocity = Vector2.zero;
                return;
            }

            if (hitGround)
            {
                _blockDestructionTimer += Time.deltaTime;
                _tileDestructionFeedback.PlayFeedbacks();

                if (_blockDestructionTimer > _blockDestructionTime)
                {
                    _blockDestructionTimer = 0f;
                    for (int i = 0; i < 8; i++)
                    {
                        var offset = _tileDestructionAnchor.position.x + i;
                        var tilePos = TilemapChunkSystem.Instance.GetTileIndex(_tileDestructionAnchor.position.WithX(offset), out BlockType blockType, out SiblingRuleTile ruleTile);
                        TilemapChunkSystem.Instance.DestroyTile(tilePos);
                    }
                    _tileDestructionAudioPlayer.PlaySound(true);
                }

                _platform.linearVelocity = Vector2.zero;
                return;
            }

            _platform.linearVelocity = -Vector2.up * _normalSpeed;

        }
        else if (direction == Direction.UP)
        {
            if (_platform.transform.localPosition.y >= MAX_LOCAL_POSITION - 0.001f)
            {
                _platform.linearVelocity = Vector2.zero;
                _platform.transform.localPosition = _platform.transform.localPosition.WithY(MAX_LOCAL_POSITION);
                return;
            }

            if (!PowerGrid.HasEnoughPower)
            {
                _platform.linearVelocity = Vector2.zero;
                return;
            }

            _platform.linearVelocity = Vector2.up * _normalSpeed;
        }
        else if (direction == Direction.IDLE)
        {
            _platform.linearVelocity = Vector2.zero;
            _tileDestructionFeedback.StopFeedbacks();
            _movingFeedback.StopFeedbacks();
            _movingAudioPlayer.StopSound();
        }

        if (_platform.linearVelocity.y < 0f || direction == Direction.IDLE)
        {
            _leftRope.UpdateSegmentLength(RopeLength + 0.1f);
            _rightRope.UpdateSegmentLength(RopeLength + 0.1f);
        }
        else if (_platform.linearVelocity.y > 0f)
        {
            _leftRope.UpdateSegmentLength(RopeLength - 0.3f);
            _rightRope.UpdateSegmentLength(RopeLength - 0.3f);
        }

        _leftWheel.Rotate(0f, 0f, _platform.linearVelocity.y * _wheelRotationSpeed * Time.deltaTime);
        _rightWheel.Rotate(0f, 0f, -_platform.linearVelocity.y * _wheelRotationSpeed * Time.deltaTime);
    }

    public void SetDrillMode(bool drillMode)
    {
        _drillMode = drillMode;
    }

    public void SetPlayMusic(bool playMusic, bool stopGameMusic = false)
    {
        _playMusic = playMusic;

        if (_playMusic && PowerGrid.HasEnoughPower)
        {
            _audioSource.Play();

            if (stopGameMusic)
                GameMusicController.Instance.PauseMusic(true);

            _musicParticleSystem.Play();
        }
        else
        {
            _audioSource.Stop();

            if (stopGameMusic)
                GameMusicController.Instance.PauseMusic(false);

            _musicParticleSystem.Stop();
        }
    }

    public void OnEnterPlatform()
    {
        TurnOnRopeSimulation();
    }

    public void OnExitPlatform()
    {
        TurnOffRopeSimulation(2f);
        Move(Direction.IDLE);
    }

    public void TurnOffRopeSimulation(float delay)
    {
        _leftRope.TurnOffSimulationDelayed(delay);
        _rightRope.TurnOffSimulationDelayed(delay);
    }

    public void TurnOnRopeSimulation()
    {
        _leftRope.StartSimulation();
        _rightRope.StartSimulation();
    }

    // Trigger Handler
    public void PreventItemsFromPushedThrough(Collider2D other)
    {
        if (_currentDirection == Direction.IDLE)
            return;

        if (other.isTrigger)
            return;

        var worldItem = other.GetComponentInParent<WorldItem>();
        if (worldItem == null)
            return;

        worldItem.transform.position = worldItem.transform.position + new Vector3(0f, 1f);
    }

    private void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        Inventory.SetFilter_PreventAll();

        _platform.gameObject.SetActive(true);
        _leftRope.CreateSegments(_leftRope.transform.position, _leftRopeAnchor.position);
        _rightRope.CreateSegments(_rightRope.transform.position, _rightRopeAnchor.position);
        _leftRope.SetStartPoint(_leftRope.transform.position);
        _rightRope.SetStartPoint(_rightRope.transform.position);
        _leftRope.UpdateSegmentLength(RopeLength + 1f);
        _rightRope.UpdateSegmentLength(RopeLength + 1f);

        SetPlayMusic(true);
    }

    protected override void OnInteractionFinishedBuilding(WorldItem carryItem)
    {

    }

    public void OnRemovedFromPowerGrid()
    {

    }

    public class SaveData
    {
        public SerializableVector platformPosition;
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData();
        saveData.platformPosition = _platform.transform.position;
        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        if (saveData == null)
            return;
        _platform.transform.position = saveData.platformPosition;
    }
}
