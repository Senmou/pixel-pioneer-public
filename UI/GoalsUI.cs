using UnityEngine;

public class GoalsUI : MonoBehaviour
{
    public static GoalsUI Instance { get; private set; }

    [SerializeField] private GoalSlot _goalSlotTemplate;
    [SerializeField] private Transform _newGoalHint;
    [SerializeField] private Transform _container;
    [SerializeField] private GameObject _group;

    private void Awake()
    {
        Instance = this;
        _goalSlotTemplate.gameObject.SetActive(false);

        ToggleUI(false);
    }

    public void UpdateUI()
    {
        foreach (Transform t in _container)
        {
            if (t == _goalSlotTemplate.transform || t == _newGoalHint)
                continue;
            Destroy(t.gameObject);
        }

        var artifactGoals = GoalController.Instance.ArtifactGoalList;
        foreach (var artifactGoal in artifactGoals)
        {
            foreach (var target in artifactGoal.Target)
            {
                var slot = Instantiate(_goalSlotTemplate, _container);
                slot.gameObject.SetActive(true);
                slot.UpdateUI(artifactGoal.GetProgressText(target.Key), artifactGoal.Description);
            }
        }

        var itemGoals = GoalController.Instance.ItemGoalList;
        foreach (var itemGoal in itemGoals)
        {
            foreach (var target in itemGoal.Target)
            {
                var slot = Instantiate(_goalSlotTemplate, _container);
                slot.gameObject.SetActive(true);
                slot.UpdateUI(itemGoal.GetProgressText(target.Key), itemGoal.Description);
            }
        }

        var buldingGoals = GoalController.Instance.BuildingGoalList;
        foreach (var buildingGoal in buldingGoals)
        {
            foreach (var target in buildingGoal.Target)
            {
                var slot = Instantiate(_goalSlotTemplate, _container);
                slot.gameObject.SetActive(true);
                slot.UpdateUI(buildingGoal.GetProgressText(target.Key), buildingGoal.Description);
            }
        }

        _newGoalHint.SetAsLastSibling();
    }

    public void ToggleUI(bool state)
    {
        _group.SetActive(state);
    }

    public void ToggleUI()
    {
        _group.SetActive(!_group.activeSelf);
    }
}
