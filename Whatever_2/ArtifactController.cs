using System.Collections.Generic;
using MoreMountains.Tools;
using System.Collections;
using Newtonsoft.Json;
using static Helper;
using UnityEngine;
using System.Linq;
using System;

public class ArtifactController : MonoBehaviour, ITooltip, MMEventListener<ArtifactRetrievedEvent>
{
    public static ArtifactController Instance { get; private set; }

    [SerializeField] private PrefabSO _prefabSO;
    [SerializeField] private DialogSO _dialog;

    private int _retrievedArtifactCount;
    private bool _hasDiscoveredFirstArtifact;
    private SaveData _saveData;

    public int RetrievedArtifactCount => _retrievedArtifactCount;

    public void OnMMEvent(ArtifactRetrievedEvent e)
    {
        _retrievedArtifactCount += e.artifactCount;

        ArtifactCounter.Instance.UpdateUI();
    }

    private void Awake()
    {
        Instance = this;

        var saveComp = GetComponent<SaveComponent>();
        saveComp.onLoad += OnLoad;
        saveComp.onSave += OnSave;

        SaveSystem.Instance.onDataPassed += OnDataPassed;
    }

    private void OnDataPassed()
    {
        if (_saveData != null)
        {
            _hasDiscoveredFirstArtifact = _saveData.hasDiscoveredFirstArtifact;
            StartCoroutine(SpawnArtifactsDelayedCo());
        }
    }

    private IEnumerator SpawnArtifactsDelayedCo()
    {
        yield return new WaitForSeconds(1.5f);

        _retrievedArtifactCount = _saveData.retrievedArtifactCount;
        foreach (var position in _saveData.artifactPositions)
        {
            Instantiate(_prefabSO.artefactPrefabList.Take(1).First(), position, Quaternion.identity);
        }
    }

    private void Start()
    {
        MMEventManager.AddListener(this);
    }

    public void OnEncounterArtifact()
    {
        if (!_hasDiscoveredFirstArtifact)
        {
            DialogController.Instance.EnqueueDialog(_dialog);
            _hasDiscoveredFirstArtifact = true;
        }
    }

    [Serializable]
    public class SaveData
    {
        public int retrievedArtifactCount;
        public bool hasDiscoveredFirstArtifact;
        public List<SerializableVector> artifactPositions;
    }

    private string OnSave()
    {
        var saveData = new SaveData();
        var artifacts = FindObjectsByType<BaseArtifact>(FindObjectsSortMode.None);
        saveData.retrievedArtifactCount = _retrievedArtifactCount;
        saveData.hasDiscoveredFirstArtifact = _hasDiscoveredFirstArtifact;
        saveData.artifactPositions = artifacts.Select(e => (SerializableVector)e.transform.position).ToList();
        return JsonConvert.SerializeObject(saveData);
    }

    private void OnLoad(string json)
    {
        _saveData = JsonConvert.DeserializeObject<SaveData>(json);
    }
}
