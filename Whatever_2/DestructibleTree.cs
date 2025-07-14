using UnityEngine;

public interface IDestructible
{
    void Destroy();
}

public class DestructibleTree : MonoBehaviour, IDestructible
{
    [SerializeField] private ItemSO _dropItemSO;

    public void Destroy()
    {
        WorldItemController.Instance.DropItem(transform.position, _dropItemSO);
        Destroy(gameObject);
    }
}
