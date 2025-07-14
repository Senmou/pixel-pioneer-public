using UnityEngine;
using System;

public enum SaveLocation
{
    Local,
    Global
}

[RequireComponent(typeof(GuidComponent))]
public class SaveComponent : MonoBehaviour
{
    public SaveLocation saveFile;
    public bool autoSaving = true;

    public Func<string> onSave;
    public Action<string> onLoad;

    //public Func<string, string> onMigrate_1_2;
    //public Func<string, string> onMigrate_2_3;

    private GuidComponent guidComponent;

    [HideInInspector] public string DataName;
    public string GuidString { get; private set; }
    public Guid Guid { get; private set; }

    private void Awake()
    {
        guidComponent = GetComponent<GuidComponent>();
        Guid = guidComponent.GetGuid();
        GuidString = guidComponent.GetGuid().ToString();
    }
}
