using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ScriptableObjectGraph;

namespace ScriptableObjectGraph.Editor
{
    [CustomEditor(typeof(NodeBase), true)]
    public class NodeBaseEditor : UnityEditor.Editor
    {
        SerializedProperty _guid;
        SerializedProperty _components;
        GUIStyle _headerStyle;

        private void OnEnable()
        {
            _guid = serializedObject.FindProperty("_guid");
            _components = serializedObject.FindProperty("_components");
            _headerStyle = new GUIStyle(EditorStyles.largeLabel);
            _headerStyle.fontStyle = FontStyle.Bold;
        }

        public override void OnInspectorGUI()
        {
            var dWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            string name = EditorGUILayout.TextField(new GUIContent("Guid", "Read-only"), target.name);
            if(EditorGUI.EndChangeCheck())
            {
                target.name = name;
            }

            EditorGUILayout.TextField(new GUIContent("Guid", "Read-only"), _guid.stringValue);
            EditorGUILayout.Space();

            DrawPropertiesExcluding(serializedObject, "m_Script", "_guid",
                "Position", "_ports", "_children", "_entryNode", "_components");

            EditorGUILayout.Space();            

            EditorGUILayout.LabelField(new GUIContent("Components"), _headerStyle);
            EditorGUILayout.PropertyField(_components);

            serializedObject.ApplyModifiedProperties();

            EditorGUIUtility.labelWidth = dWidth;
        }
    }
}
