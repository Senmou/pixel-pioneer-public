using System.Collections;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField] private AsteroidSize _asteroidSize;
    [SerializeField] private MiningStencil _asteroidStencil;
    [SerializeField] private LayerMask _terrainLayerMask;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Rigidbody2D _body;
    [SerializeField] private float _velocity;
    [SerializeField] private TrailRenderer[] _trails;
    [SerializeField] private ParticleSystemRenderer _particles;
    [SerializeField] private ItemLootDropTableSO _itemLDT;

    public AsteroidSize Size => _asteroidSize;

    private float _raycastTimer;
    private const float RAYCAST_TIMER_MAX = 0.5f;
    private float _distance;
    private Vector3 _direction;
    private Vector3 _hitPoint;
    private RaycastHit2D _hit;
    private AsteroidGameSystem _asteroidGameSystem;
    private int _health;
    private bool _isDestroyed;

    public float Distance => _distance;
    public int Health => _health;

    public enum AsteroidSize
    {
        SMALL,
        MEDIUM,
        LARGE
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(_hitPoint, 0.3f);
    }

    private void Awake()
    {
        _health = _asteroidSize == AsteroidSize.SMALL ? 10 : _asteroidSize == AsteroidSize.MEDIUM ? 50 : 300;
        _spriteRenderer.transform.Rotate(new Vector3(0f, 0f, 360f * Random.value));
    }

    //private void Start()
    //{
    //    _hit = Physics2D.Raycast(transform.position, _direction, distance: Helper.LastLevelDataSO.LevelData.Height * 2.5f, _terrainLayerMask);
    //    if (!_hit)
    //        DestroyAsteroid();
    //}

    private void Update()
    {
        if (_isDestroyed)
            return;

        if (_raycastTimer > RAYCAST_TIMER_MAX)
        {
            _raycastTimer = 0f;
            //_hit = Physics2D.Raycast(transform.position, _direction, distance: Helper.LastLevelDataSO.LevelData.Height * 2.5f, _terrainLayerMask);
            if (!_hit)
            {
                DestroyAsteroid();
                return;
            }
        }

        _raycastTimer += Time.deltaTime;

        _hitPoint = _hit.point;
        _distance = Vector3.Distance(transform.position, _hit.point);
        var reachedTargetPos = _distance < _velocity * Time.deltaTime;

        if (transform.position.y < _hit.point.y)
            reachedTargetPos = true;

        if (reachedTargetPos)
        {
            _asteroidStencil.gameObject.SetActive(true);
            _body.MovePosition(PixelGrid.CellPos(_hit.point));
            _asteroidGameSystem.PlayImpactFeedback(transform.position, _asteroidSize);

            PlanetStabilityController.Instance.IncTimerRelative(0.5f);

            DropRandomLoot();
            DestroyAsteroid();
        }
    }

    public void Init(Vector3 direction, AsteroidGameSystem asteroidGameSystem)
    {
        _direction = direction;
        _asteroidGameSystem = asteroidGameSystem;

        _body.linearVelocity = _velocity * direction;
    }

    public void TakeDamage(int amount)
    {
        _health -= amount;
        if (_health <= 0)
        {
            DropRandomLoot();
            DestroyAsteroid();
        }
    }

    private void DropRandomLoot()
    {
        var lootDropItem = _itemLDT.lootDropTable.PickLootDropItem();
        var amount = lootDropItem.GetRandomAmount();

        for (int i = 0; i < amount; i++)
        {
            WorldItemController.Instance.DropItem(transform.position, lootDropItem.item);
        }
    }

    private void DestroyAsteroid()
    {
        _isDestroyed = true;
        _spriteRenderer.gameObject.SetActive(false);
        _body.linearVelocity = Vector3.zero;
        StartCoroutine(FadeOutTrailsCo());
        StartCoroutine(DestroyCo());
    }

    private IEnumerator DestroyCo()
    {
        yield return new WaitForSeconds(0.75f);
        _asteroidGameSystem.RemoveAsteroid(this);
        Destroy(gameObject);
    }

    private IEnumerator FadeOutTrailsCo()
    {
        float startAlpha = 1f;
        float timer = 0f;
        float timerMax = 0.5f;

        MaterialPropertyBlock block = new MaterialPropertyBlock();
        while (timer < timerMax)
        {
            timer += Time.deltaTime;
            var alpha = Mathf.Clamp01(startAlpha - timer / timerMax);
            foreach (var trail in _trails)
            {
                trail.GetPropertyBlock(block);
                block.SetFloat("_Alpha", alpha);
                trail.SetPropertyBlock(block);
            }

            _particles.GetPropertyBlock(block);
            block.SetFloat("_Alpha", alpha);
            _particles.SetPropertyBlock(block);
            yield return null;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        var building = other.GetComponent<BaseBuilding>();
        var placeable = other.GetComponent<Placeable>();
        if (building == null && placeable != null)
        {
            BuildingController.Instance.DestroyPlaceable(placeable);
        }
    }
}
