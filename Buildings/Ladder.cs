using Newtonsoft.Json;
using UnityEngine;
using System;

public class Ladder : BaseBuildable
{
    [SerializeField] private Collider2D _triggerCollider;

    private new void Awake()
    {
        base.Awake();
        _triggerCollider.enabled = false;
        OnFinishedBuilding += ActivateTrigger;
    }

    private void ActivateTrigger(object sender, EventArgs args)
    {
        _triggerCollider.enabled = true;
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
        _triggerCollider.enabled = true;
    }

    public override BuildingController.Direction GetBuildingDirection(Vector3 startElementPosition, Vector3 mousePosition)
    {
        var diff = mousePosition.y - startElementPosition.y;
        if (diff <= 0f)
            return BuildingController.Direction.DOWN;
        else
            return BuildingController.Direction.UP;
    }
}
