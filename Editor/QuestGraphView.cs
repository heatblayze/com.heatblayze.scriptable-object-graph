using QuestGraph.Core;
using QuestGraph.Editor.Internal;
using QuestGraph.Internal;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace QuestGraph.Editor
{
    public class QuestGraphView : GraphView
    {
        public NodeContainerBase Asset { get; private set; }

        public QuestGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            // TODO: implement my own SelectionDragger to allow undo/redo of content position
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer());

            styleSheets.Add((StyleSheet)EditorGUIUtility.Load(QuestGraphWindow.PackageRoot + "GraphStyles.uss"));
        }

        public void SetAsset(UnityEngine.Object asset)
        {
            Asset = asset as NodeContainerBase;
            PopulateView();
        }

        public void PopulateView()
        {
            DeleteElements(graphElements);

            var factory = NodeViewFactoryCache.GetFactory(Asset.GetNodeType());
            foreach (var node in Asset.GetNodesInternal())
            {
                CreateNodeView(factory, node);
            }
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (Asset == null)
                return;

            bool createdAnyAction = false;
            {
                var nodeTypes = TypeCache.GetTypesDerivedFrom(Asset.GetNodeType());
                foreach (var type in nodeTypes)
                {
                    var factory = NodeViewFactoryCache.GetFactory(type);
                    if (factory != null)
                    {
                        createdAnyAction = true;
                        evt.menu.AppendAction($"{factory.ContextMenuName}", (a) =>
                        {
                            CreateNode(factory, type, a.eventInfo.localMousePosition);
                        });
                    }
                }
            }

            // The root node type
            {
                var type = Asset.GetNodeType();
                var factory = NodeViewFactoryCache.GetFactory(type);
                if (factory != null)
                {
                    createdAnyAction = true;
                    evt.menu.AppendAction($"{factory.ContextMenuName}", (a) =>
                    {
                        CreateNode(factory, type, a.eventInfo.localMousePosition);
                    });
                }
            }

            if (!createdAnyAction)
            {
                evt.menu.AppendAction("<empty>", null, DropdownMenuAction.Status.Disabled);
            }
        }

        void CreateNode(NodeViewFactoryBase factory, System.Type type, Vector2 position)
        {
            var node = Asset.CreateNode(type);
            node.Position = position;
            CreateNodeView(factory, node);
        }

        void CreateNodeView(NodeViewFactoryBase factory, NodeBase node)
        {
            var nodeView = factory.CreateNodeView(node);
            nodeView.SetPosition(new Rect(node.Position, Vector2.zero));
            AddElement(nodeView);
        }
    }
}
