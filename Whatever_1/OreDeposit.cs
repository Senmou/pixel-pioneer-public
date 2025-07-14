using UnityEngine;

public class OreDeposit : MonoBehaviour, ILaserInteractable, IPayload
{
    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private Collider2D _collider;
    [SerializeField] private SpriteRenderer _spriteRenderer;

    private float _hitTimer;
    private bool _isDestroyed;

    public ItemSO ItemSO => throw new System.NotImplementedException();

    public Sprite Sprite => _spriteRenderer.sprite;

    public GameObject GameObject => gameObject;

    public void OnHitByLaser()
    {
        if (_isDestroyed)
            return;

        _hitTimer += Time.deltaTime;

        if (_hitTimer > 1f)
        {
            _isDestroyed = true;

            _collider.gameObject.SetActive(true);
            _body.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void OnPayloadCollected(IPayloadContainer payloadContainer)
    {
        _body.bodyType = RigidbodyType2D.Kinematic;
        _body.linearVelocity = Vector2.zero;
        _body.angularVelocity = 0f;
        _collider.gameObject.SetActive(false);
        _spriteRenderer.maskInteraction = SpriteMaskInteraction.VisibleOutsideMask;
    }

    public void OnPayloadDiscarded()
    {
        _body.bodyType = RigidbodyType2D.Dynamic;
        _collider.gameObject.SetActive(true);
        _spriteRenderer.maskInteraction = SpriteMaskInteraction.None;
    }
}
