using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine;
using System;

public class TutorialMining : BaseTutorial
{
    [SerializeField] private SimpleSlider _slider;
    [SerializeField] private float _laserTimerMax;

    [Header("Item Goals")]
    [SerializeField] private ItemSO _woodItemSO;
    [SerializeField] private ItemSO _woodenPlankSO;
    [SerializeField] private ItemSO _stoneSO;
    [SerializeField] private ItemSO _stoneBrickSO;
    [SerializeField] private ItemSO _copperIngotSO;
    [SerializeField] private ItemSO _cableSO;

    [Header("Building Goals")]
    [SerializeField] private BuildingRecipeSO _workbenchSO;
    [SerializeField] private BuildingRecipeSO _furnaceSO;
    [SerializeField] private BuildingRecipeSO _liftSO;
    [SerializeField] private BuildingRecipeSO _coalGeneratorSO;

    [Space(10)]
    [Header("Localization")]
    [SerializeField] private LocalizedString _collectWoodString;
    [SerializeField] private LocalizedString _buildWorkbenchString;
    [SerializeField] private LocalizedString _mineStoneString;
    [SerializeField] private LocalizedString _craftStoneBricksString;
    [SerializeField] private LocalizedString _buildFurnaceString;
    [SerializeField] private LocalizedString _buildLiftString;
    [SerializeField] private LocalizedString _buildCoalGeneratorString;
    [SerializeField] private LocalizedString _craftCableString;

    private float _laserTimer;
    private bool _hasSubscribed;

    private void Awake()
    {
        _slider.SetValue(0f);
    }

    public override bool CancelCondition()
    {
        var laserCannon = FindFirstObjectByType<LaserCannon>();
        if (laserCannon == null)
        {
            _hasSubscribed = false;
            return false;
        }

        if (!_hasSubscribed)
        {
            _hasSubscribed = true;
            laserCannon.OnShootLaser += LaserCannon_OnShootLaser;
        }

        return _laserTimer >= _laserTimerMax;
    }

    private void LaserCannon_OnShootLaser(object sender, LaserCannon.OnShootLaserEventArgs e)
    {
        throw new NotImplementedException();
    }

    public override void OnFinish()
    {
        var laserCannon = FindFirstObjectByType<LaserCannon>();
        if (laserCannon == null)
            return;

        laserCannon.OnShootLaser -= LaserCannon_OnShootLaser;

        TutorialController.Instance.PlayStepCompleteFeedback();

        Helper.RepeatAction(3f, 0f, 1, () =>
        {
            GoalController.Instance.AddItemGoal(
                itemDict: new Dictionary<string, int> { { _woodItemSO.Id, 3 } },
                description: $"{_collectWoodString.GetLocalizedString()}",
                onGoalReached: (s, e) =>
                {
                    Helper.RepeatAction(2f, 0f, 1, () =>
                    {
                        Helper.RepeatAction(2f, 0f, 1, () => { TutorialController.Instance.ShowTutorial_BuildMenu(); });
                        GoalController.Instance.AddBuildingGoal(
                            buildingDict: new Dictionary<string, int> { { _workbenchSO.Id, 1 } },
                            description: $"{_buildWorkbenchString.GetLocalizedString()}",
                            onGoalReached: (s, e) =>
                            {
                                Helper.RepeatAction(2f, 0f, 1, () =>
                                {
                                    GoalController.Instance.AddItemGoal(
                                    itemDict: new Dictionary<string, int> { { _stoneSO.Id, 10 } },
                                    description: $"{_mineStoneString.GetLocalizedString()}",
                                    onGoalReached: (s, e) =>
                                    {

                                    });

                                    GoalController.Instance.AddItemGoal(
                                    itemDict: new Dictionary<string, int> { { _stoneBrickSO.Id, 5 } },
                                    description: $"{_craftStoneBricksString.GetLocalizedString()}",
                                    onGoalReached: (s, e) =>
                                    {

                                    });

                                    GoalController.Instance.AddBuildingGoal(
                                    buildingDict: new Dictionary<string, int> { { _furnaceSO.Id, 1 } },
                                    description: $"{_buildFurnaceString.GetLocalizedString()}",
                                    onGoalReached: (s, e) =>
                                    {
                                        GoalController.Instance.AddBuildingGoal(
                                        buildingDict: new Dictionary<string, int> { { _liftSO.Id, 1 } },
                                        description: $"{_buildLiftString.GetLocalizedString()}",
                                        onGoalReached: (s, e) =>
                                        {
                                            GoalController.Instance.AddBuildingGoal(
                                            buildingDict: new Dictionary<string, int> { { _coalGeneratorSO.Id, 1 } },
                                            description: $"{_buildCoalGeneratorString.GetLocalizedString()}",
                                            onGoalReached: (s, e) =>
                                            {
                                                GoalController.Instance.AddItemGoal(
                                                itemDict: new Dictionary<string, int> { { _cableSO.Id, 1 } },
                                                description: $"{_craftCableString.GetLocalizedString()}",
                                                onGoalReached: (s, e) =>
                                                {
                                                    TutorialController.Instance.ShowTutorial_Cable();
                                                });
                                            });
                                        });
                                    });
                                });
                            });
                    });
                });
        });
    }
}
