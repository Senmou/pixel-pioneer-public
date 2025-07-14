using Unity.Cinemachine;
using UnityEngine;

public class LevelBorderController : MonoBehaviour
{
    public static LevelBorderController Instance { get; private set; }

    [SerializeField] private Transform _levelBorderTop;
    [SerializeField] private Transform _levelBorderLeft;
    [SerializeField] private Transform _levelBorderRight;
    [SerializeField] private Transform _levelBorderBottom;
    [SerializeField] private PolygonCollider2D _cameraConfinerShape;
    [SerializeField] private CinemachineConfiner2D _surfaceCameraConfiner;
    [SerializeField] private CinemachineConfiner2D _undergroundCameraConfiner;
    [SerializeField] private WorldParameterSO _worldParameterSO;

    private const float BORDER_WIDTH = 25f;
    private Vector3 _origin;
    private float _levelWidth;

    private void Awake()
    {
        Instance = this;
    }

    public void InitBorders(Vector3 origin, float levelWidth, float levelHeight)
    {
        _origin = origin;
        _levelWidth = levelWidth;

        var leftX = origin.x - BORDER_WIDTH;
        var leftY = origin.y - BORDER_WIDTH;
        _levelBorderLeft.position = new Vector3(leftX, leftY);
        _levelBorderLeft.localScale = new Vector3(BORDER_WIDTH, 2f * levelHeight + 2f * BORDER_WIDTH);

        var rightX = origin.x + levelWidth;
        var rightY = origin.y - BORDER_WIDTH;
        _levelBorderRight.position = new Vector3(rightX, rightY);
        _levelBorderRight.localScale = new Vector3(BORDER_WIDTH, 2f * levelHeight + 2f * BORDER_WIDTH);

        var botX = origin.x;
        var botY = origin.y - BORDER_WIDTH;
        _levelBorderBottom.position = new Vector3(botX, botY);
        _levelBorderBottom.localScale = new Vector3(levelWidth, BORDER_WIDTH);

        var topX = origin.x;
        var topY = origin.y + 2f * levelHeight;
        _levelBorderTop.position = new Vector3(topX, topY);
        _levelBorderTop.localScale = new Vector3(levelWidth, BORDER_WIDTH);

        var confinerPoints = new Vector2[4] {
            origin + new Vector3(0.5f, 0f),
            origin + new Vector3(0.5f, 2f * levelHeight),
            origin + new Vector3(levelWidth - 0.5f , 2f * levelHeight),
            origin + new Vector3(levelWidth - 0.5f , 0f)
        };

        _cameraConfinerShape.points = confinerPoints;
        _surfaceCameraConfiner.InvalidateBoundingShapeCache();
    }
}
