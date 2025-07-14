using System.Collections.Generic;
using UnityEngine;

public class LiftControlBox : MonoBehaviour, IInteractable
{
    [SerializeField] private KinematicLift _lift;

    public List<KeyCode> InteractionKeys => new List<KeyCode> { KeyCode.E };

    public Vector3 IndicatorPosition => transform.position + new Vector3(0.5f, 2f);

    public Transform Transform => transform;

    public bool AllowIndicator() => _lift.IsBuildingFinished;

    public void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (interactionType == Interactor.InteractionType.START)
        {
            _lift.Move(KinematicLift.Direction.IDLE);
            Player.Instance.PlayerController.FreezePlayer();
            LiftMenu.Show(_lift);
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            Player.Instance.PlayerController.UnfreezePlayer();
            LiftMenu.Hide();
        }
    }
}
