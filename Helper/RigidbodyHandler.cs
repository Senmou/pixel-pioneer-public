using UnityEngine;

public class RigidbodyHandler : MonoBehaviour
{
    //[SerializeField] private Rigidbody2D _body;

    //private Vector2 _velocity;
    //private float _angularVelocity;
    //private bool _shouldToggleDetectionMode;
    //private float _timer;

    //private void OnEnable()
    //{
    //    _body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    //    _shouldToggleDetectionMode = true;
    //}

    //private void Start()
    //{
    //    if (Player.Instance == null)
    //    {
    //        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;
    //    }
    //    else
    //    {
    //        Player.Instance.OnReachedMoveThreshold += Player_OnReachedMoveThreshold;
    //    }
    //}

    //private void Update()
    //{
    //    if (!_shouldToggleDetectionMode)
    //        return;

    //    _timer += Time.deltaTime;
    //    if (_timer > 3f)
    //    {
    //        _shouldToggleDetectionMode = false;
    //        _body.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
    //    }
    //}

    //private void PlayerSpawner_OnPlayerSpawned(object sender, System.EventArgs e)
    //{
    //    Player.Instance.OnReachedMoveThreshold += Player_OnReachedMoveThreshold;
    //}

    //private void OnDisable()
    //{
    //    Player.Instance.OnReachedMoveThreshold -= Player_OnReachedMoveThreshold;
    //}

    //private void Player_OnReachedMoveThreshold(object sender, Vector2 e)
    //{
    //    var distanceToPlayer = Vector2.Distance(Player.Instance.transform.position, _body.transform.position);
    //    if (distanceToPlayer > 25f && !_body.isKinematic)
    //    {
    //        _body.bodyType = RigidbodyType2D.Kinematic;

    //        _velocity = _body.velocity;
    //        _angularVelocity = _body.angularVelocity;

    //        _body.velocity = Vector2.zero;
    //        _body.angularVelocity = 0f;
    //    }
    //    else if (distanceToPlayer < 25f && _body.isKinematic)
    //    {
    //        _body.isKinematic = false;
    //        _body.velocity = _velocity;
    //        _body.angularVelocity = _angularVelocity;
    //    }
    //}
}
