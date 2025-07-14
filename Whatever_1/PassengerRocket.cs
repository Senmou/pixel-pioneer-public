using System.Collections.Generic;
using UnityEngine;

public class PassengerRocket : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform _cameraAnchor;
    [SerializeField] private Transform _playerDropPoint;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Rocket _rocket;

    #region IInteractable
    public List<KeyCode> InteractionKeys => new List<KeyCode> { KeyCode.F };
    public Vector3 IndicatorPosition => Player.Instance.transform.position + new Vector3(0f, 2f);
    public Transform Transform => transform;
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            _rocket.RocketPlatform.SetState(RocketPlatform.State.TAKE_OFF);
        }
    }

    public bool AllowIndicator() => true;

    public void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (interactionType == Interactor.InteractionType.START)
        {
            Player.Instance.EnterVehicle();

            Player.Instance.transform.parent = _cameraAnchor;

            InputController.Instance.UseActionMap_Rocket();
            MainCanvas.Instance.Canvas.gameObject.SetActive(false);
            Player.Instance.DeselectLastActionItem();
            _canvas.gameObject.SetActive(true);
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            Player.Instance.ExitVehicle();

            Player.Instance.transform.parent = null;
            Player.Instance.transform.position = _playerDropPoint.position.WithZ(Player.Instance.transform.position.z);

            InputController.Instance.UseActionMap_Player();
            MainCanvas.Instance.Canvas.gameObject.SetActive(true);

            Player.Instance.ReselectLastActionItem();
            _canvas.gameObject.SetActive(false);
        }
    }
}
