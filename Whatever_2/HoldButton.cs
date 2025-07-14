using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

public class HoldButton : Button, IPointerDownHandler
{
    public event EventHandler OnPress;
    public event EventHandler OnRelease;

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        OnPress?.Invoke(this, EventArgs.Empty);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        OnRelease?.Invoke(this, EventArgs.Empty);
    }
}
