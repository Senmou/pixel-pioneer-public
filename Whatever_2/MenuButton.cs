using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;
using UnityEngine.UI;
using UnityEngine;

public class MenuButton : Button
{
    [SerializeField] private MMF_Player OnPointerEnterFeedback;
    [SerializeField] private MMF_Player OnPointerExitFeedback;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (!interactable)
            return;

        base.OnPointerEnter(eventData);
        OnPointerExitFeedback?.StopFeedbacks();
        OnPointerEnterFeedback?.PlayFeedbacks();
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (!interactable)
            return;

        base.OnPointerExit(eventData);
        OnPointerEnterFeedback?.StopFeedbacks();
        OnPointerExitFeedback?.PlayFeedbacks();
    }

    public override void OnSelect(BaseEventData eventData)
    {

    }
}
