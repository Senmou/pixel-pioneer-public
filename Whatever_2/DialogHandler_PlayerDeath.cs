using UnityEngine;
using System;

public class DialogHandler_PlayerDeath : MonoBehaviour
{
    [SerializeField] private DialogSO _playerFirstDeath;
    [SerializeField] private DialogSO _playerSecondDeath;
    [SerializeField] private DialogSO _playerRespawn;

    private void Start()
    {
        PlayerSpawner.Instance.OnPlayerSpawned += PlayerSpawner_OnPlayerSpawned;
    }

    private void OnDestroy()
    {
        PlayerSpawner.Instance.OnPlayerSpawned -= PlayerSpawner_OnPlayerSpawned;
        Player.Instance.OnPlayerDied -= Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned -= Player_OnPlayerRespawned;
    }

    private void PlayerSpawner_OnPlayerSpawned(object sender, EventArgs e)
    {
        Player.Instance.OnPlayerDied += Player_OnPlayerDied;
        Player.Instance.OnPlayerRespawned += Player_OnPlayerRespawned;
    }

    private void Player_OnPlayerRespawned(object sender, EventArgs e)
    {
        var playerName = Player.Instance.PlayerName;
        _playerRespawn.keyDict["PlayerName"] = playerName;
        _playerRespawn.keyDict["Credits"] = $"{GlobalStats.Instance.LastCreditsLostByDeath}";

        ShowDialog(_playerRespawn);
    }

    private void Player_OnPlayerDied(object sender, EventArgs e)
    {
        //HandleDeathDialog();
    }

    private void HandleDeathDialog()
    {
        if (GlobalStats.Instance.DeathCounter == 1)
        {
            ShowDialog(_playerFirstDeath);
        }
        else if (GlobalStats.Instance.DeathCounter == 2)
        {
            ShowDialog(_playerSecondDeath);
        }
    }

    private void ShowDialog(DialogSO dialog)
    {
        DialogController.Instance.EnqueueDialog(dialog);
    }
}
