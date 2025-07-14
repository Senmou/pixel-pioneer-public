using UnityEngine;

public class InfoStationMenuSingularity : MonoBehaviour
{
    [SerializeField] private InfoStationSingularitySlot _slotTemplate;
    [SerializeField] private GameObject _slotContainer;

    private void Awake()
    {
        _slotTemplate.gameObject.SetActive(false);
    }

    public void Show()
    {
        foreach (Transform slot in _slotContainer.transform)
        {
            if (slot == _slotTemplate.transform)
                continue;
            Destroy(slot.gameObject);
        }

        var singularities = FindObjectsByType<Singularity>(FindObjectsSortMode.None);
        foreach (var singularity in singularities)
        {
            var slot = Instantiate(_slotTemplate, _slotContainer.transform);
            slot.UpdateUI(singularity);
            slot.gameObject.SetActive(true);
        }
    }
}
