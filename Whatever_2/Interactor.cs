using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Interactor : MonoBehaviour
{
    public static Interactor Instance;

    [SerializeField] private Transform _interactionPoint;
    [SerializeField] private float _interactionRadius;
    [SerializeField] private LayerMask _interactableLayerMask;

    private readonly Collider2D[] _colliders = new Collider2D[3];

    private ContactFilter2D _contactFilter;
    private Dictionary<IInteractable, List<KeyCode>> _currentInteractionDict = new Dictionary<IInteractable, List<KeyCode>>();

    public enum InteractionType
    {
        START,
        STOP,
        BUTTON
    }

    private void Awake()
    {
        Instance = this;

        _contactFilter = new();
        _contactFilter.useTriggers = true;
        _contactFilter.useLayerMask = true;
        _contactFilter.layerMask = _interactableLayerMask;
    }

    private void Update()
    {
        if (Player.Instance.IsDead)
            return;

        for (int i = 0; i < _colliders.Length; i++)
        {
            _colliders[i] = null;
        }

        var hitCount = Physics2D.OverlapCircle(_interactionPoint.position, _interactionRadius, _contactFilter, _colliders);
        if (hitCount > 0)
        {
            var interactables = _colliders.Where(e => e != null).Select(e => e.GetComponentInParent<IInteractable>());

            if (interactables.Where(e => e != null).Count() == 0)
                return;

            var highestPriority = interactables.Where(e => e != null).Max(e => e.Priority);
            var highestPrioInteractables = interactables.Where(e => e.Priority == highestPriority);

            var nearestInteractable = highestPrioInteractables
                                            .OrderBy(e => Vector2.Distance(Player.Instance.transform.position.WithZ(0f), e.Transform.position.WithZ(0f)))
                                            .Where(e => e.InteractionKeys.Count > 0)
                                            .FirstOrDefault();

            var isAlreadyInteractingWithOtherObject = _currentInteractionDict.Count > 0 && !_currentInteractionDict.ContainsKey(nearestInteractable);
            if (isAlreadyInteractingWithOtherObject)
            {
                var interactionList = _currentInteractionDict.ToList();
                foreach (var pair in interactionList)
                {
                    if (InputController.Instance.GetPressedKey(pair.Key.InteractionKeys, out KeyCode pressKey))
                    {
                        StopInteraction(pair.Key, pressKey);
                    }
                }
                return;
            }

            if (nearestInteractable != null)
                InteractionController.Instance.ShowIndicator(nearestInteractable);

            if (nearestInteractable != null && InputController.Instance.GetPressedKey(nearestInteractable.InteractionKeys, out KeyCode pressedKey))
            {
                if (MenuManager.Instance.IsTopMenu(PauseMenu.Instance))
                    return;

                if (nearestInteractable.IsButton || (nearestInteractable.ButtonInteractionKeys != null && nearestInteractable.ButtonInteractionKeys.Contains(pressedKey)))
                {
                    if (MenuManager.Instance.OpenMenuCount > 0)
                        return;

                    nearestInteractable.Interact(pressedKey, InteractionType.BUTTON);
                    return;
                }

                _currentInteractionDict.TryGetValue(nearestInteractable, out List<KeyCode> keys);

                if (keys != null && keys.Contains(pressedKey))
                    StopInteraction(nearestInteractable, pressedKey);
                else
                    StartInteraction(nearestInteractable, pressedKey);
            }
        }
        else
        {
            InteractionController.Instance.HideIndicator();
        }
    }

    private void StartInteraction(IInteractable nearestInteractable, KeyCode pressedKey)
    {
        nearestInteractable.Interact(pressedKey, InteractionType.START);

        if (_currentInteractionDict.TryGetValue(nearestInteractable, out List<KeyCode> keys))
        {
            keys.Add(pressedKey);
            _currentInteractionDict[nearestInteractable] = keys;
        }
        else
        {
            _currentInteractionDict.Add(nearestInteractable, new List<KeyCode> { pressedKey });
        }
        InteractionController.Instance.HideIndicator();
    }

    private void StopInteraction(IInteractable interactable, KeyCode pressKey)
    {
        interactable.Interact(pressKey, InteractionType.STOP);
        _currentInteractionDict.TryGetValue(interactable, out List<KeyCode> keys);

        if (keys.Count > 1)
        {
            keys.Remove(pressKey);
            _currentInteractionDict[interactable] = keys;
        }
        else
        {
            _currentInteractionDict.Remove(interactable);
        }
        InteractionController.Instance.ShowIndicator(interactable);
    }

    public bool IsInteractingWithMineCart(out MineCart mineCart)
    {
        var cart = _currentInteractionDict.Keys.Select(e => e.Transform.GetComponent<MineCart>()).FirstOrDefault();
        if (cart != null)
        {
            mineCart = cart;
            return true;
        }
        mineCart = null;
        return false;
    }

    public static bool IsInteracting(IInteractable interactable)
    {
        if (Instance == null)
            return false;
        return Instance._currentInteractionDict.ContainsKey(interactable);
    }

    public static bool IsHoldingKey(List<KeyCode> keysToCheck, out KeyCode holdKey)
    {
        if (Instance == null)
        {
            holdKey = KeyCode.None;
            return false;
        }

        var isHoldingInteractionKey = InputController.Instance.GetHoldKey(keysToCheck, out KeyCode key);
        holdKey = key;
        return isHoldingInteractionKey;
    }

    public void StopInteraction(IInteractable interactable)
    {
        if (!_currentInteractionDict.ContainsKey(interactable))
            return;

        interactable.Interact(default, InteractionType.STOP);
        _currentInteractionDict.Remove(interactable);
    }

    public void StopAllInteractions()
    {
        foreach (var pair in _currentInteractionDict)
        {
            pair.Key.Interact(default, InteractionType.STOP);
        }
        _currentInteractionDict.Clear();
    }
}
