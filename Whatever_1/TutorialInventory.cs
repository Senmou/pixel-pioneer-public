using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TutorialInventory : BaseTutorial
{
    private bool _usedScrollWheel;

    [SerializeField] private TextMeshProUGUI _scrollWheelText;

    private void Awake()
    {
        InputController.Instance.OnScrollWheel += InputController_OnScrollWheel;
    }

    public override bool CancelCondition()
    {
        return _usedScrollWheel;
    }

    public override void OnFinish()
    {
        InputController.Instance.OnScrollWheel -= InputController_OnScrollWheel;
    }

    private void InputController_OnScrollWheel(object sender, InputController.InteractEventArgs e)
    {
        _usedScrollWheel = true;
        OnUsedScrollWheel();
    }

    private void OnUsedScrollWheel()
    {
        _scrollWheelText.color = Color.green;
    }
}
