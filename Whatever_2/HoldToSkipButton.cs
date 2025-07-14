using UnityEngine;
using System;

public class HoldToSkipButton : MonoBehaviour
{
    public event EventHandler OnSkip;

    [SerializeField] private SimpleSlider _slider;

    private void Update()
    {
        if (InputController.Instance.IsPressed_Escape)
        {
            var durationRatio = InputController.Instance.HoldEscapeButtonDuration;

            _slider.SetValue(durationRatio);

            if (durationRatio >= 1f)
            {
                OnSkip?.Invoke(this, EventArgs.Empty);
            }
        }
        else
        {
            _slider.SetValue(0f);
        }
    }
}
