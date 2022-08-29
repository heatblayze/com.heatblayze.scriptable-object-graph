using ScriptableObjectGraph;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ScriptableObjectGraph.Editor
{
    [CustomPropertyDrawer(typeof(NodeComponentList))]
    public class NodeComponentListDrawer : PropertyDrawer
    {
        const int AddComponentButtonHeight = 20;
        SerializedProperty _components;

        void Init(SerializedProperty property)
        {
            _components = property.FindPropertyRelative("_components");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);

            float h = EditorGUI.GetPropertyHeight(_components, true);
            h += AddComponentButtonHeight;
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            var height = EditorGUI.GetPropertyHeight(_components, true);
            position.height = height;
            EditorGUI.PropertyField(position, _components, label);

            position.y += height;

            position.height = AddComponentButtonHeight;
            if (EditorGUI.DropdownButton(position, new GUIContent("Add Component"), FocusType.Passive))
            {
                GenericMenu menu = new GenericMenu();

                var types = TypeCache.GetTypesDerivedFrom<NodeComponent>();
                if (types.Count > 0)
                {
                    foreach (var type in types)
                    {
                        var typeName = ObjectNames.NicifyVariableName(type.Name);
                        menu.AddItem(new GUIContent(typeName), false, AddComponent, new Tuple<Type, SerializedProperty>(type, property));
                    }
                }
                else
                {
                    menu.AddItem(new GUIContent("<empty>"), false, null);
                }

                menu.ShowAsContext();
            }
        }

        void AddComponent(object container)
        {
            var data = container as Tuple<Type, SerializedProperty>;

            var index = _components.arraySize;
            _components.InsertArrayElementAtIndex(index);
            var elem = _components.GetArrayElementAtIndex(index);
            var obj = (NodeComponent)Activator.CreateInstance(data.Item1);
            obj.ParentNode = data.Item2.serializedObject.targetObject as NodeBase;
            elem.managedReferenceValue = obj;
            elem.FindPropertyRelative("m_Name").stringValue = data.Item1.Name;

            _components.serializedObject.ApplyModifiedProperties();
        }
    }
}
