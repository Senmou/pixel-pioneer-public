using UnityEngine;
using TMPro;

public class DisplayItemName : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;

    public void UpdateUI(ItemSO itemSO)
    {
        if (itemSO == null)
            _text.text = "";
        else
            _text.text = itemSO.ItemName;
    }
}
