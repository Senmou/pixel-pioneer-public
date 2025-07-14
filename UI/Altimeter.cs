using MoreMountains.Tools;
using UnityEngine;
using TMPro;

public class Altimeter : MonoBehaviour, MMEventListener<PlayerEvent>
{
    public static Altimeter Instance { get; private set; }

    [Space(10)]
    [SerializeField] private Transform _currentHeightMinPosition;
    [SerializeField] private Transform _currentHeightMaxPosition;
    [SerializeField] private TextMeshProUGUI _zeroHeightText;
    [SerializeField] private TextMeshProUGUI _currentHeightText;
    [SerializeField] private WorldParameterSO _worldParameterSO;
    [SerializeField] private GameObject _exclamationMark;

    public float PlayerPosY { get; private set; }

    private int _lastHeight;
    private int _minHeight;
    private int _maxHeight;

    public void OnMMEvent(PlayerEvent playerEvent)
    {
        if (playerEvent.positionData != null)
        {
            _exclamationMark.SetActive(playerEvent.positionData.isBelowDepthLimit);
        }
    }

    private void Awake()
    {
        Instance = this;
        _minHeight = 0;
        _maxHeight = _worldParameterSO.chunkCountVertical * _worldParameterSO.chunkDimension;
        _exclamationMark.SetActive(false);
    }

    private void Start()
    {
        MMEventManager.AddListener(this);
    }

    private void Update()
    {
        if (Player.Instance != null)
            SetCurrentHeight(Player.Instance.transform.position.y);
    }

    private void SetCurrentHeight(float currentHeight)
    {
        var deepestSurfaceHeight = TilemapChunkSystem.Instance != null ? TilemapChunkSystem.Instance.World.deepestSurfaceHeight : 0;

        float maxPosY = _currentHeightMaxPosition.transform.position.y;
        float minPosY = _currentHeightMinPosition.transform.position.y;

        int smallerValue = _maxHeight < _minHeight ? _maxHeight : _minHeight;
        int biggerValue = _maxHeight > _minHeight ? _maxHeight : _minHeight;
        var sum = Mathf.Abs(biggerValue) + Mathf.Abs(smallerValue);
        float t = 1f - (biggerValue - currentHeight) / (float)sum;

        float currentHeightY = Mathf.Lerp(minPosY, maxPosY, t);
        _currentHeightText.transform.position = _zeroHeightText.transform.position.WithY(currentHeightY);
        int currentHeightInt = (int)currentHeight - deepestSurfaceHeight;
        PlayerPosY = currentHeightInt;

        if (Mathf.Abs(currentHeightInt - _lastHeight) >= 1)
        {
            _lastHeight = currentHeightInt;
            _currentHeightText.text = $"{currentHeightInt}m";
        }
    }
}
