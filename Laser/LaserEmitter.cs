using UnityEngine;
using System;

public class LaserEmitter : MonoBehaviour
{
    public event EventHandler<OnHitEventArgs> OnHit;
    public event EventHandler OnMiss;
    public class OnHitEventArgs : EventArgs
    {
        public Vector3 hitPoint;
        public int hitLayer;
        public GameObject gameObject;
    }

    [SerializeField] private ContactFilter2D _contactFilter;

    [Header("Debug")]
    [SerializeField] private bool _showGizmos;

    private Vector3 _hitPoint;
    private Vector3 _mousePos;

    RaycastHit2D[] result = new RaycastHit2D[1];
    public void Raycast(Vector2 direction, float maxDistance, out float hitDistance, bool noHitMaxDistance = false)
    {
        int hits = Physics2D.RaycastNonAlloc(transform.position, direction, result, maxDistance, _contactFilter.layerMask);

        if (hits > 0)
        {
            _hitPoint = result[0].point;

            hitDistance = Vector2.Distance(_hitPoint, transform.position);

            OnHit?.Invoke(this, new OnHitEventArgs
            {
                hitPoint = result[0].point,
                hitLayer = result[0].transform.gameObject.layer,
                gameObject = result[0].transform.gameObject
            });
        }
        else
        {
            OnMiss?.Invoke(this, EventArgs.Empty);
            hitDistance = noHitMaxDistance ? maxDistance : 0f;
        }
    }

    public void Raycast(Vector2 direction, float maxDistance, out float hitDistance, out Vector2 hitPoint, bool noHitMaxDistance = false)
    {
        RaycastHit2D[] result = new RaycastHit2D[1];
        int hits = Physics2D.Raycast(transform.position, direction, _contactFilter, result, maxDistance);

        if (hits > 0)
        {
            _hitPoint = result[0].point;
            hitPoint = _hitPoint;

            hitDistance = Vector2.Distance(_hitPoint, transform.position);

            OnHit?.Invoke(this, new OnHitEventArgs
            {
                hitPoint = result[0].point,
                hitLayer = result[0].transform.gameObject.layer,
                gameObject = result[0].transform.gameObject
            });
        }
        else
        {
            hitPoint = Vector2.zero;
            OnMiss?.Invoke(this, EventArgs.Empty);
            hitDistance = noHitMaxDistance ? maxDistance : 0f;
        }
    }

    public void CircleCast(Vector2 direction, float maxDistance, float radius, out float hitDistance)
    {
        RaycastHit2D[] result = new RaycastHit2D[1];
        int hits = Physics2D.CircleCast(transform.position, radius, direction, _contactFilter, result, maxDistance);

        if (hits > 0)
        {
            _hitPoint = result[0].point;

            OnHit?.Invoke(this, new OnHitEventArgs
            {
                hitPoint = _hitPoint,
                hitLayer = result[0].transform.gameObject.layer,
                gameObject = result[0].transform.gameObject
            });

            hitDistance = Vector2.Distance(transform.position, _hitPoint);
        }

        hitDistance = -1f;
    }

    private void OnDrawGizmos()
    {
        if (!_showGizmos) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, _hitPoint);
    }
}
