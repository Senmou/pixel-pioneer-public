using System.Collections.Generic;
using UnityEngine;

public class LadderActionItem : BuildableActionItem
{
    [SerializeField] private ContactFilter2D _restrictionFilter;

    private List<Collider2D> _results;

    private void Awake()
    {
        _results = new();
    }

    public override bool CanBePlaced(Vector3Int tilePos)
    {
        var hit = Physics2D.OverlapBox(tilePos.ToV3() + Vector3.one, _collider.bounds.size, 0f, _restrictionFilter, _results);
        return hit == 0;
    }
}
