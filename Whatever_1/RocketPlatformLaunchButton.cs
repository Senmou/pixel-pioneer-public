using System.Collections.Generic;
using UnityEngine;

public class RocketPlatformLaunchButton : MonoBehaviour, IInteractable
{
    [SerializeField] private PlayerDetector _playerDetector;
    [SerializeField] private RocketPlatform _rocketPlatform;
    [SerializeField] private float _rocketForce;

    #region IInteractable
    public bool IsButton => true;
    public int Priority => 1;
    public List<KeyCode> InteractionKeys => new List<KeyCode> { KeyCode.W };

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
        if (keyCode == KeyCode.W)
        {
            _rocketPlatform.SetState(RocketPlatform.State.TAKE_OFF);
        }
    }
}
