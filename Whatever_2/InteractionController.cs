using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    public bool IsButton { get => false; }
    public int Priority { get => 0; }
    public string IndicatorTextOverride { get => string.Empty; }
    public List<KeyCode> InteractionKeys { get; }
    public List<KeyCode> ButtonInteractionKeys => null;
    public Vector3 IndicatorPosition { get; }
    public Transform Transform { get; }
    public void Interact(KeyCode keyCode, Interactor.InteractionType interactionType);
    public bool AllowIndicator();
}

public class InteractionController : MonoBehaviour
{
    public static InteractionController Instance;

    [SerializeField] private UI_InteractionIndicator _interactionIndicatorPrefab;
    [SerializeField] private ContactFilter2D _interactableContactFilter;

    private State _state;
    private UI_InteractionIndicator _spawnedIndicator;
    private IInteractable _lastInteractable;

    public enum State
    {
        NONE,
        LOCK_INTERACTION,
        RELEASE_INTERACTION,
        ENTER_BUILDING,
        EXIT_BUILDING,
        INTERACT_BUILDING
    }

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        if (Player.Instance != null && Player.Instance.IsDead)
        {
            HideIndicator();
            return;
        }

        if (_spawnedIndicator != null && _lastInteractable != null)
        {
            _spawnedIndicator.transform.position = _lastInteractable.IndicatorPosition;
        }
    }

    public void ShowIndicator(IInteractable interactable)
    {
        if (_lastInteractable == interactable) return;
        if (Interactor.IsInteracting(interactable)) return;
        if (!interactable.AllowIndicator()) return;

        _lastInteractable = interactable;

        if (_spawnedIndicator != null)
        {
            Destroy(_spawnedIndicator.gameObject);
        }

        _spawnedIndicator = Instantiate(_interactionIndicatorPrefab, interactable.IndicatorPosition, Quaternion.identity);

        string prompt = "";
        foreach (var keyCode in interactable.InteractionKeys)
        {
            prompt += keyCode.ToString() + " | ";
        }
        prompt = prompt.Remove(prompt.Length - 2);

        if (interactable.IndicatorTextOverride != string.Empty)
            prompt = interactable.IndicatorTextOverride;

        _spawnedIndicator.SetText(prompt);
    }

    public void HideIndicator()
    {
        if (_spawnedIndicator != null)
        {
            _lastInteractable = null;
            Destroy(_spawnedIndicator.gameObject);
        }
    }

    public void SetState(State state)
    {
        _state = state;

        switch (state)
        {
            case State.NONE:
                {
                    if (_lastInteractable != null)
                    {
                        //var trigger = _interactable.GetComponent<InteractionIndicatorTrigger>();
                        //if (trigger != null)
                        //    trigger.ShowIndicator();
                    }
                }
                break;
            case State.LOCK_INTERACTION:
                {
                    HideIndicator();
                }
                break;
            case State.RELEASE_INTERACTION:
                {
                    SetState(State.NONE);
                }
                break;
            case State.ENTER_BUILDING:
                break;
            case State.EXIT_BUILDING:
                break;
            case State.INTERACT_BUILDING:
                break;
        }
    }
}
