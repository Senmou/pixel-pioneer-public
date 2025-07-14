using UnityEngine;

public class DisplayItemList : MonoBehaviour
{
    [SerializeField] private Transform _container;
    [SerializeField] private Transform _slotContainer;
    [SerializeField] private ItemSlot _slotTemplate;

    private BaseRecipeSO _lastRecipeSO;

    private void Awake()
    {
        Hide();
        _slotTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        BuildingController.Instance.OnStateChanged += BuildingController_OnStateChanged;
        PowerConnectionController.Instance.OnStateChanged += PowerConnectionController_OnStateChanged;
    }

    private void PowerConnectionController_OnStateChanged(object sender, PowerConnectionController.State e)
    {

    }

    private void BuildingController_OnStateChanged(object sender, BuildingController.State e)
    {
        //if (e == BuildingController.State.None)
        //    Hide();
        //else if (_lastRecipeSO != null)
        //    UpdateUI(_lastRecipeSO);
    }

    public void UpdateUI(BaseRecipeSO recipe)
    {
        _lastRecipeSO = recipe;

        Show();

        foreach (Transform child in _slotContainer)
        {
            if (child.transform != _slotTemplate.transform)
                Destroy(child.gameObject);
        }

        foreach (var inputItem in recipe.InputItems)
        {
            var materialSlot = Instantiate(_slotTemplate, _slotContainer);
            materialSlot.gameObject.SetActive(true);

            Player.Instance.Inventory.HasItem(inputItem.Key, out int availableAmount);
            materialSlot.UpdateUI(inputItem.Key, $"{availableAmount}/{inputItem.Value}");
        }
    }

    public void Show()
    {
        _container.gameObject.SetActive(true);
    }

    public void Hide()
    {
        _container.gameObject.SetActive(false);
    }
}
