using UnityEditor;
using UnityEngine;
using System;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class MyReadOnlyAttribute : PropertyAttribute { }

#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(MyReadOnlyAttribute))]
public class MyReadOnlyAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label)
    {
        bool wasEnabled = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(rect, prop);
        GUI.enabled = wasEnabled;
    }
}
#endif