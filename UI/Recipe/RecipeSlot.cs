using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class RecipeSlot : MonoBehaviour, ITooltip, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image _icon;
    [SerializeField] private Image _background;
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _outputNameText;

    #region ITooltip
    public string TooltipTitle => _recipeSO.OutputItems?.Keys.FirstOrDefault()?.ItemName;
    public string TooltipDescription => _recipeSO.OutputItems?.Keys.FirstOrDefault()?.Description;
    #endregion

    public CraftingRecipeSO CraftingRecipeSO => _recipeSO;

    private CraftingRecipeSO _recipeSO;
    private bool _isSelected;

    private void OnDestroy()
    {
        RecipeMenu.Instance.OnSlotSelected -= RecipeMenu_OnSlotSelected;
    }

    public void InitSlot(BaseProductionBuilding productionBuilding, CraftingRecipeSO recipeSO, Action onSlotSelected = null)
    {
        RecipeMenu.Instance.OnSlotSelected += RecipeMenu_OnSlotSelected;

        _recipeSO = recipeSO;
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() =>
        {
            productionBuilding.SetCraftingRecipe(recipeSO);
            onSlotSelected?.Invoke();
        });

        if (recipeSO.UseInputAsIcon)
        {
            _icon.sprite = recipeSO.InputItems.Count > 0 ? recipeSO.InputItems.ElementAt(0).Key.sprite : recipeSO.sprite;
            //_outputNameText.text = recipeSO.InputItems.Count > 0 ? recipeSO.InputItems.ElementAt(0).Key.ItemName : recipeSO.RecipeName;
        }
        else
        {
            _icon.sprite = recipeSO.OutputItems.Count > 0 ? recipeSO.OutputItems.ElementAt(0).Key.sprite : recipeSO.sprite;
            //_outputNameText.text = recipeSO.OutputItems.Count > 0 ? recipeSO.OutputItems.ElementAt(0).Key.ItemName : recipeSO.RecipeName; ;
        }
    }

    private void RecipeMenu_OnSlotSelected(object sender, RecipeSlot e)
    {
        _isSelected = e == this;

        if (_isSelected)
            _background.color = _background.color.WithA(0.5f);
        else
            _background.color = _background.color.WithA(0.2f);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.localScale = Vector3.one * 1.05f;
        RecipeMenu.Instance.OnHoverOverSlot();
        _background.color = _background.color.WithA(0.5f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.localScale = Vector3.one;

        if (!_isSelected)
            _background.color = _background.color.WithA(0.2f);
    }
}
