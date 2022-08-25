using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using ScriptableObjectGraph.Core;

namespace ScriptableObjectGraph.Editor
{
    [CustomEditor(typeof(NodeBase), true)]
    public class NodeBaseEditor : UnityEditor.Editor
    {
        SerializedProperty _guid;

        private void OnEnable()
        {
            _guid = serializedObject.FindProperty("_guid");
        }

        public override void OnInspectorGUI()
        {
            var dWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            serializedObject.Update();

            EditorGUILayout.TextField(new GUIContent("Guid", "Read-only"), _guid.stringValue);
            EditorGUILayout.Space();

            DrawPropertiesExcluding(serializedObject, "m_Script", "_guid",
                "Position", "_connections", "_children", "_entryNode");

            serializedObject.ApplyModifiedProperties();

            EditorGUIUtility.labelWidth = dWidth;
        }
    }
}
