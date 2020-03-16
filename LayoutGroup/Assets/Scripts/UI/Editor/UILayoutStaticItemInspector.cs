using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UILayoutGroupElementStatic), true)]
public class UILayoutStaticItemInspector : Editor
{
    public override void OnInspectorGUI()
    {
        UILayoutGroupElementStatic layoutGroup = target as UILayoutGroupElementStatic;
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_index"));
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            layoutGroup.SetDirty();
        }
    }
}
