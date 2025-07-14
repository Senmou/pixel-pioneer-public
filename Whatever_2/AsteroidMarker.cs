using UnityEngine;
using TMPro;

public class AsteroidMarker : MonoBehaviour
{
    [SerializeField] private Asteroid _asteroid;
    [SerializeField] private TextMeshProUGUI _distanceText;
    [SerializeField] private TextMeshProUGUI _healthText;

    private void Update()
    {
        var screenPoint = Camera.main.WorldToScreenPoint(_asteroid.transform.position);
        var clampPosX = Mathf.Clamp(screenPoint.x, 100f, Screen.width - 100f);
        transform.position = transform.position.WithX(clampPosX);

        _distanceText.text = $"{(int)_asteroid.Distance}m";
        _healthText.text = $"{_asteroid.Health}";
    }
}
