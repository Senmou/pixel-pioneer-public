using UnityEngine.UI;
using UnityEngine;
using System.Linq;
using System;
using TMPro;

public class RecipeUnlockMenu : Menu<RecipeUnlockMenu>
{
    public event EventHandler OnRecipeUnlocked;

    [SerializeField] private TextMeshProUGUI _descriptionText;
    [SerializeField] private GameObject _descriptionContainer;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private DisplayRecipe _displayRecipe;
    [SerializeField] private GameObject _displayRecipeContainer;
    [SerializeField] private Button _unlockButton;

    [Header("Crafting Recipes")]
    [SerializeField] private GameObject _slotContainer;
    [SerializeField] private RecipeUnlockSlot _slotTemplate;
    [SerializeField] private RecipeUnlockListSO _recipeUnlockListConstructor;

    [Space(10)]
    [Header("Equipment Crafting Recipes")]
    [SerializeField] private GameObject _slotContainerEquipment;
    [SerializeField] private RecipeUnlockListSO _recipeUnlockListWorkshop;

    private RecipeUnlockSlot _selectedRecipeSlot;

    private new void Awake()
    {
        base.Awake();
        _slotTemplate.gameObject.SetActive(false);
    }

    public static void Show()
    {
        Open();
        Instance.Init();
    }

    public static void Hide()
    {
        Close();
    }

    private void Init()
    {
        _priceText.gameObject.SetActive(false);
        _displayRecipeContainer.gameObject.SetActive(false);
        _descriptionContainer.gameObject.SetActive(false);
        _unlockButton.gameObject.SetActive(false);

        _unlockButton.interactable = false;
        _unlockButton.onClick.RemoveAllListeners();
        _unlockButton.onClick.AddListener(OnUnlockButtonClicked);

        var craftingRecipeListConstructor = _recipeUnlockListConstructor.recipes;
        foreach (var recipeUnlockSO in craftingRecipeListConstructor)
        {
            if (recipeUnlockSO == null)
            {
                Debug.LogWarning("Constructor UnlockList contains null");
                continue;
            }

            var slot = Instantiate(_slotTemplate, _slotContainer.transform);
            slot.Init(recipeUnlockSO, OnRecipeButtonClicked);
            slot.gameObject.SetActive(true);
        }

        var craftingRecipeListWorkshop = _recipeUnlockListWorkshop.recipes;
        foreach (var recipeUnlockSO in craftingRecipeListWorkshop)
        {
            if (recipeUnlockSO == null)
            {
                Debug.LogWarning("Workshop UnlockList contains null");
                continue;
            }

            var slot = Instantiate(_slotTemplate, _slotContainerEquipment.transform);
            slot.Init(recipeUnlockSO, OnRecipeButtonClicked);
            slot.gameObject.SetActive(true);
        }
    }

    private void OnRecipeButtonClicked(RecipeUnlockSlot selectedSlot)
    {
        _priceText.gameObject.SetActive(true);
        _displayRecipeContainer.gameObject.SetActive(true);
        _descriptionContainer.gameObject.SetActive(true);
        _unlockButton.gameObject.SetActive(true);

        _selectedRecipeSlot = selectedSlot;

        _priceText.text = $"{selectedSlot.RecipeUnlockSO.Recipe.unlockPrice} Credits";
        _displayRecipe.UpdateCurrentRecipeUI(selectedSlot.RecipeUnlockSO.Recipe, null);

        var item = selectedSlot.RecipeUnlockSO.Recipe.OutputItems.Keys.FirstOrDefault(e => e != null);
        if (item != null)
        {
            _descriptionText.text = item.Description;
        }

        _unlockButton.interactable = !RecipeUnlockController.Instance.IsUnlocked(selectedSlot.RecipeUnlockSO) && GlobalStats.Instance.Credits >= selectedSlot.RecipeUnlockSO.Recipe.unlockPrice;
    }

    private void OnUnlockButtonClicked()
    {
        if (GlobalStats.Instance.Credits >= _selectedRecipeSlot.RecipeUnlockSO.Recipe.unlockPrice)
        {
            GlobalStats.Instance.SubCredits(_selectedRecipeSlot.RecipeUnlockSO.Recipe.unlockPrice);
            RecipeUnlockController.Instance.UnlockRecipe(_selectedRecipeSlot.RecipeUnlockSO);

            _unlockButton.interactable = false;

            OnRecipeUnlocked?.Invoke(this, EventArgs.Empty);
        }
    }
}
