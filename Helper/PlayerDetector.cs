using UnityEngine.Events;
using UnityEngine;
using System;

public class PlayerDetector : MonoBehaviour
{
    [SerializeField] private UnityEvent OnPlayerEnter;
    [SerializeField] private UnityEvent OnPlayerExit;

    public event EventHandler<bool> OnPlayerDetection;

    public bool IsPlayerInRange { get; private set; }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (Player.Instance == null || other.attachedRigidbody != Player.Instance.Body)
            return;

        IsPlayerInRange = true;
        OnPlayerDetection?.Invoke(this, true);
        OnPlayerEnter?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (Player.Instance == null || other.attachedRigidbody != Player.Instance.Body)
            return;

        IsPlayerInRange = false;
        OnPlayerDetection?.Invoke(this, false);
        OnPlayerExit?.Invoke();
    }
}
