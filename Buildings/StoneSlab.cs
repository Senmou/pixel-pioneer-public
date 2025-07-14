using Newtonsoft.Json;
using UnityEngine;
using System;

public class StoneSlab : BaseBuildable
{
    [SerializeField] private Collider2D _collider;

    private new void Awake()
    {
        base.Awake();
        OnFinishedBuilding += ToggleOffTrigger;
        Placeable.OnStartedPlacing += Placeable_OnStartedPlacing;
    }

    private void Placeable_OnStartedPlacing(object sender, EventArgs e)
    {
        _collider.isTrigger = true;
    }

    protected void ToggleOffTrigger(object sender, EventArgs args)
    {
        _collider.isTrigger = false;
    }

    public class SaveData
    {
        public Extendable.Segment segment;
    }

    public override string GetCustomJson()
    {
        var saveData = new SaveData();
        var extendable = GetComponent<Extendable>();
        saveData.segment = extendable.CurrentSegment;
        return JsonConvert.SerializeObject(saveData);
    }

    public override void Load(string json)
    {
        var saveData = JsonConvert.DeserializeObject<SaveData>(json);
        var extendable = GetComponent<Extendable>();
        extendable.UpdateSprite(saveData.segment);
        _collider.isTrigger = false;
    }

    public override BuildingController.Direction GetBuildingDirection(Vector3 startElementPosition, Vector3 mousePosition)
    {
        var mouseDir = (mousePosition - startElementPosition.WithZ(0f)).normalized;

        // Calculate distance from mid point of the buildable
        var verticalDistance = mousePosition.y - startElementPosition.y;
        var horizontalDistance = mousePosition.x - startElementPosition.x;

        var diagDotValue = 0.5f;
        var dot = Vector3.Dot(Vector3.up, mouseDir);
        var isLeft = horizontalDistance < 0f;
        var isDown = verticalDistance < 0f;

        BuildingController.Direction buildDirection = BuildingController.Direction.UP;

        //string text = "";
        if (!isDown && dot >= 1f - diagDotValue)
        {
            //text = "up";
            buildDirection = BuildingController.Direction.UP;
        }
        else if (isDown && dot <= -1f + diagDotValue)
        {
            //text = "down";
            buildDirection = BuildingController.Direction.DOWN;
        }
        else if (dot <= diagDotValue && dot >= -diagDotValue)
        {
            if (isLeft)
            {
                //text = "left";
                buildDirection = BuildingController.Direction.LEFT;
            }
            else
            {
                //text = "right";
                buildDirection = BuildingController.Direction.RIGHT;
            }
        }
        else
        {
            //text = "diag";
            //if (isLeft && !isDown) buildDirection = BuildingController.Direction.UP_LEFT;
            //else if (!isLeft && !isDown) buildDirection = BuildingController.Direction.UP_RIGHT;
            //else if (isLeft && isDown) buildDirection = BuildingController.Direction.DOWN_LEFT;
            //else if (!isLeft && isDown) buildDirection = BuildingController.Direction.DOWN_RIGHT;
        }

        //WorldText.Instance.SetText(text + " " + dot, mousePos);

        return buildDirection;
    }
}
