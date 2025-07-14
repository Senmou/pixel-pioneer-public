using UnityEngine.EventSystems;
using MoreMountains.Feedbacks;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class BuildingMenuSlot : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private Button _button;
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private MMF_Player _hoverFeedback;
    [SerializeField] private Image _image;

    public void UpdateUI(BuildingRecipeSO recipe, UnityAction action)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(action);

        var sprite = recipe.sprite;
        _image.sprite = sprite;

        _nameText.text = $"{recipe.BuildingName}";
    }

    public void UpdateUI(BuildableRecipeSO recipe, UnityAction action)
    {
        _button.onClick.RemoveAllListeners();
        _button.onClick.AddListener(action);

        var sprite = recipe.prefab.GetComponentInChildren<SpriteRenderer>().sprite;
        _image.sprite = sprite;

        _nameText.text = $"{recipe.buildableName}";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoverFeedback.PlayFeedbacks();
    }
}
