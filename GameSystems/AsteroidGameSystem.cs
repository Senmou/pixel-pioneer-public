using System.Collections.Generic;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using static Asteroid;
using UnityEngine;
using System;

public class AsteroidGameSystem : MonoBehaviour, ISaveable
{
    [SerializeField] private bool _startInstantly;
    [SerializeField] private bool _autoSpawn;
    [SerializeField] private bool _clickSpawn;
    [SerializeField] private Asteroid _asteroidPrefabSmall;
    [SerializeField] private Asteroid _asteroidPrefabMedium;
    [SerializeField] private Asteroid _asteroidPrefabBig;
    [SerializeField] private LayerMask _terrainLayerMask;
    [SerializeField] private MMF_Player _asteroidImpactSmall;
    [SerializeField] private MMF_Player _asteroidImpactMedium;
    [SerializeField] private GameObject _asteroidStartWarning;

    [Space(10)]
    [Header("Stats")]
    [SerializeField] private Vector2 _initialDelayMinutes;
    [SerializeField] private Vector2 _waveDelayMinutes;
    [SerializeField] private Vector2 _waveDurationMinutes;
    [SerializeField] private Vector2 _asteroidMinSpawnTimeSeconds;
    [SerializeField] private Vector2 _asteroidMaxSpawnTimeSeconds;

    [SerializeField] private AnimationCurve _difficultyCurve;

    private float WaveDelayMinutes => UnityEngine.Random.Range(_waveDelayMinutes.x, _waveDelayMinutes.y);
    private float WaveDurationMinutes => UnityEngine.Random.Range(_waveDurationMinutes.x, _waveDurationMinutes.y);
    private float AsteroidMinSpawnTimeSeconds => UnityEngine.Random.Range(_asteroidMinSpawnTimeSeconds.x, _asteroidMinSpawnTimeSeconds.y);
    private float AsteroidMaxSpawnTimeSeconds => UnityEngine.Random.Range(_asteroidMaxSpawnTimeSeconds.x, _asteroidMaxSpawnTimeSeconds.y);

    private float _spawnTimer;
    private List<Asteroid> _asteroids = new List<Asteroid>();
    private float _difficulty;
    [ReadOnly] public Wave _currentWave;

    public Wave CurrentWave => _currentWave;
    public List<Asteroid> Asteroids => _asteroids;
    public bool IsWaveInAction => Time.time > _currentWave.startTime && Time.time < _currentWave.EndTime;

    [Serializable]
    public struct Wave
    {
        public float startTime;
        public float duration;
        public float EndTime => startTime + duration;

        public float minAsteroidSpawnTime;
        public float maxAsteroidSpawnTime;
    }

    private void Start()
    {
        var initialStartTime = Time.time + 60f * UnityEngine.Random.Range(_initialDelayMinutes.x, _initialDelayMinutes.y);
        CreateWave(initialStartTime);
    }

    private void Update()
    {
        if (_autoSpawn)
        {
            if (Time.time < _currentWave.startTime && !_startInstantly)
                return;

            HandleDifficulty();
            HandleSpawning();

            if (Time.time > _currentWave.EndTime)
            {
                CreateWave();
                _difficulty = 0f;
            }

            Vector4 redTint = new Vector4(1f, 0f, 0.3f, -0.8f);
            PostProcessController.Instance.SetGainProperty(_difficulty, redTint);
        }

        if (_clickSpawn && Input.GetMouseButtonDown(0))
            SpawnAsteroid(Helper.MousePos);
    }

    private void CreateWave(float startTime = -1f)
    {
        _currentWave = new Wave();

        if (startTime > 0f)
            _currentWave.startTime = startTime;
        else
            _currentWave.startTime = Time.time + 60f * WaveDelayMinutes;

        _currentWave.duration = 60f * WaveDurationMinutes;
        _currentWave.minAsteroidSpawnTime = AsteroidMinSpawnTimeSeconds;
        _currentWave.maxAsteroidSpawnTime = AsteroidMaxSpawnTimeSeconds;
    }

    private void PrintWaveDetails()
    {
        print("=== New wave ===");
        print($"startTime: {(int)(_currentWave.startTime / 60f)}:{(int)(_currentWave.startTime % 60f)}");
        print($"duration: {(int)(_currentWave.duration / 60f)}:{(int)(_currentWave.duration % 60f)}");
        print($"minSpawnTime: {_currentWave.minAsteroidSpawnTime}");
        print($"maxSpawnTime: {_currentWave.maxAsteroidSpawnTime}");
    }

    private void HandleDifficulty()
    {
        var normalizedTime = 0f;
        var halfDuration = _currentWave.duration / 2f;
        var currentDuration = Time.time - _currentWave.startTime;
        var halfTime = _currentWave.startTime + halfDuration;

        if (currentDuration < halfDuration)
            normalizedTime = Helper.Remap(_currentWave.startTime, halfTime, 0f, 1f, Time.time);
        else
            normalizedTime = Helper.Remap(halfTime, _currentWave.EndTime, 1f, 0f, Time.time);

        _difficulty = _difficultyCurve.Evaluate(normalizedTime);
    }

    private void HandleSpawning()
    {
        _spawnTimer += Time.deltaTime;

        var spawnTime = Helper.Remap(0f, 1f, _currentWave.maxAsteroidSpawnTime, _currentWave.minAsteroidSpawnTime, _difficulty);
        if (_spawnTimer > spawnTime || _startInstantly)
        {
            _spawnTimer = 0f;
            SpawnAsteroid(_difficulty);
        }
    }

    private void SpawnAsteroid(float difficulty)
    {
        //var horizontalDeadZone = 5f;
        //var spawnHeight = 2f * Helper.LastLevelDataSO.LevelData.Height;
        //var spawnLeft = new Vector3(Helper.LastLevelDataSO.LevelData.Origin.x, spawnHeight);
        //var spawnRight = new Vector3(Helper.LastLevelDataSO.LevelData.Origin.x + Helper.LastLevelDataSO.LevelData.Width, spawnHeight);

        //var targetLeft = new Vector3(Helper.LastLevelDataSO.LevelData.Origin.x + horizontalDeadZone, Helper.LastLevelDataSO.LevelData.Origin.y);
        //var targetRight = new Vector3(Helper.LastLevelDataSO.LevelData.Origin.x + Helper.LastLevelDataSO.LevelData.Width - horizontalDeadZone, Helper.LastLevelDataSO.LevelData.Origin.y);

        //var spawnPos = Vector3.Lerp(spawnLeft, spawnRight, UnityEngine.Random.value);
        //var targetPos = Vector3.Lerp(targetLeft, targetRight, UnityEngine.Random.value);
        //var targetDir = (targetPos - spawnPos).normalized;

        //var asteroidSize = Mathf.Clamp01((UnityEngine.Random.value + difficulty) / 2f);
        //Asteroid asteroidPrefab;
        //if (asteroidSize >= 0.9f)
        //    asteroidPrefab = _asteroidPrefabBig;
        //else if (asteroidSize >= 0.6f)
        //    asteroidPrefab = _asteroidPrefabMedium;
        //else
        //    asteroidPrefab = _asteroidPrefabSmall;

        //var asteroid = Instantiate(asteroidPrefab, spawnPos, Quaternion.identity, null);
        //asteroid.Init(targetDir, this);

        //_asteroids.Add(asteroid);
    }

    private void SpawnAsteroid(Vector3 spawnPos)
    {
        var asteroid = Instantiate(GetRandomPrefab(), spawnPos, Quaternion.identity, null);

        var direction = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(-30f, 30f)) * Vector3.down;

        asteroid.Init(direction, this);
        _asteroids.Add(asteroid);
    }

    private Asteroid GetRandomPrefab()
    {
        var rand = UnityEngine.Random.value;
        if (rand >= 0.9f)
            return _asteroidPrefabBig;
        if (rand >= 0.5f)
            return _asteroidPrefabMedium;
        return _asteroidPrefabSmall;
    }

    public void RemoveAsteroid(Asteroid asteroid)
    {
        _asteroids.Remove(asteroid);
    }

    public void PlayImpactFeedback(Vector3 position, AsteroidSize asteroidSize)
    {
        if (asteroidSize == AsteroidSize.SMALL)
        {
            var intensity = 1.1f - Mathf.Clamp01(Vector3.Distance(Player.Instance.transform.position, position) / 100f);
            _asteroidImpactSmall.PlayFeedbacks(position, intensity);
        }

        if (asteroidSize == AsteroidSize.MEDIUM)
        {
            var intensity = 1.5f - Mathf.Clamp01(Vector3.Distance(Player.Instance.transform.position, position) / 100f);
            _asteroidImpactMedium.PlayFeedbacks(position, intensity);
        }

        if (asteroidSize == AsteroidSize.LARGE)
        {
            var intensity = 2f - Mathf.Clamp01(Vector3.Distance(Player.Instance.transform.position, position) / 100f);
            _asteroidImpactMedium.PlayFeedbacks(position, intensity);
        }
    }

    [Serializable]
    public class AsteroidGameSystemData
    {
        public bool autoSpawn;
        public float timer;
        public Wave wave;
    }

    public string GetCustomJson()
    {
        var data = new AsteroidGameSystemData();
        data.autoSpawn = _autoSpawn;
        data.timer = _spawnTimer;
        data.wave = _currentWave;
        return JsonConvert.SerializeObject(data);
    }

    public void Load(string json)
    {
        var data = JsonConvert.DeserializeObject<AsteroidGameSystemData>(json);
        _autoSpawn = data.autoSpawn;
        _spawnTimer = data.timer;
        _currentWave = data.wave;
    }
}
