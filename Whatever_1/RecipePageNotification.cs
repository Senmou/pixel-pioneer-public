using UnityEngine.Localization;
using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using TMPro;

public class RecipePageNotification : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _descriptionText;

    [Header("Recipe")]
    [SerializeField] private GameObject _recipeContainer;
    [SerializeField] private TextMeshProUGUI _recipeText;
    [SerializeField] private Image _recipeIcon;

    [Header("Localization")]
    [SerializeField] private LocalizedString _titleString;
    [SerializeField] private LocalizedString _findMorePagesString;
    [SerializeField] private LocalizedString _unlockedRecipeString;

    public void UpdateUI(CraftingRecipeSO unlockedRecipe, int pageCount, int pageCountThreshold)
    {
        _titleText.text = $"{_titleString.GetLocalizedString()} {pageCount}/{pageCountThreshold}";

        if (unlockedRecipe == null)
        {
            _recipeContainer.SetActive(false);
            _descriptionText.text = $"{_findMorePagesString.GetLocalizedString()}";
        }
        else
        {
            _recipeContainer.SetActive(true);
            _recipeIcon.sprite = unlockedRecipe.sprite;
            _recipeText.text = unlockedRecipe.OutputItems.ElementAt(0).Key.ItemName.ToString();
            _descriptionText.text = $"{_unlockedRecipeString.GetLocalizedString()}";
        }
    }
}
