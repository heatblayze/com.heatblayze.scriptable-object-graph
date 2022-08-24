using QuestGraph.Core;
using QuestGraph.Internal;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuestGraph.Editor
{
    public class CustomGraphWindow : EditorWindow
    {
        public const string PackageRoot = "Packages/com.heatblayze.quest-graph/Editor/";

        CustomGraphView _graphView;

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
            if (typeof(NodeContainerBase).IsAssignableFrom(asset.GetType()))
            {
                var window = OpenGraphWindow();
                window._graphView.SetAsset(asset);
                window.titleContent = new GUIContent($"{((NodeContainerBase)asset).EditorWindowPrefix} Graph");
                return true;
            }
            return false;
        }

        #endregion

        private void OnEnable()
        {
            _graphView = new CustomGraphView();

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }
}
