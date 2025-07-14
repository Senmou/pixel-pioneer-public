using UnityEngine.SceneManagement;
using MoreMountains.Tools;
using System.Collections;
using UnityEngine;

public class GameMusicController : MMPersistentSingleton<GameMusicController>
{
    [SerializeField] private AudioSource _musicAudioSource;
    [SerializeField] private AudioClip _mainMenuMusic;
    [SerializeField] private AudioClip _gameMusic;

    [Header("Low Pass Filter Settings")]
    [SerializeField] private AudioLowPassFilter _lowPassFilter;
    [SerializeField] private Vector2 _lowPassFrequencyRange;
    [SerializeField] private Vector2 _lowPassFrequencyRangeMainMenu;
    [SerializeField] private float _lowPassThresholdBelowSurface;

    private float _beforeFadeOutVolume;
    private bool _isPausedByLift;
    private TickSystem _tickSystem;

    private new void Awake()
    {
        base.Awake();

        if (_musicAudioSource == null)
            _musicAudioSource = gameObject.AddComponent<AudioSource>();

        _tickSystem = new(0.1f, AdjustLowPassFilter);
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnGameSceneLoaded;

        SetMusicByScene(SceneManager.GetActiveScene());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnGameSceneLoaded;
    }

    private void Update()
    {
        _tickSystem.Update();
    }

    private void AdjustLowPassFilter()
    {
        if (Player.Instance == null)
        {
            _lowPassFilter.cutoffFrequency = Helper.Remap(0f, -12f, _lowPassFrequencyRangeMainMenu.y, _lowPassFrequencyRangeMainMenu.x, Camera.main.transform.position.y);
            return;
        }

        var playerPosY = Player.Instance.transform.position.y;
        var lowestTerrainHeight = TilemapChunkSystem.Instance.World.deepestSurfaceHeight;
        var depthThreshold = lowestTerrainHeight - _lowPassThresholdBelowSurface;

        _lowPassFilter.cutoffFrequency = Helper.Remap(lowestTerrainHeight, depthThreshold, _lowPassFrequencyRange.y, _lowPassFrequencyRange.x, playerPosY);
    }

    private void OnGameSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetMusicByScene(scene);
    }

    private void SetMusicByScene(Scene scene)
    {
        if (scene.name.Equals("Game"))
            _musicAudioSource.clip = _gameMusic;
        else if (scene.name.Equals("MainMenu"))
            _musicAudioSource.clip = _mainMenuMusic;

        _musicAudioSource.Play();
    }

    public void PauseMusic(bool state)
    {
        _isPausedByLift = state;
        if (state)
            _musicAudioSource?.Pause();
        else
            _musicAudioSource?.UnPause();
    }

    public void ResetMusicVolume()
    {
        _musicAudioSource.volume = _beforeFadeOutVolume;
    }

    public void FadeOutMusic()
    {
        _beforeFadeOutVolume = _musicAudioSource.volume;
        StartCoroutine(FadeOutMusicCo());
    }

    private IEnumerator FadeOutMusicCo()
    {
        while (_musicAudioSource.volume > 0f)
        {
            _musicAudioSource.volume -= 0.5f * Time.deltaTime;
            if (_musicAudioSource.volume < 0.01f)
                _musicAudioSource.volume = 0f;
            yield return null;
        }
    }
}
