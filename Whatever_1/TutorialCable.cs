using UnityEngine;
using TMPro;

public class TutorialCable : BaseTutorial
{
    [SerializeField] private TextMeshProUGUI _text;

    private bool _connectedBuildings;

    private void Start()
    {
        PowerConnectionController.Instance.OnSetPowerLineEndPosition += PowerConnectionController_OnSetPowerLineEndPosition;
    }

    public override void OnFinish()
    {
        PowerConnectionController.Instance.OnSetPowerLineEndPosition -= PowerConnectionController_OnSetPowerLineEndPosition;
    }

    private void PowerConnectionController_OnSetPowerLineEndPosition(object sender, PowerConnectionController.OnPowerLineEventArgs e)
    {
        _connectedBuildings = true;
        _text.color = Color.green;
    }

    public override bool CancelCondition()
    {
        return _connectedBuildings;
    }
}
