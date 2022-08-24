using ScriptableObjectGraph.Internal;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectGraph.Editor
{
    public class CustomGraphWindow : EditorWindow
    {
        public const string PackageRoot = "Packages/com.heatblayze.scriptable-object-graph/Editor/";

        CustomGraphView _graphView;
        INodeContainerBase _nodeContainer;

        #region Static

        static CustomGraphWindow OpenGraphWindow()
        {
            var window = GetWindow<CustomGraphWindow>();
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
            _graphView = new CustomGraphView();
            if (_nodeContainer != null)
                _graphView.SetAsset(_nodeContainer);

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void OnDisable()
        {
            if (_graphView != null)
                rootVisualElement.Remove(_graphView);
        }
    }
}
