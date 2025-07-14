using UnityEngine;

public class ObjectDetectorOverlay : MonoBehaviour
{
    public static ObjectDetectorOverlay Instance { get; private set; }

    public bool IsActive => _container.activeSelf;

    [SerializeField] private GameObject _container;
    [SerializeField] private RenderTexture _renderTexture;
    [SerializeField] private Camera _objectDetectorOverlayCamera;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private LayerMask _objectDetectorMarkerLayerMask;
    [SerializeField] private float _triggerZoomDistance;
    [SerializeField] private float _triggerMarkerAlphaDistance;
    [SerializeField] private float _maxOrthoSize;
    [SerializeField] private float _lerpZoomSpeed;
    [SerializeField] private float _lerpAlphaSpeed;

    private float _targetOrthoSize;

    private void Awake()
    {
        Instance = this;
        Resize(_renderTexture, Screen.width, Screen.height);
        _targetOrthoSize = _maxOrthoSize;
    }

    private void Update()
    {
        if (IsActive)
        {
            var markerCollider = Physics2D.OverlapCircleAll(Player.Instance.transform.position, 25f, _objectDetectorMarkerLayerMask);

            float nearestDistance = float.PositiveInfinity;
            foreach (var collider in markerCollider)
            {
                var marker = collider.GetComponent<ObjectDetectorMarker>();
                if (marker != null)
                {
                    var distanceToPlayer = Vector2.Distance(collider.transform.position, Player.Instance.transform.position);
                    if (distanceToPlayer < nearestDistance)
                        nearestDistance = distanceToPlayer;

                    marker.SetTargetAlpha(distanceToPlayer < _triggerMarkerAlphaDistance ? 0f : 1, _lerpAlphaSpeed);
                }
            }

            if (nearestDistance < _triggerZoomDistance)
                _targetOrthoSize = _playerCamera.orthographicSize;
            else
                _targetOrthoSize = _maxOrthoSize;

            UpdateOrthographicSize();
        }
    }

    private void UpdateOrthographicSize()
    {
        var orthoSize = Mathf.MoveTowards(_objectDetectorOverlayCamera.orthographicSize, _targetOrthoSize, _lerpZoomSpeed * Time.deltaTime);
        _objectDetectorOverlayCamera.orthographicSize = orthoSize;
    }

    private void Resize(RenderTexture renderTexture, int width, int height)
    {
        if (renderTexture)
        {
            renderTexture.Release();
            renderTexture.width = width;
            renderTexture.height = height;
        }
    }

    public void Show()
    {
        _container.SetActive(true);
    }

    public void Hide()
    {
        _container.SetActive(false);
    }
}
