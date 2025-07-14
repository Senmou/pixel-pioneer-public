using UnityEngine.EventSystems;
using UnityEngine;

public class SnapScrollViewButton : MonoBehaviour, IPointerClickHandler
{
    private SnapScrollRect _snapScrollRect;

    private void Awake()
    {
        _snapScrollRect = GetComponentInParent<SnapScrollRect>();

        if (_snapScrollRect == null)
            Debug.LogError("No SnapScrollRect in parents found!");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _snapScrollRect.SnapToChild(transform);
    }
}
