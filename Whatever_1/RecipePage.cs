using UnityEngine.Localization;
using UnityEngine;

public class RecipePage : MonoBehaviour, ITooltip
{
    [SerializeField] protected Rigidbody2D _body;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private ContactFilter2D _terrainContactFilter;

    [SerializeField] private LocalizedString _tooltipTitleString;
    [SerializeField] private LocalizedString _tooltipDescriptionString;

    #region ITooltip
    public string TooltipTitle => $"{_tooltipTitleString.GetLocalizedString()}";
    public string TooltipDescription => $"{_tooltipDescriptionString.GetLocalizedString()}";
    #endregion

    private bool _isRecovered;
    private float _distanceToPlayer;

    private void Update()
    {
        if (Player.Instance == null)
            return;

        if (TilemapChunkSystem.Instance == null || !TilemapChunkSystem.Instance.IsWorldSpawned)
            return;

        _distanceToPlayer = Vector3.Distance(Player.Instance.transform.position.WithZ(0f), transform.position.WithZ(0f));
        if (!_isRecovered && _body.bodyType == RigidbodyType2D.Kinematic && _distanceToPlayer < 10f)
        {
            var isOverlapping = IsOverlappingWithTerrain();
            _isRecovered = !isOverlapping;
            _body.bodyType = _isRecovered ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;
        }
    }

    private bool IsOverlappingWithTerrain()
    {
        int v = Physics2D.OverlapCollider(_collider, _terrainContactFilter, new Collider2D[1]);
        return v > 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Player.Instance == null)
            return;

        if (other.attachedRigidbody != Player.Instance.Body)
            return;

        Destroy(gameObject);
    }
}
