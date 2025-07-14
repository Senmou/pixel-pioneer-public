
public class ActionItem : WorldItem
{
    public bool IsEquipped { get; private set; }
    public ActionItemSO ActionItemSO => ItemSO as ActionItemSO;

    public virtual void OnItemSelected()
    {
        IsEquipped = true;
        Destroy(_body);
    }
    public virtual void OnItemDeselected()
    {
        IsEquipped = false;
    }

    public virtual void OnInventorySlotContextButtonClicked()
    {

    }

    public override bool ShouldBeSaved() => !IsEquipped;
}
