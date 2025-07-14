using UnityEngine;

[RequireComponent(typeof(InventoryMenu))]
public class PlayerInventoryMenu : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<InventoryMenu>().Init(Player.Instance.Inventory);
    }
}
