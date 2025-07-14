using UnityEngine;

public class PlacementIndicator : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Color _legalPlacementColor;
    [SerializeField] private Color _illegalPlacementColor;

    public void Init(Sprite sprite)
    {
        gameObject.SetActive(true);
        _renderer.sprite = sprite;
    }

    public void UpdateColor(bool legalPlacement)
    {
        _renderer.color = legalPlacement ? _legalPlacementColor : _illegalPlacementColor;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
