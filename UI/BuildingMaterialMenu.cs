using MoreMountains.Feedbacks;
using UnityEngine;
using System;

public class BuildingMaterialMenu : MonoBehaviour
{
    [SerializeField] private Transform _materialSlotContainer;
    [SerializeField] private BuildingMaterialSlot _materialSlotTemplate;
    [SerializeField] private MMF_Player _hideFeedback;

    private BaseBuilding _building;

    private void Awake()
    {
        _materialSlotTemplate.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        _building.OnFinishedBuilding -= Building_OnFinishedBuilding;
        _building.OnAddedBuildingMaterial -= Building_OnAddedBuildingMaterial;
    }

    public void Show(BaseBuilding baseBuilding)
    {
        _building = baseBuilding;
        _building.OnFinishedBuilding += Building_OnFinishedBuilding;
        _building.OnAddedBuildingMaterial += Building_OnAddedBuildingMaterial;

        UpdateUI(baseBuilding.BuildingRecipe, baseBuilding.Inventory);
    }

    public void Hide()
    {
        foreach (Transform item in _materialSlotContainer)
        {
            var slot = item.GetComponent<BuildingMaterialSlot>();
            slot.Hide();
        }

        _hideFeedback.PlayFeedbacks();
    }

    private void Building_OnAddedBuildingMaterial(object sender, EventArgs e)
    {
        UpdateUI(_building.BuildingRecipe, _building.Inventory);
    }

    private void Building_OnFinishedBuilding(object sender, EventArgs e)
    {
        Destroy(gameObject);
    }

    private void UpdateUI(BuildingRecipeSO buildingRecipeSO, Inventory inventory)
    {
        foreach (Transform child in _materialSlotContainer)
        {
            if (child.transform != _materialSlotTemplate.transform)
                Destroy(child.gameObject);
        }

        foreach (var inputItem in buildingRecipeSO.InputItems)
        {
            var materialSlot = Instantiate(_materialSlotTemplate, _materialSlotContainer);
            materialSlot.gameObject.SetActive(true);

            inventory.HasItem(inputItem.Key, out int currentAmount);

            materialSlot.Init(inputItem.Key, currentAmount, inputItem.Value);
        }
    }
}
