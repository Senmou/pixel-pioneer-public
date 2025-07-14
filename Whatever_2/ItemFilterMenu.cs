using UnityEditor;
using UnityEngine;

public class ItemFilterMenu : MonoBehaviour
{
    public static ItemFilterMenu Instance { get; private set; }

    [SerializeField] private MenuItemFilterSlot _slotTemplatePrefab;
    [SerializeField] private Transform _slotContainerUnselected;
    [SerializeField] private Transform _slotContainerSelected;
    [SerializeField] private PrefabSO _prefabSO;

    private void Awake()
    {
        Instance = this;
        _slotTemplatePrefab.gameObject.SetActive(false);
    }

    public void Init(IPipeNetworkEntity pipeNetworkEntity)
    {
        foreach (Transform child in _slotContainerUnselected)
        {
            if (child == _slotTemplatePrefab.transform)
                continue;

            var slot = child.GetComponent<MenuItemFilterSlot>();
            slot.Init((ItemSO item) =>
            {
                pipeNetworkEntity.AddItemToRequestList(item);
                slot.gameObject.SetActive(false);
                ActivateSlot(_slotContainerSelected, slot);
            });

            if (pipeNetworkEntity.RequestItemListContains(slot.Item))
            {
                child.gameObject.SetActive(false);
            }
        }

        foreach (Transform child in _slotContainerSelected)
        {
            var slot = child.GetComponent<MenuItemFilterSlot>();
            slot.Init((ItemSO item) =>
            {
                pipeNetworkEntity.RemoveItemFromRequestList(item);
                slot.gameObject.SetActive(false);
                ActivateSlot(_slotContainerUnselected, slot);
            });
            if (pipeNetworkEntity.RequestItemListContains(slot.Item))
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    private void ActivateSlot(Transform container, MenuItemFilterSlot slot)
    {
        foreach (Transform child in container)
        {
            if (child == _slotTemplatePrefab.transform)
                continue;

            var childSlot = child.GetComponent<MenuItemFilterSlot>();
            if (childSlot.Item == slot.Item)
                childSlot.gameObject.SetActive(true);
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Populate Slots with Items")]
    public void Editor_InitSlots()
    {
        foreach (var item in _prefabSO.items)
        {
            var slot = PrefabUtility.InstantiatePrefab(_slotTemplatePrefab, _slotContainerUnselected) as MenuItemFilterSlot;
            slot.gameObject.SetActive(true);
            slot.Editor_Init(item);
        }
    }
#endif
}
