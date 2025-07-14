using UnityEngine.Events;
using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class TriggerHandler : MonoBehaviour
{
    [SerializeField] private UnityEvent<Collider2D> OnTriggerEnter;
    [SerializeField] private UnityEvent<Collider2D> OnTriggerExit;

    public event EventHandler<CollisionEventArgs> OnEnter;
    public event EventHandler<CollisionEventArgs> OnExit;
    public class CollisionEventArgs
    {
        public Collider2D trigger;
        public Collider2D collider;
    }

    private Collider2D _trigger;

    private void Awake()
    {
        _trigger = GetComponent<Collider2D>();
        if (!_trigger.isTrigger)
        {
            Debug.LogWarning("Collider automatically set to trigger", _trigger);
            _trigger.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled)
            return;

        OnTriggerEnter?.Invoke(other);
        OnEnter?.Invoke(this, new CollisionEventArgs { trigger = _trigger, collider = other });
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        OnTriggerExit?.Invoke(other);
        OnExit?.Invoke(this, new CollisionEventArgs { trigger = _trigger, collider = other });
    }
}
