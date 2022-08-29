using ScriptableObjectGraph.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectGraph.Editor
{
    public class AssetModificationsListener : AssetModificationProcessor
    {
        static string[] OnWillSaveAssets(string[] paths)
        {
            if (EditorWindow.HasOpenInstances<ScriptableGraphWindow>())
            {
                EditorWindow.GetWindow<ScriptableGraphWindow>().CheckDirty(paths);
            }

            return paths;
        }
    }

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
        BreadcrumbContainer _breadcrumbContainer;

        List<INodeContainerBase> _crumbTrail = new List<INodeContainerBase>();

        bool _guiCreated;

        string _title;

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
                window.SetTitle($"{((INodeContainerBase)asset).EditorWindowPrefix} Graph");
                window.SetAsset(window._nodeContainer);
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
                    var assetGuid = EditorPrefs.GetString("scriptable_window_container_guid");
                    var typeString = EditorPrefs.GetString("scriptable_window_container_type");
                    var assets = AssetDatabase.LoadAllAssetsAtPath(path);
                    foreach (var asset in assets)
                    {
                        if (assets == null || asset is not GuidScriptable guidScriptable) continue;

                        if (guidScriptable.GuidString == assetGuid)
                        {
                            SetTitle($"{((INodeContainerBase)asset).EditorWindowPrefix} Graph");
                            SetAsset(asset as INodeContainerBase);
                        }
                    }
                }
            }

            if (_inspector != null)
                _inspector.Clean();

            if (!_guiCreated)
            {
                if (_graphView != null)
                {
                    _graphView.OnNodeSelected += _inspector.NodeSelected;
                    _graphView.OnNodeUnselected += _inspector.NodeUnselected;
                }

                if (_breadcrumbContainer != null)
                {
                    SetBreadcrumbs();
                }
            }
        }

        private void OnDisable()
        {
            if (_nodeContainer != null)
            {
                var guidScriptable = (_nodeContainer as GuidScriptable);
                EditorPrefs.SetString("scriptable_window_container_path", AssetDatabase.GetAssetPath(guidScriptable));
                EditorPrefs.SetString("scriptable_window_container_guid", guidScriptable.GuidString);
                EditorPrefs.SetString("scriptable_window_container_type", _nodeContainer.GetType().FullName);
            }
            else
            {
                EditorPrefs.SetString("scriptable_window_container_guid", string.Empty);
                EditorPrefs.SetString("scriptable_window_container_path", string.Empty);
                EditorPrefs.SetString("scriptable_window_container_type", string.Empty);
            }
        }

        public void CreateGUI()
        {
            _guiCreated = true;

            var document = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(PackageRoot + "GraphWindow.uxml");
            document.CloneTree(rootVisualElement);

            var styles = AssetDatabase.LoadAssetAtPath<StyleSheet>(PackageRoot + "GraphWindow.uss");
            rootVisualElement.styleSheets.Add(styles);

            _breadcrumbContainer = rootVisualElement.Q<BreadcrumbContainer>();
            _breadcrumbContainer.BreadcrumbSelected += BreadcrumbSelected;

            _graphView = rootVisualElement.Q<ScriptableGraphView>();
            if (_nodeContainer != null)
            {
                SetAsset(_nodeContainer);
            }

            _splitView = rootVisualElement.Q<SplitView>();
            _inspectorPanel = _splitView.Q("right-panel");

            var size = EditorPrefs.GetFloat("scriptable_graph_panel_size", -1);
            if (size > -1)
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
            _graphView.OnSelectAsset += GraphAssetSelected;
            _graphView.OnAssetDirty += AssetDirty;
        }

        void SetAsset(INodeContainerBase nodeContainer)
        {
            _nodeContainer = nodeContainer;
            if (_graphView != null)
                _graphView.SetAsset(_nodeContainer);

            if (_breadcrumbContainer != null)
                SetBreadcrumbs();
        }

        void SetTitle(string title)
        {
            _title = title;
            titleContent = new GUIContent(title);
        }

        public void CheckDirty(string[] paths)
        {
            if (_nodeContainer != null && _graphView != null)
            {
                string assetPath = AssetDatabase.GetAssetPath(_nodeContainer as UnityEngine.Object);
                if (paths.Any(x => x == assetPath))
                {
                    AssetDirty(false);
                }
            }
        }

        #region Breadcrumbs
        void SetBreadcrumbs()
        {
            _crumbTrail.Clear();

            List<string> breadcrumbs = new List<string>();
            WriteBreadcrumbs(ref breadcrumbs, (UnityEngine.Object)_nodeContainer);

            _breadcrumbContainer.Breadcrumbs = breadcrumbs.ToArray();
        }

        void WriteBreadcrumbs(ref List<string> crumbs, UnityEngine.Object target)
        {
            crumbs.Insert(0, target.name);
            _crumbTrail.Insert(0, (INodeContainerBase)target);
            if (target is NodeBase node)
            {
                WriteBreadcrumbs(ref crumbs, (UnityEngine.Object)node.Parent);
            }
        }

        void BreadcrumbSelected(string name, int index, int crumbSize)
        {
            SetAsset(_crumbTrail[index]);
        }
        #endregion

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

        void GraphAssetSelected(INodeContainerBase container)
        {
            SetAsset(container);
        }

        private void AssetDirty(bool dirty)
        {
            titleContent = new GUIContent(_title + (dirty ? "*" : ""));
        }
        #endregion
    }
}
