using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/**
 * DON'T use layer "MiningStencil" for anything but the laser cannon stencil
 */

public class MiningStencil : MonoBehaviour
{
    [SerializeField] private Sprite _referenceSprite;

    public EdgeCollider2D EdgeCollider => _edgeCollider;
    public PolygonCollider2D PolygonCollider => _polygonCollider;

    private EdgeCollider2D _edgeCollider;
    private PolygonCollider2D _polygonCollider;

    private void Awake()
    {
        _edgeCollider = GetComponent<EdgeCollider2D>();
        _polygonCollider = GetComponent<PolygonCollider2D>();
    }

    public List<Vector2> GetVerticesFromPhysicsShape()
    {
        List<Vector2> physicsShape = new List<Vector2>();
        _referenceSprite.GetPhysicsShape(0, physicsShape);
        return physicsShape;
    }

    public void GeneratePolygonCollider(List<Vector2> vertices)
    {
        _polygonCollider = gameObject.AddComponent<PolygonCollider2D>();
        var polygonPoints = vertices.ToArray();
        _polygonCollider.points = polygonPoints;
    }

    public void GenerateEdgeCollider(List<Vector2> vertices)
    {
        _edgeCollider = gameObject.AddComponent<EdgeCollider2D>();
        vertices.Add(vertices.ElementAt(0));
        _edgeCollider.SetPoints(vertices);
    }
}