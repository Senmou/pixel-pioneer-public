using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour
{
    [SerializeField] private Collider2D _collider;
    [SerializeField] private ContactFilter2D _contactFilter;

    public Collider2D Collider => _collider;

    private List<Collider2D> _results;
    public List<Collider2D> Results => _results;
    public LayerMask LayerMask => _contactFilter.layerMask;

    private void Awake()
    {
        _results = new List<Collider2D>();
    }

    public bool IsColliding(out bool hitObstacle)
    {
        int hits = Physics2D.OverlapCollider(_collider, _contactFilter, _results);
        hitObstacle = _collider.IsTouchingLayers(LayerMask.GetMask("Obstacles"));
        return hits > 0;
    }
}
