using UnityEngine;

public class ObjectDetectorMarker : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;

    private float _targetAlpha;
    private float _alphaLerpSpeed;

    private void Awake()
    {
        _targetAlpha = 1f;
        _alphaLerpSpeed = 1f;
    }

    private void Update()
    {
        if (Player.Instance == null)
            return;

        var playerPos = Player.Instance.transform.position;
        var dir = (transform.position.WithZ(0f) - playerPos.WithZ(0f)).normalized;
        transform.up = dir;

        var alpha = Mathf.MoveTowards(_renderer.color.a, _targetAlpha, _alphaLerpSpeed * Time.deltaTime);
        SetAlpha(alpha);
    }

    public void SetTargetAlpha(float targetAlpha, float lerpSpeed)
    {
        _alphaLerpSpeed = lerpSpeed;
        _targetAlpha = targetAlpha;
    }

    private void SetAlpha(float alpha)
    {
        _renderer.color = _renderer.color.WithA(alpha);
    }
}
