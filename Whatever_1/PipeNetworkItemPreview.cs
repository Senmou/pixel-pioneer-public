using UnityEngine;

public class PipeNetworkItemPreview : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;

    public void UpdateUI(ItemSO itemSO)
    {
        _spriteRenderer.sprite = itemSO.sprite;
    }
}
