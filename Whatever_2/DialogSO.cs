using UnityEngine.Localization.Settings;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine.Localization;
using UnityEngine;
using System;

[CreateAssetMenu(menuName = "Dialog")]
public class DialogSO : ScriptableObject
{
    [MyReadOnly] public string Id;
    private void OnValidate()
    {
#if UNITY_EDITOR
        if (Id == "")
        {
            Id = UnityEditor.GUID.Generate().ToString();
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }

    public bool debug;
    public bool oneShot;
    public float delay;
    public List<DialogLine> lines = new();
    public Dict<string, string> keyDict = new();
}

[Serializable]
public class DialogLine
{
    public DialogSO dialog; // only used for smart strings
    public LocalizedString text;
    public DialogEventSO onShowEvent;
    public DialogEventSO onHideEvent;
    public DialogEventSO endLineEvent;
    public float delay;
    public float delayAfterSkip;
    public float extraDuration;
    public float progress;

    public bool ShouldSkip { get; private set; }
    public bool ShowProgress => endLineEvent != null && endLineEvent.Incremental;

    private Action _onProgress;

    public void Init(Action onProgress)
    {
        ShouldSkip = endLineEvent == null;

        if (endLineEvent != null)
            endLineEvent.Subscribe(this, OnSkipEvent);

        progress = 0f;
        _onProgress = onProgress;
        _onProgress?.Invoke();
    }

    public string GetLocalizedText()
    {
        var table = LocalizationSettings.StringDatabase.GetTable(text.TableReference);
        var entryKey = table.SharedData.GetEntryFromReference(text.TableEntryReference).Key;
        var entry = table.GetEntry(entryKey);

        if (!entry.IsSmart)
            return text.GetLocalizedString();
        else
        {
            if (dialog == null)
            {
                Debug.LogError("DialogSO is null, but needed for smart string");
                return "[Error]";
            }

            var rawSmartString = entry.LocalizedValue;
            var matches = Regex.Matches(rawSmartString, @"\{([^}]*)\}");

            Dict<string, string> parameters = new();
            foreach (Match match in matches)
            {
                var parameterName = match.Groups[1].Value;
                parameters[parameterName] = dialog.keyDict[parameterName];
            }

            return text.GetLocalizedString(parameters);
        }
    }

    public void Destroy()
    {
        progress = 0f;

        if (endLineEvent != null)
            endLineEvent.Unsubscribe(OnSkipEvent);

        _onProgress = null;
    }

    private void OnSkipEvent(object sender, DialogEventSO.OnTriggerEventArgs e)
    {
        if (e.incremental)
        {
            progress += e.progressInc;
            progress = Mathf.Clamp01(progress);

            ShouldSkip = progress >= 0.99f;

            _onProgress?.Invoke();
        }
        else
        {
            ShouldSkip = true;
        }
    }
}
