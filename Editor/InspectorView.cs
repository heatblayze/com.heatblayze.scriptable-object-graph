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

        public InspectorView()
        {

        }

        public void NodeSelected(NodeView nodeView)
        {
            if (_editor != null && _currentNode == nodeView) return;

            Clean();

            _currentNode = nodeView;
            _editor = UnityEditor.Editor.CreateEditor(nodeView.Node);
            var container = new IMGUIContainer(_editor.OnInspectorGUI);

            Add(container);
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
