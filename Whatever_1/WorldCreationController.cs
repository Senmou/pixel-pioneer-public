using System;
using System.Collections;
using UnityEngine;

public class WorldCreationController : MonoBehaviour
{
    public static WorldCreationController Instance { get; private set; }

    [SerializeField] private Portal _portalPrefab;

    private void Awake()
    {
        Instance = this;
    }

    public void OnWorldCreated(World world, bool isNewWorld, WorldParameters worldParameters, Vector2 playerTilePos)
    {
        if (isNewWorld)
        {
            SpawnArtifacts(world);
        }

        StartCoroutine(SpawnPlayerAndPortalDelayedCo(playerTilePos, useSavedPosition: !isNewWorld));

        var tileSize = 1f;
        LevelBorderController.Instance.InitBorders(Vector3.zero, worldParameters.BlocksHorizontal * tileSize, worldParameters.BlocksVertical * tileSize);
    }

    private void SpawnArtifacts(World world)
    {
        for (int i = 0; i < world.ArtifactSpawnPositions.Count; i++)
        {
            var spawnPosition = world.ArtifactSpawnPositions[i];
            var artifact = PrefabManager.Instance.Prefabs.artefactPrefabList[i % PrefabManager.Instance.Prefabs.artefactPrefabList.Count];
            Instantiate(artifact, spawnPosition, Quaternion.identity);
        }
    }

    private void SpawnPortal(Vector2 playerTilePos)
    {
        var portal = Instantiate(_portalPrefab, playerTilePos, Quaternion.identity);
        StartCoroutine(FinishPortalDelayedCo(portal));
    }

    private IEnumerator FinishPortalDelayedCo(Portal portal)
    {
        yield return null;
        portal.FinishBuildingOnLoad();
    }

    private IEnumerator SpawnPlayerAndPortalDelayedCo(Vector2 playerTilePos, bool useSavedPosition)
    {
        yield return new WaitForSeconds(0.5f);

        PlayerSpawner.Instance.SpawnPlayer(playerTilePos, useSavedPosition);

        Player.Instance.PlayerController.FreezePlayer();

        yield return null;
        PlayerCamera.Instance.SnapCamera();

        if (FindAnyObjectByType<Portal>() == null)
            SpawnPortal(playerTilePos);

        yield return new WaitForSeconds(1.5f);
        Player.Instance.PlayerController.UnfreezePlayer();

        if (GameManager.Instance.IsFirstGameStart())
            GameManager.Instance.SaveGame();
    }
}
