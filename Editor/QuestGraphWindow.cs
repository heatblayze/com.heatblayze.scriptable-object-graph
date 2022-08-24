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
    public class QuestGraphWindow : EditorWindow
    {
        public const string PackageRoot = "Packages/com.heatblayze.quest-graph/Editor/";

        QuestGraphView _graphView;

        #region Static

        [MenuItem("RPG Tools/Quest Graph")]
        public static QuestGraphWindow OpenQuestGraphEditor()
        {
            var window = GetWindow<QuestGraphWindow>();
            window.titleContent = new GUIContent("Quest Graph");
            return window;
        }

        [OnOpenAsset]
        public static bool OpenAsset(int instanceId, int line)
        {
            var asset = EditorUtility.InstanceIDToObject(instanceId);

            // Only allow node containers to be selected
            if (typeof(NodeContainerBase).IsAssignableFrom(asset.GetType()))
            {
                var window = OpenQuestGraphEditor();
                window._graphView.SetAsset(asset);
                return true;
            }
            return false;
        }

        #endregion

        private void OnEnable()
        {
            _graphView = new QuestGraphView();

            _graphView.StretchToParentSize();
            rootVisualElement.Add(_graphView);
        }

        private void OnDisable()
        {
            rootVisualElement.Remove(_graphView);
        }
    }
}
