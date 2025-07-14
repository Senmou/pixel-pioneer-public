using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class IntroController : MonoBehaviour
{
    public static IntroController Instance { get; private set; }

    [SerializeField] private CinemachineCamera _introCam;
    [SerializeField] private SpaceCapsule _spaceCapsulePrefab;
    [SerializeField] private float _capsuleSpeed;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _levelBorderContainer;
    [SerializeField] private HoldToSkipButton _holdToSkipButtonPrefab;

    public bool IntroInAction { get; private set; }

    private bool _skipIntro;
    private HoldToSkipButton _spawnedHoldToSkipButton;

    private void Awake()
    {
        Instance = this;

        _introCam.gameObject.SetActive(false);
    }

    public void StartIntro(Vector2 playerSpawnPosition, bool skip = false)
    {
        _skipIntro = skip;

        IntroInAction = true;
        InputController.Instance.UseActionMap_Intro();
        _spawnedHoldToSkipButton = Instantiate(_holdToSkipButtonPrefab);
        _spawnedHoldToSkipButton.OnSkip += HoldToSkipButton_OnSkip;

        _levelBorderContainer.SetActive(false);
        _canvas.gameObject.SetActive(false);
        _introCam.Priority = 999;

        //var worldData = WorldSpawner.Instance.WorldData;

        //var introStartPos = new Vector3(worldData.Origin.x + 20f, worldData.Height - 20f);
        var spaceCapsule = Instantiate(_spaceCapsulePrefab);
        //spaceCapsule.transform.position = introStartPos.WithZ(spaceCapsule.transform.position.z);
        _introCam.Follow = spaceCapsule.transform;
        _introCam.PreviousStateIsValid = false;
        //_introCam.ForceCameraPosition(introStartPos, Quaternion.identity);

        var targetPoint = new Vector3(playerSpawnPosition.x, playerSpawnPosition.y, spaceCapsule.transform.position.z);
        StartCoroutine(MoveSpaceCapsuleCo(spaceCapsule, targetPoint));
    }

    private void HoldToSkipButton_OnSkip(object sender, System.EventArgs e)
    {
        _skipIntro = true;
    }

    private IEnumerator MoveSpaceCapsuleCo(SpaceCapsule spaceCapsule, Vector3 targetPoint)
    {
        spaceCapsule.transform.up = (targetPoint - spaceCapsule.transform.position).normalized;

        while (Vector3.Distance(spaceCapsule.transform.position.WithZ(0f), targetPoint.WithZ(0f)) > 1f)
        {
            if (_skipIntro)
            {
                spaceCapsule.transform.position = targetPoint;
            }

            spaceCapsule.transform.position = Vector3.MoveTowards(spaceCapsule.transform.position, targetPoint, _capsuleSpeed * Time.deltaTime);
            yield return null;
        }

        //spaceCapsule.ClipTerrain();
        //spaceCapsule.PlayExplosion();
        _introCam.Priority = 0;
        PlayerSpawner.Instance.SpawnPlayer(spaceCapsule.transform.position);
        Destroy(spaceCapsule.gameObject);
        _canvas.gameObject.SetActive(true);
        _levelBorderContainer.SetActive(true);

        Destroy(_spawnedHoldToSkipButton.gameObject);

        InputController.Instance.UseActionMap_Player();
        IntroInAction = false;
    }
}
