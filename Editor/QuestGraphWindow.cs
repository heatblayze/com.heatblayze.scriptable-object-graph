using System.Collections;
using System.Collections.Generic;
using UnityEditor;
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
        public static void OpenQestGraphEditor()
        {
            var window = GetWindow<QuestGraphWindow>();
            window.titleContent = new GUIContent("Quest Graph");
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
