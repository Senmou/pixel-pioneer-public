using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class SingleRecipeSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button _selectButton;
    [SerializeField] private Image _recipeIcon;
    [SerializeField] private TextMeshProUGUI _recipeNameText;
    [SerializeField] private MMF_Player _hoverFeedback;
    [SerializeField] private MMF_Player _clickFeedback;
    [SerializeField] private Color _defaultTextColor;
    [SerializeField] private Color _hoverTextColor;

    public void Init(BaseProductionBuilding productionBuilding, CraftingRecipeSO recipeSO)
    {
        _selectButton.onClick.RemoveAllListeners();
        _selectButton.onClick.AddListener(() =>
        {
            productionBuilding.SetCraftingRecipe(recipeSO);
            _clickFeedback.PlayFeedbacks();
        });

        _recipeIcon.sprite = recipeSO.sprite;
        _recipeNameText.text = $"{recipeSO.RecipeName}";

        if (productionBuilding.CurrentCraftingRecipe == recipeSO)
            _selectButton.Select();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoverFeedback.PlayFeedbacks();
        _recipeNameText.color = _hoverTextColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _recipeNameText.color = _defaultTextColor;
    }
}
