using UnityEngine;
using System;

public class BackgroundShadowMaskHandler : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private SpriteMask _mask;
    [SerializeField] private float _shadowDistance;
    [SerializeField] private bool _isPlayerShadow;

    private bool _isSubscribed;

    private void Start()
    {
        if (!_isSubscribed && Player.Instance != null)
        {
            Player.Instance.OnEquippedLaserCannon += Player_OnLaserCannonEquipped;

            var laserCannon = FindAnyObjectByType<LaserCannon>();
            if (laserCannon != null)
            {
                LaserCannon.Instance.OnShootLaser += MiningLaser_OnShootLaser;
                //Debug.Log($"Subscribed (Start) OnShootLaser: {transform.parent.name}");
            }
        }
    }

    private void OnDestroy()
    {
        if (LaserCannon.Instance != null)
        {
            LaserCannon.Instance.OnShootLaser -= MiningLaser_OnShootLaser;
            //Debug.Log($"Unsubscribe OnShootLaser: {transform.parent.name}");
        }
        PlayerSpawner.Instance.OnPlayerSpawned -= PlayerSpawner_OnPlayerSpawned;
        Player.Instance.OnEquippedLaserCannon -= Player_OnLaserCannonEquipped;
    }

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        if (!_isSubscribed)
            Player.Instance.OnEquippedLaserCannon += Player_OnLaserCannonEquipped;
    }

    private void Player_OnLaserCannonEquipped(object sender, bool equippedLaserCannon)
    {
        _isSubscribed = true;

        if (equippedLaserCannon)
        {
            LaserCannon.Instance.OnShootLaser += MiningLaser_OnShootLaser;
            //Debug.Log($"Subscribed (Equipped) OnShootLaser: {transform.parent.name}");
        }
        else
        {
            LaserCannon.Instance.OnShootLaser -= MiningLaser_OnShootLaser;
            //Debug.Log($"Unsubscribed (Equipped) OnShootLaser: {transform.parent.name}");
        }
    }

    private void MiningLaser_OnShootLaser(object sender, LaserCannon.OnShootLaserEventArgs e)
    {
        var isFlipped = transform.localScale.x < 0f;
        var isRotated = transform.rotation != Quaternion.identity;
        var shadowPos = (transform.position - e.laserPos).normalized * _shadowDistance;

        if (isFlipped)
        {
            if (isRotated)
            {
                transform.localRotation = Quaternion.identity;
                transform.localScale = transform.lossyScale.WithX(1f);
                transform.localPosition = transform.worldToLocalMatrix * shadowPos;
            }
            else
            {
                transform.localScale = transform.lossyScale.WithX(1f);
                transform.localPosition = transform.worldToLocalMatrix * shadowPos;
            }
        }
        else
        {
            if (isRotated)
            {
                transform.rotation = Quaternion.identity;
                transform.localPosition = transform.worldToLocalMatrix * shadowPos;
            }
            else
            {
                transform.localPosition = transform.worldToLocalMatrix * shadowPos;
            }
        }

        transform.rotation = _spriteRenderer.transform.rotation;
    }

    private void Update()
    {
        _mask.sprite = _spriteRenderer.sprite;
        transform.localScale = _mask.transform.localScale.WithX(_spriteRenderer.flipX ? -1f : 1f);
        transform.rotation = _spriteRenderer.transform.rotation;
    }
}
