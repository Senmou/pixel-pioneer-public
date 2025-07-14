using MoreMountains.Feedbacks;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PlayerCamera : MonoBehaviour
{
    public static PlayerCamera Instance { get; private set; }

    [SerializeField] private CinemachineCamera _camSurface;
    [SerializeField] private float _lookAheadDistance;
    [SerializeField] private CameraZoom _zoom;

    [Space(10)]
    [Header("Camera Shakes")]
    [SerializeField] private MMF_Player _constructionSiteStartShaker;
    [SerializeField] private MMF_Player _constructionSiteLandingShaker;
    [SerializeField] private MMF_Player _playerDeathShaker;
    [SerializeField] private MMF_Player _tilePlacementShaker;

    public MMF_Player ConstructionSiteStartShaker => _constructionSiteStartShaker;
    public MMF_Player ConstructionSiteLandingShaker => _constructionSiteLandingShaker;
    public MMF_Player PlayerDeathShaker => _playerDeathShaker;
    public MMF_Player TilePlacementShaker => _tilePlacementShaker;

    private Transform _cameraTarget;

    private void Awake()
    {
        Instance = this;
        _cameraTarget = new GameObject("Camera_Target").transform;
        _camSurface.Follow = _cameraTarget;
    }

    private void Start()
    {
        _zoom.OnZoomChanged += Zoom_OnZoomChanged;
    }

    private void Zoom_OnZoomChanged(object sender, float orthoSize)
    {
        _camSurface.Lens.OrthographicSize = orthoSize;
        _camSurface.GetComponent<CinemachineConfiner2D>().InvalidateBoundingShapeCache();
    }

    private void Update()
    {
        if (Player.Instance == null)
            return;

        var targetPos = (Helper.MousePos.WithZ(0f) - Player.Instance.transform.position.WithZ(0f)) * _lookAheadDistance;
        _cameraTarget.position = Player.Instance.transform.position + targetPos;
    }

    public void SnapCamera()
    {
        if (_camSurface != null)
        {
            var posDelta = Player.Instance.transform.position.WithZ(0f) - _cameraTarget.position;
            _camSurface.OnTargetObjectWarped(_cameraTarget, posDelta);
        }
    }
}
