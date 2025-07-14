using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine;

public class PointerExit : MonoBehaviour, IPointerExitHandler
{
    [SerializeField] private UnityEvent _onPointerExit;

    public void OnPointerExit(PointerEventData eventData)
    {
        _onPointerExit?.Invoke();
    }
}
