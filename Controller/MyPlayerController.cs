using TarodevController;
using UnityEngine;
using System;

public class MyPlayerController : PlayerController
{
    public event EventHandler<OnHitGroundAfterFallingEventArgs> OnHitGroundAfterFalling;
    public class OnHitGroundAfterFallingEventArgs : EventArgs
    {
        public float fallHeight;
    }

    [SerializeField] protected ScriptableStats MoveablePlatformStats;

    public new bool IsOnWall => base.IsOnWall;
    public bool ShouldStickToWall => WallDirection != 0 && !Grounded && _shouldStickToWall;

    private bool _isFalling;
    private float _lastGroundedHeight;
    private bool _isFlying;
    private bool _shouldStickToWall;
    private ScriptableStats _lastMovementStats;

    public void ResetToLastMovementStats()
    {
        Stats = _lastMovementStats;
    }

    public void SetMoveablePlatformMovementStats()
    {
        if (Stats != MoveablePlatformStats)
            _lastMovementStats = Stats;
        Stats = MoveablePlatformStats;
    }

    public void SetNormalMovementStats()
    {
        if (Stats != _normalMovementStats)
            _lastMovementStats = Stats;
        Stats = _normalMovementStats;
    }

    public void SetMovementStats(ScriptableStats movementStats)
    {
        if (Stats != movementStats)
            _lastMovementStats = Stats;
        Stats = movementStats;
    }

    public void SetAirControl(bool flying)
    {
        _isFlying = flying;
    }

    public void StickToWall(bool landedHook)
    {
        _shouldStickToWall = landedHook;
    }

    protected override void HandleVertical()
    {
        if (_isFlying)
        {
            var moveX = FrameInput.Move.x;
            var moveY = FrameInput.Move.y;
            _speed.x = moveX * Stats.MaxSpeed * 2f;
            _speed.y = moveY * Stats.MaxSpeed * 2f;
            return;
        }

        if (Dashing) return;

        // Ladder
        if (ClimbingLadder)
        {
            _isFalling = false;
            var yInput = FrameInput.Move.y;

            var touchingSecondLadder = LadderHits[1] != null;
            var preventFromClimbingOverTop = transform.position.y >= LadderHits[0].bounds.max.y;

            //WorldText.Instance.SetText($"{(LadderHits[0] != null) + " " + (LadderHits[1] != null)}", PixelGrid.MouseCellPos());

            _speed.y = yInput * (yInput > 0 ? Stats.LadderClimbSpeed : Stats.LadderSlideSpeed) * (yInput > 0 && preventFromClimbingOverTop && !touchingSecondLadder ? 0f : 1f);
        }
        // Grounded & Slopes
        else if (Grounded && _speed.y <= 0f)
        {
            if (_isFalling)
            {
                var fallHeight = _lastGroundedHeight - transform.position.y;
                _isFalling = false;

                OnHitGroundAfterFalling?.Invoke(this, new OnHitGroundAfterFallingEventArgs { fallHeight = fallHeight });
            }

            _speed.y = Stats.GroundingForce;

            if (TryGetGroundNormal(out var groundNormal))
            {
                GroundNormal = groundNormal;
                if (!Mathf.Approximately(GroundNormal.y, 1f))
                {
                    if (GroundNormal.y <= 0.01f && GroundNormal.y > -0.01f)
                        print($"GroundNormal.y = {GroundNormal.y}");
                    else
                    {
                        // on a slope
                        _speed.y = _speed.x * -GroundNormal.x / GroundNormal.y;
                        if (_speed.x != 0) _speed.y += Stats.GroundingForce;
                    }
                }
            }
        }
        // Wall Climbing & Sliding
        else if (IsOnWall && !IsLeavingWall)
        {
            _lastGroundedHeight = transform.position.y;
            _speed.y = Mathf.MoveTowards(Mathf.Min(_speed.y, 0), -Stats.MaxWallFallSpeed, Stats.WallFallAcceleration * Time.fixedDeltaTime);
        }
        // In Air
        else
        {
            if (!_isFalling)
                _lastGroundedHeight = transform.position.y;

            var inAirGravity = Stats.FallAcceleration;
            if (EndedJumpEarly && _speed.y > 0) inAirGravity *= Stats.JumpEndEarlyGravityModifier;
            _speed.y = Mathf.MoveTowards(_speed.y, -Stats.MaxFallSpeed, inAirGravity * Time.fixedDeltaTime);
            _isFalling = _speed.y < 0f;
        }
    }

    protected override bool TryStickToWall()
    {
        //print($"{WallDirection}   {Grounded}   {_shouldStickToWall}");

        if (WallDirection == 0 || Grounded)
            return false;

        return _shouldStickToWall;
    }
}
