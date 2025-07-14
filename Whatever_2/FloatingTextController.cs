using UnityEngine;
using TMPro;

public class FloatingTextController : MonoBehaviour
{
    public static FloatingTextController Instance { get; private set; }

    [SerializeField] private TextMeshPro _textPrefab;

    private float _lastSpawnTime;

    private void Awake()
    {
        Instance = this;
        _textPrefab.gameObject.SetActive(false);
    }

    public void SpawnText(string text, Vector3 position, bool ignoreTimeRestriction = false)
    {
        if (!ignoreTimeRestriction && Time.time < _lastSpawnTime + 0.25f)
            return;

        _lastSpawnTime = Time.time;

        var floatingText = Instantiate(_textPrefab, position.WithZ(_textPrefab.transform.position.z), Quaternion.identity);
        floatingText.text = text;
        floatingText.gameObject.SetActive(true);
    }
}
