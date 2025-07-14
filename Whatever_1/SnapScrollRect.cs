using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using System;

public class SnapScrollRect : ScrollRect
{
    public event EventHandler<OnElementSelectedEventArgs> OnElementSelected;
    public class OnElementSelectedEventArgs : EventArgs
    {
        public int index;
    }

    [SerializeField] private AnimationCurve _snapCurve;
    [SerializeField] private float _snapTime;

    private int SelectedElementIndex
    {
        get
        {
            var pos = horizontalNormalizedPosition;
            var step = 1f / (ChildCount - 1);
            return Mathf.RoundToInt(pos / step);
        }
    }

    private int _dragDirection;
    private float _startDragPos;
    private float _endDragPos;
    private int _lastSelectedIndex = -1;

    private int ChildCount => content.childCount - 1; // -1 for slot template

    private void Update()
    {
        if (SelectedElementIndex != _lastSelectedIndex)
        {
            _lastSelectedIndex = SelectedElementIndex;
            OnElementSelected?.Invoke(this, new OnElementSelectedEventArgs { index = _lastSelectedIndex });
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        StopAllCoroutines();
        _startDragPos = horizontalNormalizedPosition;
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        _endDragPos = horizontalNormalizedPosition;
        Snap();
    }

    public override void OnScroll(PointerEventData data)
    {
        var pos = horizontalNormalizedPosition;
        var step = 1f / (ChildCount - 1);

        StopAllCoroutines();

        if (data.scrollDelta.y > 0f)
        {
            StartCoroutine(SnapCo(pos + step));
        }
        else
        {
            StartCoroutine(SnapCo(pos - step));
        }
    }

    private void Snap(int targetChildIndex = -1)
    {
        var pos = horizontalNormalizedPosition;
        var step = 1f / (ChildCount - 1);

        if (targetChildIndex == -1)
        {
            var nearestPos = Mathf.RoundToInt(pos / step) * step;
            _dragDirection = _startDragPos < _endDragPos ? 1 : -1;

            if (_startDragPos.IsApprox(_endDragPos))
                _dragDirection = 0;

            if (nearestPos.IsApprox(_startDragPos, step / 2f))
                nearestPos += _dragDirection * step;

            nearestPos = Mathf.Clamp01(nearestPos);

            StartCoroutine(SnapCo(nearestPos));
        }
        else
        {
            StartCoroutine(SnapCo(targetChildIndex * step));
        }
    }

    public void SnapToChild(Transform target)
    {
        Transform targetChild = null;
        foreach (Transform child in content)
        {
            if (child == target)
            {
                targetChild = child;
                break;
            }
        }

        if (targetChild == null)
        {
            Debug.LogWarning("Is not child of scrollView content", target);
            return;
        }

        Snap(targetChild.GetSiblingIndex() - 1); // -1 for slot template
    }

    public void SnapToSecondLastElement()
    {
        SnapToChild(content.GetChild(ChildCount-1));
    }

    private IEnumerator SnapCo(float targetPos)
    {
        var startPos = horizontalNormalizedPosition;
        var timer = 0f;
        while (timer < _snapTime)
        {
            timer += Time.deltaTime;
            horizontalNormalizedPosition = Mathf.Lerp(startPos, targetPos, _snapCurve.Evaluate(timer / _snapTime));
            yield return null;
        }
    }
}
