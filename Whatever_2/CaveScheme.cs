using System.Collections.Generic;
using UnityEngine;
using System;

[Flags]
public enum CaveOpenings
{
    Left = 1,
    Right = 2,
    Top = 4,
    Bottom = 8
}

public class CaveScheme : MonoBehaviour
{
    [SerializeField] private CaveOpenings _openings;
    [SerializeField] private TilemapCollisionChecker _collisionChecker;

    public CaveOpenings Openings => _openings;
    public TilemapCollisionChecker CollisionChecker => _collisionChecker;

    public List<Vector3> GetCavePositionList() => _collisionChecker.GetGroundTilePositions();
    public bool HasAtLeastOpening(CaveOpenings opening) => _openings.HasFlag(opening);
    public bool HasMatchingOpenings(CaveOpenings includedOpenings, CaveOpenings excludedOpenings)
    {
        var hasAllIncludedOpenings = (_openings & includedOpenings) == includedOpenings;
        var hasAnyExcludedOpenings = (_openings & excludedOpenings) != 0;
        return hasAllIncludedOpenings && !hasAnyExcludedOpenings;
    }
}
