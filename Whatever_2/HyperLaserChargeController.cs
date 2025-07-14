using MoreMountains.Tools;
using System.Collections;
using UnityEngine;

public class HyperLaserChargeController : MonoBehaviour, MMEventListener<LaserCannonBlockDestructionEvent>
{
    public static HyperLaserChargeController Instance { get; private set; }

    [SerializeField] private GameObject _particlePrefab;

    public int HyperLaserCharge { get; private set; }

    public const int HYPER_LASER_CHARGE_MAX = 100;
    public const float HYPER_LASER_CHARGE_TIME = 0.5f;
    public const float HYPER_LASER_DURATION = 5f;
    public const int HYPER_LASER_RAYCAST_COUNT = 6;
    public const float RAYCAST_SPACING = 0.5f;
    public const float RAYCAST_RADIUS = 0.15f;
    public const float RAYCAST_HORIZONTAL_OFFSET = 1.25f;

    public void SetCharge(int value)
    {
        HyperLaserCharge = value;
        HyperLaserCharge = Mathf.Clamp(HyperLaserCharge, 0, HYPER_LASER_CHARGE_MAX);
    }

    public void AddCharge(int value)
    {
        HyperLaserCharge += value;
        HyperLaserCharge = Mathf.Clamp(HyperLaserCharge, 0, HYPER_LASER_CHARGE_MAX);
    }

    private void OnEnable()
    {
        MMEventManager.AddListener(this);
    }

    private void OnDisable()
    {
        MMEventManager.RemoveListener(this);
    }

    private void Awake()
    {
        Instance = this;
    }

    public void OnMMEvent(LaserCannonBlockDestructionEvent e)
    {
        if (e.usingHyperLaser)
            return;

        var spawnParticle = Random.value < LaserUpgradeController.Instance.HyperLaserChargeSpawnChance;
        if (!spawnParticle)
            return;

        var particle = Instantiate(_particlePrefab);
        particle.transform.position = e.tilePos + new Vector2(0.5f, 0.5f);

        StartCoroutine(MoveToPlayerCo(particle));
    }

    private IEnumerator MoveToPlayerCo(GameObject particle)
    {
        var playerPos = Player.Instance.transform.position.WithZ(0f) + new Vector3(0f, 1f);
        var particlePos = particle.transform.position.WithZ(0f);
        var distance = Vector2.Distance(playerPos, particlePos);
        var initialPos = particlePos;
        var speed = 30f;
        var acc = 15f;

        var verticalOffset = Random.Range(1f, 5f);
        var direction = Random.value < 0.5f ? -1 : 1;

        var startPoint = particlePos;
        var midPoint = Vector3.Lerp(initialPos, playerPos, 0.5f);
        var offsetPoint = midPoint + Quaternion.Euler(0f, 0f, direction * 90f) * midPoint.normalized * verticalOffset;
        var endPoint = offsetPoint;

        while (distance > 0.1f)
        {
            speed += acc * Time.deltaTime;
            playerPos = Player.Instance.transform.position.WithZ(0f) + new Vector3(0f, 1f);
            particlePos = particle.transform.position.WithZ(0f);

            startPoint = Vector2.MoveTowards(startPoint, offsetPoint, speed * Time.deltaTime);
            endPoint = Vector2.MoveTowards(endPoint, playerPos, speed * Time.deltaTime);
            midPoint = Vector3.Lerp(initialPos, playerPos, 0.5f);
            offsetPoint = midPoint + Quaternion.Euler(0f, 0f, direction * 90f) * midPoint.normalized * verticalOffset;

            particle.transform.position = Vector2.MoveTowards(particlePos, endPoint, speed * Time.deltaTime);

            distance = Vector2.Distance(playerPos, particlePos);
            yield return null;
        }

        if (LaserCannon.Instance != null)
        {
            AddCharge(1);
        }

        Destroy(particle);
    }
}
