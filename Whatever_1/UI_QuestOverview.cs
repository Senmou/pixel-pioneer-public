using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class UI_QuestOverview : MonoBehaviour
{
    [SerializeField] private UI_QuestOverview_Slot _slotTemplate;
    [SerializeField] private GameObject _background;
    [SerializeField] private GameObject _slotContainer;
    [SerializeField] private GameObject _rewardsContainer;
    [SerializeField] private TextMeshProUGUI _rewardsText;

    private List<UI_QuestOverview_Slot> _slots = new();

    private void Awake()
    {
        _slotTemplate.gameObject.SetActive(false);

        SaveSystem.Instance.onDataPassed += OnDataPassed;

        ShowOrHide();
    }

    private void OnDataPassed()
    {
        foreach (var quest in QuestController.Instance.QuestList)
        {
            CreateSlot(quest);
        }
        ShowOrHide();
    }

    private void Start()
    {
        QuestController.Instance.OnQuestProgress += QuestController_OnQuestProgress;
    }

    private void QuestController_OnQuestProgress(object sender, QuestController.QuestEventArgs e)
    {
        if (e.isNewQuest)
            CreateSlot(e.quest);
        else if (e.isFinalized)
            DeleteSlot(e.quest);
        else
            UpdateSlot(e.quest);

        ShowOrHide();
    }

    private void CreateSlot(QuestController.ItemQuest quest)
    {
        var slot = Instantiate(_slotTemplate, _slotContainer.transform);
        slot.gameObject.SetActive(true);
        slot.Init(quest);

        _slots.Add(slot);
    }

    private void DeleteSlot(QuestController.ItemQuest quest)
    {
        var slots = _slotContainer.GetComponentsInChildren<UI_QuestOverview_Slot>(includeInactive: false);
        var slot = slots.Where(e => e.ItemQuest == quest).FirstOrDefault();
        if (slot != null)
        {
            Destroy(slot.gameObject);
        }

        _slots.Remove(slot);
    }

    private void UpdateSlot(QuestController.ItemQuest quest)
    {
        var slots = _slotContainer.GetComponentsInChildren<UI_QuestOverview_Slot>();
        var slot = slots.Where(e => e.ItemQuest == quest).FirstOrDefault();
        if (slot != null)
        {
            slot.UpdateUI();
        }
    }

    private void ShowOrHide()
    {
        if (_slots.Count == 0)
            Hide();
        else
            Show();
    }

    public void ShowRewards(UI_QuestOverview_Slot slot)
    {
        if (_rewardsContainer.activeSelf)
        {
            _rewardsContainer.SetActive(false);
            return;
        }

        _rewardsContainer.SetActive(true);

        var text = $"";
        if (slot.ItemQuest.RewardBuildings.Count > 0)
        {
            text += "Gebäude\n";
            foreach (var building in slot.ItemQuest.RewardBuildings)
            {
                text += $"- {building.BuildingName}\n";
            }
        }

        if (slot.ItemQuest.RewardRecipeUnlock.Count > 0)
        {
            text += "Rezepte\n";
            foreach (var recipe in slot.ItemQuest.RewardRecipeUnlock)
            {
                text += $"- {recipe.Recipe.RecipeName} ({recipe.Building})\n";
            }
        }

        _rewardsText.text = text;
    }

    private void Show()
    {
        _background.SetActive(true);
        _slotContainer.SetActive(true);
    }

    private void Hide()
    {
        _background.SetActive(false);
        _slotContainer.SetActive(false);
    }
}
