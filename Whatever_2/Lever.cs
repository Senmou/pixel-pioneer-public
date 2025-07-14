using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class Lever : MonoBehaviour, IInteractable
{
    [SerializeField] private UnityEvent _onButtonPressed;

    #region IInteractable
    public bool IsButton => true;
    public int Priority => 1;
    public List<KeyCode> InteractionKeys => new List<KeyCode> { KeyCode.F };

    public Vector3 IndicatorPosition
    {
        get
        {
            if (this == null)
                return Vector3.zero;
            return transform.position + 1.5f * Vector3.up;
        }
    }

    public Transform Transform => transform;

    public bool AllowIndicator() => true;
    #endregion

    public void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (keyCode == KeyCode.F)
        {
            _onButtonPressed?.Invoke();
        }
    }
}
