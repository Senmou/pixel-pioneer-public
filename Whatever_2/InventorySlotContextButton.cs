using UnityEngine.EventSystems;
using UnityEngine;
using System;

public class InventorySlotContextButton : MonoBehaviour, IPointerDownHandler
{
    private Action _onClick;

    public void Init(bool show, Action onClick = null)
    {
        _onClick = onClick;

        if (show)
            Show();
        else
            Hide();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _onClick?.Invoke();
    }

    private void Show() => gameObject.SetActive(true);

    private void Hide() => gameObject.SetActive(false);
}
