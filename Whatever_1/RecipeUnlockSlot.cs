using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class RecipeUnlockSlot : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Button _button;

    [Space(10)]
    [SerializeField] private Image _background;
    [SerializeField] private Color _unlockedColor;
    [SerializeField] private Color _unlockableColor;
    [SerializeField] private Color _tooExpensiveColor;

    public RecipeUnlockSO RecipeUnlockSO => _recipeUnlockSO;

    private RecipeUnlockSO _recipeUnlockSO;

    public void Init(RecipeUnlockSO recipeUnlockSO, Action<RecipeUnlockSlot> onButtonClick)
    {
        RecipeUnlockMenu.Instance.OnRecipeUnlocked += RecipeUnlockMenu_OnRecipeUnlocked;

        UpdateColor(recipeUnlockSO);

        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(() =>
        {
            onButtonClick?.Invoke(this);
            UpdateColor(recipeUnlockSO);
        });

        _recipeUnlockSO = recipeUnlockSO;

        _icon.sprite = recipeUnlockSO.Recipe.sprite;
        _nameText.text = recipeUnlockSO.Recipe.RecipeName;

        var item = recipeUnlockSO.Recipe.OutputItems.Keys.FirstOrDefault(e => e != null);
    }

    private void RecipeUnlockMenu_OnRecipeUnlocked(object sender, EventArgs e)
    {
        UpdateColor(_recipeUnlockSO);
    }

    private void UpdateColor(RecipeUnlockSO recipeUnlockSO)
    {
        if (RecipeUnlockController.Instance.IsUnlocked(recipeUnlockSO))
        {
            _background.color = _unlockedColor;
        }
        else
        {
            if (GlobalStats.Instance.Credits >= recipeUnlockSO.Recipe.unlockPrice)
                _background.color = _unlockableColor;
            else
                _background.color = _tooExpensiveColor;
        }
    }
}
