using UnityEngine.Events;
using UnityEngine;
using System.Linq;
using System;

public class Placeable : MonoBehaviour
{
    public event EventHandler OnPlaced;
    public event EventHandler OnStartedPlacing;

    [SerializeField] private UnityEvent OnStartPlacing;
    [SerializeField] private UnityEvent OnFinishedPlacing;
    [SerializeField] protected SpriteRenderer _spriteRenderer;
    [SerializeField] protected PlacementIndicator _placementIndicator;
    [SerializeField] private TilemapCollisionChecker _tilemapCollisionChecker;

    public bool IsPlaced { get; private set; }

    protected bool _hitGround;
    private ContactFilter2D _contactFilter;
    private TooltipHandler _tooltipHandler;
    private Collider2D[] _results;

    public bool IsValidPosition
    {
        get
        {
            var worldBoundsMin = _tilemapCollisionChecker.transform.position + _tilemapCollisionChecker.Bounds.min;
            worldBoundsMin.y += 1;
            var worldBoundsMax = _tilemapCollisionChecker.transform.position + _tilemapCollisionChecker.Bounds.max;

            _results[0] = null;
            _results[1] = null;
            var hitCount = Physics2D.OverlapArea(worldBoundsMin, worldBoundsMax, _contactFilter, _results);
            var buildingBlocking = _results.Where(e => e != null && e.GetComponent<Placeable>() != this).FirstOrDefault();

            return _tilemapCollisionChecker.IsValid() && buildingBlocking == null;
        }
    }

    private void Awake()
    {
        _results = new Collider2D[2];
        _contactFilter = new ContactFilter2D();
        _contactFilter.layerMask = LayerMask.GetMask("Building");
        _contactFilter.useLayerMask = true;
        _contactFilter.useTriggers = true;
    }

    private void Update()
    {
        _placementIndicator.UpdateColor(IsValidPosition);
    }

    public void TintPreviewSprite(Color color)
    {
        _spriteRenderer.material.SetColor("_Color", color);
    }

    public void ResetTintColor()
    {
        _spriteRenderer.material.SetColor("_Color", Color.white);
    }

    public virtual void StartPlacing()
    {
        _tooltipHandler = GetComponent<TooltipHandler>();

        if (_tooltipHandler != null)
            _tooltipHandler.enabled = false;

        _placementIndicator.Init(_spriteRenderer.sprite);
        OnStartPlacing?.Invoke();
        OnStartedPlacing?.Invoke(this, EventArgs.Empty);
    }

    public void FinishPlacing()
    {
        if (_tooltipHandler != null)
            _tooltipHandler.enabled = true;

        IsPlaced = true;
        _placementIndicator.Hide();
        _tilemapCollisionChecker.Destroy();
        ResetTintColor();
        OnFinishedPlacing?.Invoke();
        OnPlaced?.Invoke(this, EventArgs.Empty);
        BuildingController.Instance.OnBuildingPlaced?.Invoke(this, this);
    }
}
