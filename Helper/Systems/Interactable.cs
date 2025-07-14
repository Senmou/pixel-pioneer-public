using UnityEngine;
using System;

[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    public event EventHandler OnInteract;
    public event EventHandler OnInteractAlt;

    private Collider2D _triggerCollider;

    private void Awake()
    {
        _triggerCollider = GetComponent<Collider2D>();

        if (_triggerCollider.isTrigger == false)
        {
            Debug.Log("Interactable collider is not set as trigger!");
        }
    }

    public void Interact()
    {
        OnInteract?.Invoke(this, EventArgs.Empty);
    }

    public void InteractAlt()
    {
        OnInteractAlt?.Invoke(this, EventArgs.Empty);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var player = other.gameObject.GetComponent<Player>();
        if (player == null)
            return;

        //InteractionController.Instance.AddInteractable(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        var player = other.gameObject.GetComponent<Player>();
        if (player == null)
            return;

        //InteractionController.Instance.RemoveInteractable(this);
    }
}
