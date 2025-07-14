using System.Collections.Generic;
using static KinematicLift;
using UnityEngine;

public class LiftPlatform : MonoBehaviour
{
    [SerializeField] private KinematicLift _lift;
    [SerializeField] private PlayerDetector _playerDetector;

    private void Start()
    {
        _playerDetector.OnPlayerDetection += PlayerDetector_OnPlayerDetection;
    }

    private void OnDestroy()
    {
        _playerDetector.OnPlayerDetection -= PlayerDetector_OnPlayerDetection;
    }

    private void Update()
    {
        if (!_playerDetector.IsPlayerInRange)
            return;

        if (InputController.Instance.GetHoldKey(new List<KeyCode> { KeyCode.W, KeyCode.S }, out KeyCode holdKey))
        {
            if (holdKey == KeyCode.W)
            {
                _lift.Move(Direction.UP);
            }
            else if (holdKey == KeyCode.S)
            {
                _lift.Move(Direction.DOWN);
            }
        }
        else
        {
            _lift.Move(Direction.IDLE);
        }
    }

    private void PlayerDetector_OnPlayerDetection(object sender, bool isInRange)
    {
        if (isInRange)
        {
            _lift.OnEnterPlatform();
            MovementStatsController.Instance.SetGroundingForce(-10f);

            if (_lift.PlayMusic)
                GameMusicController.Instance.PauseMusic(true);
        }
        else
        {
            _lift.OnExitPlatform();
            MovementStatsController.Instance.ResetGroundingForce();

            if (_lift.PlayMusic)
                GameMusicController.Instance.PauseMusic(false);
        }
    }
}
