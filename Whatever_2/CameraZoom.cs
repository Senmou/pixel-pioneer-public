using UnityEngine;
using System;
using TMPro;

public class CameraZoom : MonoBehaviour
{
    public event EventHandler<float> OnZoomChanged;

    [SerializeField] private float _minOrthoSize = 15f;
    [SerializeField] private float _maxOrthoSize = 22f;
    [SerializeField] private TextMeshProUGUI _zoomText;

    private float _relativeZoom = 1f;

    public void ChangeZoom(float relativeChange)
    {
        _relativeZoom += relativeChange;
        _relativeZoom = Mathf.Clamp(_relativeZoom, 1f, 2f);
        var orthoSize = Helper.Remap(1f, 2f, _minOrthoSize, _maxOrthoSize, _relativeZoom);

        _zoomText.text = $"{(int)(100f * _relativeZoom)}%";

        OnZoomChanged?.Invoke(this, orthoSize);
    }
}
