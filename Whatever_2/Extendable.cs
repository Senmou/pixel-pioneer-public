using MoreMountains.Feedbacks;
using UnityEngine;
using System;

public class Extendable : Placeable
{
    [Serializable]
    public enum Segment
    {
        SingleCenter,
        HorizontalLeft,
        HorizontalRight,
        HorizontalCenter,
        VerticalTop,
        VerticalCenter,
        VerticalBottom,
        DiagonalUp,
        DiagonalDown
    }

    [SerializeField] private bool _allowVertical;
    [SerializeField] private bool _allowHorizontal;
    [SerializeField] private float _width;
    [SerializeField] private float _height;
    [SerializeField] private Sprite _singleCenter;
    [SerializeField] private Sprite _horizontalLeft;
    [SerializeField] private Sprite _horizontalCenter;
    [SerializeField] private Sprite _horizontalRight;
    [SerializeField] private Sprite _verticalTop;
    [SerializeField] private Sprite _verticalCenter;
    [SerializeField] private Sprite _verticalBottom;
    [SerializeField] private MMF_Player _spawnVerticalFeedback;
    [SerializeField] private MMF_Player _spawnHorizontalFeedback;
    [SerializeField] private MMF_Player _placementVerticalFeedback;
    [SerializeField] private MMF_Player _placementHorizontalFeedback;
    [SerializeField] private MMF_Player _despawnFeedback;

    public float Width => _width;
    public float Height => _height;
    public bool AllowVertical => _allowVertical;
    public bool AllowHorizontal => _allowHorizontal;

    private Segment _segment;
    public Segment CurrentSegment => _segment;

    private void Awake()
    {
        UpdateSprite(Segment.SingleCenter);
    }

    public void PlayVerticalSpawnFeedback()
    {
        _spawnVerticalFeedback.PlayFeedbacks();
    }

    public void PlayHorizontalSpawnFeedback()
    {
        _spawnHorizontalFeedback.PlayFeedbacks();
    }

    public void PlayPlacementVerticalFeedback()
    {
        _spawnVerticalFeedback?.StopFeedbacks();
        _spawnHorizontalFeedback?.StopFeedbacks();
        _placementVerticalFeedback.PlayFeedbacks();
    }

    public void PlayPlacementHorizontalFeedback()
    {
        _spawnVerticalFeedback?.StopFeedbacks();
        _spawnHorizontalFeedback?.StopFeedbacks();
        _placementHorizontalFeedback.PlayFeedbacks();
    }

    public void PlayDespawnFeedback()
    {
        _spawnVerticalFeedback?.StopFeedbacks();
        _spawnHorizontalFeedback?.StopFeedbacks();
        _placementVerticalFeedback?.StopFeedbacks();
        _placementHorizontalFeedback?.StopFeedbacks();
        _despawnFeedback.PlayFeedbacks();
    }

    public void UpdateSprite(Segment segment)
    {
        _segment = segment;
        switch (segment)
        {
            case Segment.SingleCenter:
                _spriteRenderer.sprite = _singleCenter;
                break;
            case Segment.HorizontalLeft:
                _spriteRenderer.sprite = _horizontalLeft;
                break;
            case Segment.HorizontalRight:
                _spriteRenderer.sprite = _horizontalRight;
                break;
            case Segment.HorizontalCenter:
                _spriteRenderer.sprite = _horizontalCenter;
                break;
            case Segment.VerticalTop:
                _spriteRenderer.sprite = _verticalTop;
                break;
            case Segment.VerticalCenter:
                _spriteRenderer.sprite = _verticalCenter;
                break;
            case Segment.VerticalBottom:
                _spriteRenderer.sprite = _verticalBottom;
                break;
        }

        _placementIndicator.Init(_spriteRenderer.sprite);
    }
}