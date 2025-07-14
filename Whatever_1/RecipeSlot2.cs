using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class RecipeSlot2 : MonoBehaviour, ITooltip
{
    [SerializeField] private Image _icon;
    [SerializeField] private Button _addButton_1;
    [SerializeField] private Button _addButton_10;
    [SerializeField] private Button _subButton_1;
    [SerializeField] private Button _subButton_10;
    [SerializeField] private Toggle _infToggle;
    [SerializeField] private TextMeshProUGUI _queueAmountText;
    [SerializeField] private TextMeshProUGUI _infiniteUI;

    #region ITooltip
    public string TooltipTitle => _recipeSO.OutputItems?.Keys.FirstOrDefault()?.ItemName;
    public string TooltipDescription => _recipeSO.OutputItems?.Keys.FirstOrDefault()?.Description;
    #endregion

    public CraftingRecipeSO CraftingRecipeSO => _recipeSO;

    private CraftingRecipeSO _recipeSO;
    private BaseProductionBuilding _building;

    private void OnDestroy()
    {
        _building.OnRecipeQueueChanged -= Building_OnRecipeQueueChanged;
    }

    public void InitSlot(BaseProductionBuilding productionBuilding, CraftingRecipeSO recipeSO, Action onSlotSelected = null)
    {
        _building = productionBuilding;
        _building.OnRecipeQueueChanged += Building_OnRecipeQueueChanged;
        _recipeSO = recipeSO;

        var isInfinite = productionBuilding.IsRecipeInfinite(recipeSO);
        _infiniteUI.gameObject.SetActive(isInfinite);
        _infToggle.SetIsOnWithoutNotify(isInfinite);

        _infToggle.onValueChanged.RemoveAllListeners();
        _infToggle.onValueChanged.AddListener((state) =>
        {
            _infiniteUI.gameObject.SetActive(state);
            productionBuilding.SetCraftingRecipeInfinite(recipeSO, state);

            if (!state)
                productionBuilding.RemoveRecipeFromQueue(recipeSO);
            _queueAmountText.text = $"{productionBuilding.GetQueuedItemAmount(recipeSO)}x";
        });

        _addButton_1.onClick.RemoveAllListeners();
        _addButton_1.onClick.AddListener(() =>
        {
            productionBuilding.AddRecipeToQueue(recipeSO, 1);
            _queueAmountText.text = $"{productionBuilding.GetQueuedItemAmount(recipeSO)}x";
        });

        _addButton_10.onClick.RemoveAllListeners();
        _addButton_10.onClick.AddListener(() =>
        {
            productionBuilding.AddRecipeToQueue(recipeSO, 10);
            _queueAmountText.text = $"{productionBuilding.GetQueuedItemAmount(recipeSO)}x";
        });

        _subButton_1.onClick.RemoveAllListeners();
        _subButton_1.onClick.AddListener(() =>
        {
            productionBuilding.AddRecipeToQueue(recipeSO, -1);
            _queueAmountText.text = $"{productionBuilding.GetQueuedItemAmount(recipeSO)}x";
        });

        _subButton_10.onClick.RemoveAllListeners();
        _subButton_10.onClick.AddListener(() =>
        {
            productionBuilding.AddRecipeToQueue(recipeSO, -10);
            _queueAmountText.text = $"{productionBuilding.GetQueuedItemAmount(recipeSO)}x";
        });

        if (recipeSO.UseInputAsIcon)
            _icon.sprite = recipeSO.InputItems.Count > 0 ? recipeSO.InputItems.ElementAt(0).Key.sprite : recipeSO.sprite;
        else
            _icon.sprite = recipeSO.OutputItems.Count > 0 ? recipeSO.OutputItems.ElementAt(0).Key.sprite : recipeSO.sprite;

        _queueAmountText.text = $"{productionBuilding.GetQueuedItemAmount(recipeSO)}x";
    }

    private void Building_OnRecipeQueueChanged(object sender, EventArgs e)
    {
        _queueAmountText.text = $"{_building.GetQueuedItemAmount(_recipeSO)}x";
    }
}
