using UnityEngine;

public class Cable : ActionItem
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Collider2D _trigger;

    private bool _isSelected;
    private bool _startedPlacing;

    private void Update()
    {
        if (!_isSelected)
            return;

        if (!_startedPlacing)
        {
            StartPlacingCable();
        }
    }

    private void StartPlacingCable()
    {
        TooltipHandler.PreventTooltip = true;

        _startedPlacing = true;
        PowerConnectionController.Instance.OnSetPowerLineEndPosition += PowerConnectionController_OnSetPowerLineEndPosition;
        PowerConnectionController.Instance.StartPlacingPowerConnection();
    }

    private void CancelPlacingCable()
    {
        TooltipHandler.PreventTooltip = false;

        _startedPlacing = false;
        PowerConnectionController.Instance.OnSetPowerLineEndPosition -= PowerConnectionController_OnSetPowerLineEndPosition;
        PowerConnectionController.Instance.CancelConnection();
    }

    public override void OnItemSelected()
    {
        base.OnItemSelected();

        transform.parent = Player.Instance.transform;
        transform.localPosition = new Vector3(0f, 0.8f);
        _collider.enabled = false;
        _trigger.enabled = false;
        _body.bodyType = RigidbodyType2D.Kinematic;
        _spriteRenderer.enabled = true;

        _isSelected = true;
    }

    public override void OnItemDeselected()
    {
        if (!_isSelected)
            return;

        TooltipHandler.PreventTooltip = false;

        base.OnItemDeselected();
        Destroy(gameObject);

        _isSelected = false;

        CancelPlacingCable();
    }

    private void PowerConnectionController_OnSetPowerLineEndPosition(object sender, PowerConnectionController.OnPowerLineEventArgs e)
    {
        TooltipHandler.PreventTooltip = false;

        PowerConnectionController.Instance.OnSetPowerLineEndPosition -= PowerConnectionController_OnSetPowerLineEndPosition;

        _startedPlacing = false;
        Player.Instance.Inventory.RemoveItem(ItemSO);
    }
}
