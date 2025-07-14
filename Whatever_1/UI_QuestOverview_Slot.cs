using static QuestController;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class UI_QuestOverview_Slot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _textTemplate;
    [SerializeField] private GameObject _textContainer;
    [SerializeField] private Button _rewardButton;

    public ItemQuest ItemQuest => _quest;

    private ItemQuest _quest;

    private void Awake()
    {
        _textTemplate.gameObject.SetActive(false);
    }

    public void Init(ItemQuest quest)
    {
        _quest = quest;
        _rewardButton.interactable = _quest.AllGoalsReached();

        _rewardButton.onClick.RemoveAllListeners();
        _rewardButton.onClick.AddListener(() => QuestController.Instance.FinalizeQuest(_quest));

        CreateTexts(quest);
    }

    public void UpdateUI()
    {
        foreach (Transform child in _textContainer.transform)
        {
            if (child == _textTemplate.transform)
                continue;

            Destroy(child.gameObject);
        }

        CreateTexts(_quest);

        _rewardButton.interactable = _quest.AllGoalsReached();
    }

    private void CreateTexts(ItemQuest quest)
    {
        foreach (var target in quest.Target)
        {
            var text = Instantiate(_textTemplate, _textContainer.transform);
            text.gameObject.SetActive(true);
            quest.Current.TryGetValue(target.Key, out int currentValue);
            text.text = $"{target.Key.ItemName} {currentValue}/{target.Value}";
        }
    }
}
