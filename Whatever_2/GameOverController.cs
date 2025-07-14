using MoreMountains.Feedbacks;
using System.Collections;
using UnityEngine;
using QFSW.QC;

public class GameOverController : MonoBehaviour
{
    [SerializeField] private Transform _singularityPrefab;
    [SerializeField] private float _growSpeed;
    [SerializeField] private MMF_Player _cameraShakeFeedback;
    [SerializeField] private MMF_Player _spawnAudiofeedback;
    [SerializeField] private MMF_Player _endScreenFeedback;

    private Transform _spawnedSingularity;

    private void Start()
    {
        PlanetStabilityController.Instance.OnStabilityZero += PlanetStabilityController_OnStabilityZero;
    }

    private void PlanetStabilityController_OnStabilityZero(object sender, System.EventArgs e)
    {
        SpawnSingularity();
    }

    private void SpawnSingularity()
    {
        StartCoroutine(SpawnSingularityCo());
    }

    private IEnumerator SpawnSingularityCo()
    {
        if (_spawnedSingularity != null)
        {
            StopAllCoroutines();
            Destroy(_spawnedSingularity.gameObject);
        }

        GameMusicController.Instance.FadeOutMusic();

        yield return new WaitForSeconds(3f);

        _spawnAudiofeedback.PlayFeedbacks();

        _spawnedSingularity = Instantiate(_singularityPrefab);
        //var worldData = WorldSpawner.Instance.WorldData;
        //_spawnedSingularity.position = new Vector3(worldData.Origin.x + worldData.Width / 2f, worldData.Origin.y + worldData.Height / 2f, -2f);

        StartCoroutine(GrowSingularityCo());
    }

    private IEnumerator GrowSingularityCo()
    {
        var refSize = 50f;
        var targetSize = 1f;// 3f * (WorldSpawner.Instance.WorldData.Height + WorldSpawner.Instance.WorldData.Width) / 2f;
        var speedFactor = targetSize / refSize;

        bool showEndMenu = true;
        bool disableAllThings = true;
        MainCanvas.Instance.CanvasGroup.blocksRaycasts = false;

        while (_spawnedSingularity.localScale.x < targetSize)
        {
            var sizeRatio = _spawnedSingularity.lossyScale.x / targetSize;
            var speed = speedFactor * _growSpeed + (sizeRatio * _growSpeed);
            var distanceToPlayer = Vector3.Distance(Player.Instance.transform.position.WithZ(0f), _spawnedSingularity.position.WithZ(0f));

            MainCanvas.Instance.CanvasGroup.alpha = Mathf.Clamp01(1f - 4f * sizeRatio);

            if (disableAllThings && _spawnedSingularity.localScale.x >= 3f * distanceToPlayer)
            {
                disableAllThings = false;
                Player.Instance.PlayerController.TakeAwayControl();
                MouseCursorController.Instance.SetCursor_Default();
                Player.Instance.gameObject.SetActive(false);
            }

            if (showEndMenu && MainCanvas.Instance.CanvasGroup.alpha == 0f)
            {
                showEndMenu = false;
                GameOverMenu.Show();
                _endScreenFeedback.PlayFeedbacks();
            }

            _cameraShakeFeedback.PlayFeedbacks(Player.Instance.transform.position, 1f - sizeRatio);
            _spawnedSingularity.localScale += new Vector3(speed, speed) * Time.deltaTime;
            yield return null;
        }
    }

    [Command(aliasOverride: "gameover")]
    public void GameOver()
    {
        SpawnSingularity();
    }
}
