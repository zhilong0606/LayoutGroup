using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(UILayoutGroupElementStatic), true)]
public class UILayoutStaticItemInspector : Editor
{
    public override void OnInspectorGUI()
    {
        UILayoutGroupElementStatic staticElement = target as UILayoutGroupElementStatic;
        EditorGUI.BeginChangeCheck();
        EditorGUILayout.PropertyField(serializedObject.FindProperty("m_indexType"));
        if (staticElement.indexType == UILayoutGroupElementStatic.EIndexType.Custom)
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_index"));
        }
        if (EditorGUI.EndChangeCheck())
        {
            serializedObject.ApplyModifiedProperties();
            staticElement.SetDirty();
        }
    }
}
