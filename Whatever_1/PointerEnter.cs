using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine;

public class PointerEnter : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private UnityEvent _onPointerEnter;

    public void OnPointerEnter(PointerEventData eventData)
    {
        _onPointerEnter?.Invoke();
    }
}
