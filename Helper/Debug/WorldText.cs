using UnityEngine;
using TMPro;

public class WorldText : MonoBehaviour
{
    public static WorldText Instance { get; private set; }

    [SerializeField] private TextMeshPro _text;

    private void Awake()
    {
        Instance = this;
    }

    public void SetText(string text, Vector3 position)
    {
        gameObject.SetActive(true);
        _text.text = text;
        transform.position = position.WithZ(-10f);
    }

    public void SetText(object text, Vector3 position)
    {
        gameObject.SetActive(true);
        _text.text = text.ToString();
        transform.position = position.WithZ(-10f);
    }
}
