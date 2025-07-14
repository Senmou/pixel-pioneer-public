using static PowerConnectionController;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class RopeConnection
{
    public Rope rope;
    public PowerLineSnappingPoint otherSnappingPoint;
}

public class PowerLineSnappingPoint : MonoBehaviour
{
    [SerializeField] private int _id;
    [SerializeField] private BaseBuilding _baseBuilding;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _normalColor;
    [SerializeField] private Color _selectedColor;

    public int Id => _id;
    public Rope ConnectedRope { get; private set; }
    public bool IsSnappable { get; private set; }
    public BaseBuilding BaseBuilding => _baseBuilding;

    private List<RopeConnection> _connectedPoints = new List<RopeConnection>();
    public List<RopeConnection> ConnectedPoints => _connectedPoints;

    private void Awake()
    {
        Tint(_normalColor);
        _spriteRenderer.gameObject.SetActive(false);
    }

    private void Start()
    {
        PowerConnectionController.Instance.OnStateChanged += PowerConnectionController_OnStateChanged;

        // Place power line
        PowerConnectionController.Instance.OnStartedPlacingPowerLine += PowerConnectionController_OnStartedPlacingPowerLine;
        PowerConnectionController.Instance.OnSetPowerLineStartPosition += PowerConnectionController_OnSetPowerLineStartPosition;
        PowerConnectionController.Instance.OnSetPowerLineEndPosition += PowerConnectionController_OnSetPowerLineEndPosition;

        // Remove power line
        PowerConnectionController.Instance.OnSelectedFirstSnappingPointToRemove += PowerConnectionController_OnSelectedFirstSnappingPointToRemove;
    }

    private void OnDestroy()
    {
        PowerConnectionController.Instance.OnStateChanged -= PowerConnectionController_OnStateChanged;

        // Place power line
        PowerConnectionController.Instance.OnStartedPlacingPowerLine -= PowerConnectionController_OnStartedPlacingPowerLine;
        PowerConnectionController.Instance.OnSetPowerLineStartPosition -= PowerConnectionController_OnSetPowerLineStartPosition;
        PowerConnectionController.Instance.OnSetPowerLineEndPosition -= PowerConnectionController_OnSetPowerLineEndPosition;

        // Remove power line
        PowerConnectionController.Instance.OnSelectedFirstSnappingPointToRemove -= PowerConnectionController_OnSelectedFirstSnappingPointToRemove;
    }

    private void PowerConnectionController_OnStateChanged(object sender, State e)
    {
        if (e == State.None)
        {
            Hide();
            return;
        }

        if (e == State.RemoveConnection && _connectedPoints.Count == 0)
            Hide();
        else if (e == State.RemoveConnection && _connectedPoints.Count > 0)
            Show();
    }

    private void PowerConnectionController_OnSelectedFirstSnappingPointToRemove(object sender, PowerConnectionController.OnPowerLineEventArgs args)
    {
        // Hide first selected point
        if (args.startSnappingPoint == this)
            Hide();

        // Hide all other points that are not connected to the first selected one
        var hasSelectedPointAsConnection = _connectedPoints.Where(e => e.otherSnappingPoint == args.startSnappingPoint).FirstOrDefault() != null;
        if (!hasSelectedPointAsConnection)
            Hide();
    }

    public void OnConnectPowerLine(PowerLineSnappingPoint otherSnappingPoint, Rope rope)
    {
        var thisPowerEntity = _baseBuilding.GetComponent<IPowerGridEntity>();
        if (thisPowerEntity == null)
        {
            Debug.LogWarning("Building is no IPowerEntity", gameObject);
            return;
        }

        var otherPowerEntity = otherSnappingPoint.BaseBuilding.GetComponent<IPowerGridEntity>();
        if (otherPowerEntity == null)
        {
            Debug.LogWarning("Building is no IPowerEntity", otherSnappingPoint.gameObject);
            return;
        }

        _connectedPoints.Add(new RopeConnection { otherSnappingPoint = otherSnappingPoint, rope = rope });
        otherSnappingPoint.ConnectedPoints.Add(new RopeConnection { otherSnappingPoint = this, rope = rope });

        PowerGridController.Instance.ConnectBuildings(thisPowerEntity, otherPowerEntity);
    }

    public void OnRemovePowerLine(PowerLineSnappingPoint otherSnappingPoint)
    {
        var thisPowerEntity = _baseBuilding.GetComponent<IPowerGridEntity>();
        if (thisPowerEntity == null)
            Debug.LogWarning("Building is no IPowerEntity", gameObject);

        var otherPowerEntity = otherSnappingPoint.BaseBuilding.GetComponent<IPowerGridEntity>();
        if (otherPowerEntity == null)
            Debug.LogWarning("Building is no IPowerEntity", otherSnappingPoint.gameObject);

        var ropeConnectionA = _connectedPoints.Where(e => e.otherSnappingPoint == otherSnappingPoint).FirstOrDefault();
        var ropeConnectionB = otherSnappingPoint.ConnectedPoints.Where(e => e.otherSnappingPoint == this).FirstOrDefault();

        if (ropeConnectionA != null)
        {
            _connectedPoints.Remove(ropeConnectionA);
            Destroy(ropeConnectionA.rope.gameObject);
        }
        else
        {
            Debug.LogWarning("Rope connection was null");
            return;
        }

        if (ropeConnectionB != null)
            otherSnappingPoint.ConnectedPoints.Remove(ropeConnectionB);
        else
        {
            Debug.LogWarning("Rope connection was null");
            return;
        }

        PowerGridController.Instance.DisconnectBuildings(thisPowerEntity, otherPowerEntity);
    }

    private void PowerConnectionController_OnSetPowerLineStartPosition(object sender, PowerConnectionController.OnPowerLineEventArgs e)
    {
        var thisPowerEntity = _baseBuilding.GetComponent<IPowerGridEntity>();
        if (thisPowerEntity == null)
            Debug.LogWarning("Building is no IPowerEntity", gameObject);

        var otherPowerEntity = e.startSnappingPoint.BaseBuilding.GetComponent<IPowerGridEntity>();
        if (otherPowerEntity == null)
            Debug.LogWarning("Building is no IPowerEntity", e.startSnappingPoint.gameObject);

        var hasAlreadyConnection = PowerGridController.Instance.HasConnection(thisPowerEntity, otherPowerEntity);
        var tooFarAway = Vector2.Distance(transform.position, e.ropeStartPosition) > e.maxRopeLength;
        var isSnappingPointOnSameBuilding = e.startSnappingPoint.BaseBuilding == _baseBuilding;

        if (tooFarAway || isSnappingPointOnSameBuilding || hasAlreadyConnection)
        {
            Hide();
        }
    }

    private void PowerConnectionController_OnSetPowerLineEndPosition(object sender, OnPowerLineEventArgs e)
    {
        if (e.startSnappingPoint == null || e.endSnappingPoint == null)
            return;

        e.startSnappingPoint.ConnectedRope = e.rope;
        e.endSnappingPoint.ConnectedRope = e.rope;

        Hide();
    }

    private void PowerConnectionController_OnStartedPlacingPowerLine(object sender, EventArgs e)
    {
        Show();
    }

    public void SetSelectedColor()
    {
        Tint(_selectedColor);
    }

    public void ResetColor()
    {
        Tint(_normalColor);
    }

    private void Tint(Color color)
    {
        _spriteRenderer.material.SetColor("_Tint", color);
    }

    public void Show()
    {
        if (!_baseBuilding.IsBuildingFinished)
            return;

        ResetColor();

        IsSnappable = true;
        _spriteRenderer.gameObject.SetActive(true);
    }

    public void Hide()
    {
        IsSnappable = false;
        _spriteRenderer.gameObject.SetActive(false);
    }
}
