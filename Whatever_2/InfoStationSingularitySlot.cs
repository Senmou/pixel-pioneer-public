using UnityEngine;
using TMPro;

public class InfoStationSingularitySlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _sizeUI;
    [SerializeField] private TextMeshProUGUI _stabilityUI;
    [SerializeField] private TooltipItemCount _itemQuestUI;

    public void UpdateUI(Singularity singularity)
    {
        _sizeUI.text = $"{singularity.Size}m";
        _stabilityUI.text = $"{(100f * singularity.PlanetStabilityRateModifier).ToString("0.00")}%";
        _itemQuestUI.Init(singularity.ItemCountList);
    }
}
