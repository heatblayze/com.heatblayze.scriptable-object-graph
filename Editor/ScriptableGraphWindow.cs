using ScriptableObjectGraph.Internal;
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectGraph.Editor
{
    public class ScriptableGraphWindow : EditorWindow
    {
        public const string PackageRoot = "Packages/com.heatblayze.scriptable-object-graph/Editor/";

        public event Action<NodeView> OnNodeSelected;
        public event Action<NodeView> OnNodeUnselected;

        INodeContainerBase _nodeContainer;

        ScriptableGraphView _graphView;
        InspectorView _inspector;
        VisualElement _inspectorPanel;
        SplitView _splitView;

        bool _guiCreated;

        #region Static

        static ScriptableGraphWindow OpenGraphWindow()
        {
            var window = GetWindow<ScriptableGraphWindow>();
            return window;
        }

        [OnOpenAsset]
        public static bool OpenAsset(int instanceId, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);

            // Only allow node containers to be selected
            if (typeof(INodeContainerBase).IsAssignableFrom(asset.GetType()))
            {
                var window = OpenGraphWindow();
                window._nodeContainer = asset as INodeContainerBase;
                if(window._graphView != null)
                    window._graphView.SetAsset(window._nodeContainer);

                window.titleContent = new GUIContent($"{((INodeContainerBase)asset).EditorWindowPrefix} Graph");
                return true;
            }
            return false;
        }

        #endregion

        private void OnEnable()
        {
            if (_nodeContainer == null)
            {
                var path = EditorPrefs.GetString("scriptable_window_container_path");
                if (!string.IsNullOrEmpty(path))
                {
                    var typeString = EditorPrefs.GetString("scriptable_window_container_type");
                    var asset = AssetDatabase.LoadAssetAtPath(path, typeof(INodeContainerBase));
                    if(asset != null && typeof(INodeContainerBase).IsAssignableFrom(asset.GetType()))
                    {
                        _nodeContainer = asset as INodeContainerBase;
                        if (_graphView != null)
                            _graphView.SetAsset(_nodeContainer);

                        titleContent = new GUIContent($"{((INodeContainerBase)asset).EditorWindowPrefix} Graph");
                    }
                }
            }

            if (_inspector != null)
                _inspector.Clean();

            if(_graphView != null && !_guiCreated)
            {
                _graphView.OnNodeSelected += _inspector.NodeSelected;
                _graphView.OnNodeUnselected += _inspector.NodeUnselected;
            }
        }

        private void OnDisable()
        {
            if(_nodeContainer != null)
            {
                EditorPrefs.SetString("scriptable_window_container_path", AssetDatabase.GetAssetPath(_nodeContainer as UnityEngine.Object));
                EditorPrefs.SetString("scriptable_window_container_type", _nodeContainer.GetType().FullName);
            }
        }

        public void CreateGUI()
        {
            _guiCreated = true;

            var document = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PackageRoot + "GraphWindow.uxml");
            document.CloneTree(rootVisualElement);

            var styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(PackageRoot + "GraphWindow.uss");
            rootVisualElement.styleSheets.Add(styles);

            _graphView = rootVisualElement.Q<ScriptableGraphView>();
            if (_nodeContainer != null)
                _graphView.SetAsset(_nodeContainer);

            _splitView = rootVisualElement.Q<SplitView>();
            _inspectorPanel = _splitView.Q("right-panel");

            var size = EditorPrefs.GetFloat("scriptable_graph_panel_size", -1);
            if(size > -1)
            {
                _splitView.fixedPaneInitialDimension = size;
            }

            _inspectorPanel.RegisterCallback<GeometryChangedEvent>(SplitViewChange);

            _inspector = rootVisualElement.Q<InspectorView>();
            _inspector.Clean();

            OnNodeSelected += _inspector.NodeSelected;
            OnNodeUnselected += _inspector.NodeUnselected;

            _graphView.OnNodeSelected += NodeSelected;
            _graphView.OnNodeUnselected += NodeUnselected;

        }

        #region Callbacks
        void SplitViewChange(GeometryChangedEvent evt)
        {
            EditorPrefs.SetFloat("scriptable_graph_panel_size", evt.newRect.width);
        }

        void NodeSelected(NodeView node)
        {
            OnNodeSelected?.Invoke(node);
        }

        void NodeUnselected(NodeView node)
        {
            OnNodeUnselected?.Invoke(node);
        }
        #endregion
    }
}
