using UnityEditor;
using UnityEngine;

namespace ScriptableObjectGraph.Editor
{
    [CustomEditor(typeof(NodeBase), true)]
    public class NodeBaseEditor : UnityEditor.Editor
    {
        SerializedProperty _guid;
        SerializedProperty _components;
        SerializedProperty _script;
        GUIStyle _headerStyle;

        private void OnEnable()
        {
            _guid = serializedObject.FindProperty("_guid");
            _components = serializedObject.FindProperty("_components");
            _script = serializedObject.FindProperty("m_Script");


            _headerStyle = new GUIStyle(EditorStyles.largeLabel);
            _headerStyle.fontStyle = FontStyle.Bold;
        }

        public override void OnInspectorGUI()
        {
            var dWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = 100;

            serializedObject.Update();

            EditorGUI.BeginChangeCheck();
            string name = EditorGUILayout.TextField(new GUIContent("Name"), target.name);
            if (EditorGUI.EndChangeCheck())
            {
                target.name = name;
            }

            using (var scope = new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.TextField(new GUIContent("Guid", "Read-only"), _guid.stringValue);
                EditorGUILayout.PropertyField(_script);
            }

            DrawPropertiesExcluding(serializedObject, "_guid", "_components", "m_Script");

            EditorGUILayout.Space();

            //EditorGUILayout.LabelField(new GUIContent("Components"), _headerStyle);
            EditorGUILayout.PropertyField(_components);

            serializedObject.ApplyModifiedProperties();

            EditorGUIUtility.labelWidth = dWidth;
        }
    }
}
