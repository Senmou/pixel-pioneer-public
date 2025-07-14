using MoreMountains.Feedbacks;
using UnityEngine.UI;
using UnityEngine;
using Febucci.UI;
using TMPro;

public class InventoryLogPanel : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _amountText;
    [SerializeField] private MMF_Player _hideFeedback;
    [SerializeField] private MMF_Player _scaleFeedback;
    [SerializeField] private InventoryLog _inventoryLog;
    [SerializeField] private TextAnimator_TMP _newTextAnimator;

    private int _amount;
    private ItemSO _item;

    private int _updateCount;
    private bool _finished;
    private float _timer;
    private const float TIMER_MAX = 3.5f;

    private void OnEnable()
    {
        _updateCount = 0;
        _finished = false;
        _amount = 0;
        _timer = TIMER_MAX;

        _newTextAnimator.gameObject.SetActive(false);
        _newTextAnimator.SetVisibilityEntireText(false);
    }

    private void OnDisable()
    {
        _inventoryLog.RemoveFromActiveObjects(_item);
        _amount = 0;
        _item = null;

        _newTextAnimator.gameObject.SetActive(false);
    }

    private void Update()
    {
        _timer -= Time.unscaledDeltaTime;
        if (!_finished && _timer <= 0f)
        {
            _finished = true;
            _hideFeedback.PlayFeedbacks();
        }
    }

    public void UpdateUI(ItemSO item, int amount, bool isNewItem)
    {
        if (_updateCount > 0)
            _scaleFeedback.PlayFeedbacks();

        _updateCount++;
        _icon.sprite = item.sprite;
        _timer = TIMER_MAX;
        _item = item;
        _amount += amount;
        _amountText.text = $"{_amount}x";

        if (isNewItem)
        {
            _newTextAnimator.gameObject.SetActive(true);
            _newTextAnimator.SetText("<wave a=0.2><rainb>NEW");
            _newTextAnimator.SetVisibilityEntireText(true);
        }
    }
}
