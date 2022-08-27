using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

namespace ScriptableObjectGraph.Editor
{
    public class InspectorView : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<InspectorView, VisualElement.UxmlTraits> { }

        UnityEditor.Editor _editor;
        NodeView _currentNode;
        IMGUIContainer _container;

        public InspectorView()
        {

        }

        public void NodeSelected(NodeView nodeView)
        {
            if (_editor != null && _currentNode == nodeView) return;

            Clean();

            _currentNode = nodeView;
            _editor = UnityEditor.Editor.CreateEditor(nodeView.Node);
            _container = new IMGUIContainer(() =>
            {
                using(var scope = new EditorGUI.ChangeCheckScope())
                {
                    _editor.OnInspectorGUI();

                    if (scope.changed)
                    {
                        if (_currentNode != null)
                            _currentNode.UpdateContents();
                    }
                }
            });

            Add(_container);
        }

        public void NodeUnselected(NodeView nodeView)
        {
            if (_currentNode != nodeView) return;
            Clean();
        }

        public void Clean()
        {
            Clear();
            if (_editor != null)
            {
                Object.DestroyImmediate(_editor);
            }
            _currentNode = null;
        }
    }
}
