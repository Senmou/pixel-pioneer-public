using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using QFSW.QC;

public class ConsoleCommands : MonoBehaviour
{
    public static ConsoleCommands Instance { get; private set; }

    public PrefabSO prefabSO;

    private void Awake()
    {
        Instance = this;
    }

    [Command(aliasOverride: "credits")]
    private static void Credits(int amount = 100000)
    {
        GlobalStats.Instance.AddCredits(amount, ignoreBonus: true);
    }

    [Command(aliasOverride: "dialog")]
    private static void Dialog(string text, int occurrence = 1)
    {
        var dialog = new DialogSO();
        for (int i = 0; i < occurrence; i++)
        {

        }
        DialogController.Instance.EnqueueDialog(dialog);
    }

    [Command(aliasOverride: "get")]
    private static void GetItem([ItemId] string itemId, int amount = 1)
    {
        var item = Instance.prefabSO.GetItemSOById(itemId);
        if (item != null)
        {
            var droppedItem = WorldItemController.Instance.DropItem(Helper.MousePos, item.Id);
            droppedItem.SetStackSize(amount);
        }
    }

    private bool _superman;
    [Command(aliasOverride: "superman")]
    public static void Superman(bool state = true)
    {
        Instance._superman = state;
        OxygenController.Instance.DEBUG_PreventOxygenUsage(state);
        Player.Instance.ToggleFlying(state);
        FindAnyObjectByType<LaserCannon>()?.IgnoreMiningDuration(state);
        Player.Instance.Collider.enabled = !state;
    }

    public static void ToggleSuperman()
    {
        Superman(!Instance._superman);
    }

    [Command(aliasOverride:"level")]
    private void ChangeLevel(int levelIndex)
    {
        GameManager.Instance.Continue(levelIndex);
    }
}

public struct ItemIdTag : IQcSuggestorTag
{

}

public sealed class ItemIdAttribute : SuggestorTagAttribute
{
    private readonly IQcSuggestorTag[] _tags = { new ItemIdTag() };

    public override IQcSuggestorTag[] GetSuggestorTags()
    {
        return _tags;
    }
}

public class ItemIdSuggestor : BasicCachedQcSuggestor<string>
{
    protected override bool CanProvideSuggestions(SuggestionContext context, SuggestorOptions options)
    {
        return context.HasTag<ItemIdTag>();
    }

    protected override IQcSuggestion ItemToSuggestion(string itemId)
    {
        return new RawSuggestion(itemId, true);
    }

    protected override IEnumerable<string> GetItems(SuggestionContext context, SuggestorOptions options)
    {
        var prefabSO = ConsoleCommands.Instance.prefabSO;
        return prefabSO.items.Select(e => e.Id).ToArray();
    }
}