using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class BuildingDoor : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform _cameraAnchor;
    [SerializeField] private BaseBuilding _baseBuilding;

    [SerializeField] private UnityEvent OnEnterBuilding;
    [SerializeField] private UnityEvent OnExitBuilding;

    public List<KeyCode> InteractionKeys => new List<KeyCode> { KeyCode.W, KeyCode.S };

    public Vector3 IndicatorPosition => transform.position + 3f * Vector3.up;

    public Transform Transform => transform;

    public void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (!_baseBuilding.IsBuildingFinished)
            return;

        if (!Player.Instance.IsInBuilding && keyCode == KeyCode.W)
        {
            Player.Instance.EnterVehicle();

            _baseBuilding.OnEnteredBuilding();
            OnEnterBuilding?.Invoke();
        }
        else if (Player.Instance.IsInBuilding && keyCode == KeyCode.S)
        {
            Player.Instance.ExitVehicle();
            _baseBuilding.OnExitedBuilding();
            OnExitBuilding?.Invoke();
        }
    }

    public void ForceCancelInteraction()
    {
        Player.Instance.ExitVehicle();
        _baseBuilding.OnExitedBuilding();
        OnExitBuilding?.Invoke();
    }

    public bool AllowIndicator() => _baseBuilding.IsBuildingFinished;
}
