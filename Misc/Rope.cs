using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class Rope : MonoBehaviour
{
    [SerializeField] private float _ropeSegLen = 0.25f;
    [SerializeField] private int _segmentCount = 35;
    [SerializeField] private float _lineWidth = 0.1f;
    [SerializeField] private Vector2 forceGravity = new Vector2(0f, -1f);
    [SerializeField] private int _constraintSteps = 50;

    public List<RopeSegment> Segments => _ropeSegments;

    private bool _simulate = true;
    private bool _fixEndPoint;
    private bool _fixStartPoint;
    private Vector3 _endPoint;
    private Vector3 _startPoint;
    private LineRenderer _lineRenderer;
    private List<RopeSegment> _ropeSegments = new List<RopeSegment>();

    private void Start()
    {
        _lineRenderer = GetComponent<LineRenderer>();
    }

    public void CreateSegments(Vector2 initialRopePosition, Vector2 endPos)
    {
        var startPos = initialRopePosition;

        for (int i = 0; i < _segmentCount; i++)
        {
            var ropePos = Vector3.Lerp(startPos, endPos, i / (float)_segmentCount);
            _ropeSegments.Add(new RopeSegment(ropePos));
        }
    }

    public void SetSegments(List<RopeSegment> segments)
    {
        _ropeSegments = new List<RopeSegment>(segments);
    }

    private void Update()
    {
        if (_ropeSegments.Count > 0)
            DrawRope();
    }

    private void FixedUpdate()
    {
        if (_simulate && _ropeSegments.Count > 0)
            Simulate();
    }

    private void Simulate()
    {
        // Simulation
        for (int i = 0; i < _segmentCount; i++)
        {
            var firstSegment = _ropeSegments[i];
            var velocity = firstSegment.posNow - firstSegment.posOld;
            firstSegment.posOld = firstSegment.posNow;
            firstSegment.posNow += velocity;
            firstSegment.posNow += forceGravity * Time.deltaTime;
            _ropeSegments[i] = firstSegment;
        }

        // Constraints
        for (int i = 0; i < _constraintSteps; i++)
        {
            ApplyConstraint();
        }
    }

    public void UpdateSegmentLength(float totalRopeLength)
    {
        _ropeSegLen = totalRopeLength / _segmentCount;
    }

    private void ApplyConstraint()
    {
        var firstSegment = _ropeSegments[0];
        if (_fixStartPoint)
            firstSegment.posNow = _startPoint;
        else
            firstSegment.posNow = Vector3.zero;
        _ropeSegments[0] = firstSegment;

        for (int i = 0; i < _segmentCount - 1; i++)
        {
            var firstSeg = _ropeSegments[i];
            var secondSeg = _ropeSegments[i + 1];

            float dist = (firstSeg.posNow - secondSeg.posNow).magnitude;
            float error = dist - _ropeSegLen;
            Vector2 changeDir = (firstSeg.posNow - secondSeg.posNow).normalized;

            Vector2 changeAmount = changeDir * error;
            if (i != 0)
            {
                firstSeg.posNow -= changeAmount * 0.5f;
                _ropeSegments[i] = firstSeg;
                secondSeg.posNow += changeAmount * 0.5f;
                _ropeSegments[i + 1] = secondSeg;
            }
            else
            {
                secondSeg.posNow += changeAmount;
                _ropeSegments[i + 1] = secondSeg;
            }
        }

        var lastSegment = _ropeSegments[_ropeSegments.Count - 1];

        if (_fixEndPoint)
            lastSegment.posNow = _endPoint;

        _ropeSegments[_ropeSegments.Count - 1] = lastSegment;
    }

    public void SetStartPoint(Vector3 position)
    {
        _startPoint = transform.InverseTransformPoint(position);
        _fixStartPoint = true;
    }

    public void SetEndPoint(Vector3 position)
    {
        _endPoint = transform.InverseTransformPoint(position);
        _fixEndPoint = true;
    }

    public void TurnOffSimulationDelayed(float delay)
    {
        StartCoroutine(TurnOffSimulationDelayedCo(delay));
    }

    private IEnumerator TurnOffSimulationDelayedCo(float delay)
    {
        yield return new WaitForSeconds(delay);
        _simulate = false;
    }

    public void StartSimulation()
    {
        StopAllCoroutines();
        _simulate = true;
    }

    private void DrawRope()
    {
        float lineWidth = _lineWidth;
        _lineRenderer.startWidth = lineWidth;
        _lineRenderer.endWidth = lineWidth;

        Vector3[] ropePositions = new Vector3[_segmentCount];
        for (int i = 0; i < _segmentCount; i++)
        {
            ropePositions[i] = _ropeSegments[i].posNow;
        }

        _lineRenderer.positionCount = ropePositions.Length;
        _lineRenderer.SetPositions(ropePositions);
    }

    public struct RopeSegment
    {
        public Vector2 posNow;
        public Vector2 posOld;

        public RopeSegment(Vector2 pos)
        {
            posNow = pos;
            posOld = pos;
        }
    }
}
