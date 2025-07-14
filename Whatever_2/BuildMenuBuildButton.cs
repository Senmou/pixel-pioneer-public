using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;
using UnityEngine.UI;
using UnityEngine;

public class BuildMenuBuildButton : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private MMF_Player _hoverFeedback;
    [SerializeField] private Button _button;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_button.IsInteractable())
            _hoverFeedback.PlayFeedbacks();
    }
}
