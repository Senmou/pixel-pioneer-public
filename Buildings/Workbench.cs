using UnityEngine;
using System;

public class Workbench : BaseProductionBuilding
{
    [Space(10)]
    [Header("Menu")]
    [SerializeField] private Transform _cameraAnchor;

    [Space(10)]
    [SerializeField] private AudioPlayer _craftingAudioPlayer;

    public bool IsCrafting { get; private set; }

    private bool _isHoldingCraftButton;

    private new void Update()
    {
        if (!IsBuildingFinished)
            return;

        if (!Interactor.IsInteracting(this))
            return;

        IsCrafting = false;

        if ((_isHoldingCraftButton || InputController.Instance.IsJumpPressed) && _currentCraftingRecipe != null)
        {
            if (!Inventory.HasAllInputItems(_currentCraftingRecipe))
            {
                _craftingAudioPlayer.StopSound();
                return;
            }

            IsCrafting = true;
            _craftingAudioPlayer.PlaySound();
            _currentRecipeProgress += Time.deltaTime;

            if (_currentRecipeProgress >= _currentCraftingRecipe.Duration)
            {
                _currentRecipeProgress = 0f;

                Inventory.RemoveItems(_currentCraftingRecipe);

                foreach (var outputItem in _currentCraftingRecipe.OutputItems)
                {
                    WorldItemController.Instance.OnItemSpawned?.Invoke(this, new WorldItemController.OnItemDroppedEventArgs { Item = outputItem.Key, amount = outputItem.Value, spawnSource = WorldItemController.ItemSpawnSource.CRAFTING });

                    Inventory.AddItem(outputItem.Key, amount: outputItem.Value, onFailed: () =>
                    {
                        for (int i = 0; i < outputItem.Value; i++)
                        {
                            WorldItemController.Instance.DropItem(dropPoint.position, outputItem.Key, WorldItemController.ItemSpawnSource.CRAFTING);
                        }
                    });
                    OnCraftFinish?.Invoke(this, new OnCraftFinishedEventArgs { outputItemSO = outputItem.Key });
                }
                RemoveRecipeFromQueue(_currentCraftingRecipe, 1);
            }
        }
        else
        {
            _craftingAudioPlayer.StopSound();

            _currentRecipeProgress -= 1.5f * Time.deltaTime;
            if (_currentRecipeProgress < 0f)
                _currentRecipeProgress = 0f;
        }
    }

    public void SetIsHoldingCraftButton(bool state)
    {
        _isHoldingCraftButton = state;
    }

    protected override void BaseBuilding_OnFinishedBuilding(object sender, EventArgs e)
    {
        Inventory.SetFilter((itemSO) => !itemSO.isLarge && (_craftingRecipes.IsInputItem(itemSO) || _craftingRecipes.IsOutputItem(itemSO)));
    }

    private void StartInteracting()
    {
        Player.Instance.PlayerController.FreezePlayer();
        WorkbenchMenu.Show(this);
    }

    private void StopInteracting()
    {
        _currentRecipeProgress = 0f;
        _craftingAudioPlayer.StopSound();
        Player.Instance.PlayerController.UnfreezePlayer();
        WorkbenchMenu.Hide();
    }

    protected override void Inventory_OnItemAdded(object sender, Inventory.OnItemCollectedEventArgs e)
    {
        if (IsBuildingFinished)
            return;

        base.Inventory_OnItemAdded(sender, e);
    }

    public override void Interact(KeyCode keyCode, Interactor.InteractionType interactionType)
    {
        if (!IsBuildingFinished)
            return;

        if (interactionType == Interactor.InteractionType.START)
        {
            StartInteracting();
        }
        else if (interactionType == Interactor.InteractionType.STOP)
        {
            StopInteracting();
        }
    }

    public override void ForceCancelInteraction()
    {
        StopInteracting();
    }
}
