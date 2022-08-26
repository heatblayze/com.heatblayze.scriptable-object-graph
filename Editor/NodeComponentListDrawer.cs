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
        const int ArraySpacing = 10;
        const int SeparatorSpacing = 4;
        const int SeparatorSize = 1;

        SerializedProperty _components;

        void Init(SerializedProperty property)
        {
            _components = property.FindPropertyRelative("_components");
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            Init(property);

            float h = 0;
            for (int i = 0; i < _components.arraySize; i++)
            {
                var component = _components.GetArrayElementAtIndex(i);
                h += EditorGUI.GetPropertyHeight(component, true);
                h += ArraySpacing;
                h += SeparatorSpacing;
                h += SeparatorSize;
            }

            h += AddComponentButtonHeight;
            return h;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Init(property);

            float y = position.y;
            for (int i = 0; i < _components.arraySize; i++)
            {
                string labelName = null;
                var component = _components.GetArrayElementAtIndex(i);
                if (component.managedReferenceValue != null)
                {
                    var type = component.managedReferenceValue.GetType();
                    labelName = ObjectNames.NicifyVariableName(type.Name);
                }
                else
                {
                    labelName = "Nullreference";
                }

                position.height = EditorGUI.GetPropertyHeight(component, true);
                EditorGUI.PropertyField(position, component, new GUIContent(labelName), true);

                position.y += position.height;

                position.y += SeparatorSpacing;
                position.height = SeparatorSize;
                EditorGUI.DrawRect(position, Color.black);

                position.y += ArraySpacing;
            }

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
                        menu.AddItem(new GUIContent(typeName), false, AddComponent, type);
                    }
                }
                else
                {
                    menu.AddItem(new GUIContent("<empty>"), false, null);
                }

                menu.ShowAsContext();
            }
        }

        void AddComponent(object typeObj)
        {
            var index = _components.arraySize;
            _components.InsertArrayElementAtIndex(index);
            var elem = _components.GetArrayElementAtIndex(index);
            var obj = Activator.CreateInstance((Type)typeObj);
            elem.managedReferenceValue = obj;

            _components.serializedObject.ApplyModifiedProperties();
        }
    }
}
