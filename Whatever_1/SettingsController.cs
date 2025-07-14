using MoreMountains.Tools;
using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class SettingsController : MonoBehaviour
{
    public static SettingsController Instance { get; private set; }

    private const string PLAYER_PREFS_MUSIC_VOLUME_KEY = "MusicVolume";
    private const string PLAYER_PREFS_SFX_VOLUME_KEY = "SfxVolume";
    private const string PLAYER_PREFS_MASTER_VOLUME_KEY = "MasterVolume";
    private const string PLAYER_PREFS_UI_VOLUME_KEY = "UIVolume";
    private const string PLAYER_PREFS_SCREEN_SHAKE_INTENSITY_KEY = "ScreenShakeIntensity";

    public float ScreenShakeIntensity { get; private set; }

    [Header("Audio")]
    [SerializeField] private Slider _musicVolumeSlider;
    [SerializeField] private Slider _sfxVolumeSlider;
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _uiVolumeSlider;

    [Space(10)]
    [Header("Misc")]
    [SerializeField] private Slider _screenShakeSlider;

    private MMSoundManager _mmSoundManager;

    private void OnEnable()
    {
        if (_mmSoundManager == null)
            _mmSoundManager = FindFirstObjectByType<MMSoundManager>();

        if (_mmSoundManager == null)
        {
            Debug.LogWarning("No MMSoundManager in scene");
            return;
        }

        var musicVolume = PlayerPrefs.GetFloat(PLAYER_PREFS_MUSIC_VOLUME_KEY, 1f);
        var sfxVolume = PlayerPrefs.GetFloat(PLAYER_PREFS_SFX_VOLUME_KEY, 1f);
        var masterVolume = PlayerPrefs.GetFloat(PLAYER_PREFS_MASTER_VOLUME_KEY, 0.5f);
        var uiVolume = PlayerPrefs.GetFloat(PLAYER_PREFS_UI_VOLUME_KEY, 0.5f);
        var screenShakeIntensity = PlayerPrefs.GetFloat(PLAYER_PREFS_SCREEN_SHAKE_INTENSITY_KEY, 0.5f);
        
        StartCoroutine(SetMusicVolumeCo(musicVolume));
        StartCoroutine(SetSfxVolumeCo(sfxVolume));
        StartCoroutine(SetMasterVolumeCo(masterVolume));
        StartCoroutine(SetUIVolumeCo(uiVolume));
        StartCoroutine(SetScreenShakeIntensityCo(screenShakeIntensity));

        _musicVolumeSlider?.SetValueWithoutNotify(_mmSoundManager.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Music, false));
        _sfxVolumeSlider?.SetValueWithoutNotify(_mmSoundManager.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Sfx, false));
        _masterVolumeSlider?.SetValueWithoutNotify(_mmSoundManager.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.Master, false));
        _uiVolumeSlider?.SetValueWithoutNotify(_mmSoundManager.GetTrackVolume(MMSoundManager.MMSoundManagerTracks.UI, false));
        _screenShakeSlider?.SetValueWithoutNotify(screenShakeIntensity);
    }

    private void Awake()
    {
        Instance = this;
    }

    public void OnMusicVolumeChanged(float volume)
    {
        PlayerPrefs.SetFloat(PLAYER_PREFS_MUSIC_VOLUME_KEY, volume);
        _mmSoundManager?.SetVolumeMusic(volume);
    }

    public void OnSfxVolumeChanged(float volume)
    {
        PlayerPrefs.SetFloat(PLAYER_PREFS_SFX_VOLUME_KEY, volume);
        _mmSoundManager?.SetVolumeSfx(volume);
    }

    public void OnMasterVolumeChanged(float volume)
    {
        PlayerPrefs.SetFloat(PLAYER_PREFS_MASTER_VOLUME_KEY, volume);
        _mmSoundManager?.SetVolumeMaster(volume);
    }

    public void OnUIVolumeChanged(float volume)
    {
        PlayerPrefs.SetFloat(PLAYER_PREFS_UI_VOLUME_KEY, volume);
        _mmSoundManager?.SetVolumeUI(volume);
    }

    public void OnScreenShakeIntensityChanged(float intensity)
    {
        PlayerPrefs.SetFloat(PLAYER_PREFS_SCREEN_SHAKE_INTENSITY_KEY, intensity);
        ScreenShakeIntensity = intensity;
    }

    private IEnumerator SetMusicVolumeCo(float volume)
    {
        yield return null;
        OnMusicVolumeChanged(volume);
    }

    private IEnumerator SetSfxVolumeCo(float volume)
    {
        yield return null;
        OnSfxVolumeChanged(volume);
    }

    private IEnumerator SetMasterVolumeCo(float volume)
    {
        yield return null;
        OnMasterVolumeChanged(volume);
    }

    private IEnumerator SetUIVolumeCo(float volume)
    {
        yield return null;
        OnUIVolumeChanged(volume);
    }

    private IEnumerator SetScreenShakeIntensityCo(float intensity)
    {
        yield return null;
        OnScreenShakeIntensityChanged(intensity);
    }
}
