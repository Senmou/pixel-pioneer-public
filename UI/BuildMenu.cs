using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class BuildMenu : Menu<BuildMenu>
{
    [SerializeField] private PrefabSO _prefabs;
    [SerializeField] private BuildingMenuSlot _buildingSlotTemplate;
    [SerializeField] private Transform _buildingContainer;
    [SerializeField] private Transform _buildableContainer;
    [SerializeField] private GameObject _container;
    [SerializeField] private Image _previewImage;
    [SerializeField] private TextMeshProUGUI _description;
    [SerializeField] private Button _buildButton;
    [SerializeField] private DisplayItemList _materialItemList;
    [SerializeField] private Toggle _fastBuildToggle;
    [SerializeField] private Toggle _buildWithoutMaterialToggle;
    [SerializeField] private BuildingRecipeListSO _buildingRecipeList;

    private bool _buildWithoutSubmit;
    private BaseRecipeSO _selectedRecipe;
    private const string FAST_BUILD_TOGGLE_KEY = "FastBuildToggleKey";
    private const string BUILD_WITHOUT_MATERIAL_KEY = "BuildWithoutMaterialKey";

    private void OnEnable()
    {
        _buildWithoutSubmit = Helper.PlayerPrefs_GetBool(FAST_BUILD_TOGGLE_KEY, false);
        _fastBuildToggle.SetIsOnWithoutNotify(_buildWithoutSubmit);
        _fastBuildToggle.onValueChanged.AddListener(FastBuildToggle_OnValueChanged);

        if (Debug.isDebugBuild)
        {
            BuildingController.Instance.BuildWithoutMaterials = Helper.PlayerPrefs_GetBool(BUILD_WITHOUT_MATERIAL_KEY, false);
            _buildWithoutMaterialToggle.SetIsOnWithoutNotify(BuildingController.Instance.BuildWithoutMaterials);
        }
        else
        {
            BuildingController.Instance.BuildWithoutMaterials = false;
            _buildWithoutMaterialToggle.gameObject.SetActive(false);
        }
    }

    private void FastBuildToggle_OnValueChanged(bool state)
    {
        _buildWithoutSubmit = state;
        Helper.PlayerPrefs_SetBool(FAST_BUILD_TOGGLE_KEY, state);
    }

    private new void Awake()
    {
        base.Awake();
        _buildingSlotTemplate.gameObject.SetActive(false);
    }

    private void Start()
    {
        BuildingController.Instance.BuildWithoutMaterials = _buildWithoutMaterialToggle.isOn;

        _buildWithoutMaterialToggle.onValueChanged.AddListener(OnToggleValueChanged);

        UpdateUI();
    }

    private new void OnDestroy()
    {
        base.OnDestroy();
        _buildWithoutMaterialToggle.onValueChanged.RemoveAllListeners();
        _fastBuildToggle.onValueChanged.RemoveAllListeners();
    }

    private void OnToggleValueChanged(bool state)
    {
        Helper.PlayerPrefs_SetBool(BUILD_WITHOUT_MATERIAL_KEY, state);
        BuildingController.Instance.BuildWithoutMaterials = state;
    }

    private void UpdateUI()
    {
        foreach (Transform child in _buildingContainer)
        {
            if (child == _buildingSlotTemplate.transform)
                continue;

            Destroy(child.gameObject);
        }

        foreach (var buildingRecipe in _buildingRecipeList.buildingRecipes)
        {
            var slot = Instantiate(_buildingSlotTemplate, _buildingContainer);
            slot.gameObject.SetActive(true);
            slot.UpdateUI(buildingRecipe, () =>
            {
                _previewImage.sprite = buildingRecipe.sprite ? buildingRecipe.sprite : buildingRecipe.prefab.Sprite;
                _previewImage.color = Color.white;
                _description.text = buildingRecipe.Description;
                _buildButton.interactable = true;
                _selectedRecipe = buildingRecipe;
                _materialItemList.UpdateUI(buildingRecipe);

                if (_buildWithoutSubmit)
                    OnBuildButtonClicked();
            });
        }
    }

    public void OnBuildButtonClicked()
    {
        var buildingRecipe = _selectedRecipe as BuildingRecipeSO;

        Hide();

        if (buildingRecipe != null)
            BuildingController.Instance.PlaceBuilding(Instantiate(buildingRecipe.prefab).GetComponent<Placeable>());
    }

    public override void OnBackPressed()
    {
        Close();
    }

    public static void Show()
    {
        var isGameScene = SceneManager.GetActiveScene().name.Equals("Game");
        if (!isGameScene)
            return;

        Open();
        Instance._buildButton.interactable = false;
    }

    public static void Hide()
    {
        if (Instance != null && Instance.gameObject.activeSelf)
            Close();
    }
}
