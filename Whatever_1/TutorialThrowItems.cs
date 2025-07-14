using UnityEngine;
using TMPro;

public class TutorialThrowItems : BaseTutorial
{
    [SerializeField] private TextMeshProUGUI _rightClickText;
    [SerializeField] private TextMeshProUGUI _scrollWheelText;

    private bool _threwItems;
    private bool _usedScrollWheel;

    private void Awake()
    {
        Player.Instance.Inventory.OnItemCountChanged += Inventory_OnItemCountChanged;
        InputController.Instance.OnScrollWheel += InputController_OnScrollWheel;
    }

    public override void OnFinish()
    {
        InputController.Instance.OnScrollWheel -= InputController_OnScrollWheel;
        Player.Instance.Inventory.OnItemCountChanged -= Inventory_OnItemCountChanged;
    }

    public override bool CancelCondition()
    {
        return _usedScrollWheel && _threwItems;
    }

    private void InputController_OnScrollWheel(object sender, InputController.InteractEventArgs e)
    {
        OnUsedScrollWheel();
    }

    private void Inventory_OnItemCountChanged(object sender, System.EventArgs e)
    {
        OnThrewItems();
    }

    private void OnUsedScrollWheel()
    {
        _usedScrollWheel = true;
        _scrollWheelText.color = Color.green;
    }

    private void OnThrewItems()
    {
        _threwItems = true;
        _rightClickText.color = Color.green;
    }
}
