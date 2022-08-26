using ScriptableObjectGraph.Editor.Internal;
using ScriptableObjectGraph.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectGraph.Editor
{
    [System.Serializable]
    public class CopyContainer
    {
        public List<NodeBase> elements = new List<NodeBase>();
    }

    public class ScriptableGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<ScriptableGraphView, GraphView.UxmlTraits> { }

        public INodeContainerBase Asset { get => _asset; set => _asset = value; }
        [SerializeField]
        INodeContainerBase _asset;

        public List<NodeView> NodeViews => _nodes;

        public event Action<NodeView> OnNodeSelected;
        public event Action<NodeView> OnNodeUnselected;

        NodeView _selectedNode;

        Dictionary<NodeBase, NodeView> _nodeDictionary = new Dictionary<NodeBase, NodeView>();
        Dictionary<Port, NodeView> _portDictionary = new Dictionary<Port, NodeView>();
        List<NodeView> _nodes = new List<NodeView>();

        bool _clear;

        public ScriptableGraphView()
        {
            Insert(0, new GridBackground());

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer());            

            styleSheets.Add((StyleSheet)EditorGUIUtility.Load(ScriptableGraphWindow.PackageRoot + "GraphStyles.uss"));

            graphViewChanged += OnChanges;
            serializeGraphElements += SerializeData;
            unserializeAndPaste += UnserializeAndPaste;

            Undo.undoRedoPerformed += OnUndo;
        }

        void OnUndo()
        {
            AssetDatabase.SaveAssets();

            NodeBase selectedNode = _selectedNode?.Node;

            if (Asset != null)
                PopulateView();

            if (selectedNode != null)
            {
                this.AddToSelection(_nodeDictionary[selectedNode]);
            }
        }

        #region Asset Management
        void DeleteNode(NodeBase node)
        {
            if (node == null) return;
            if (!Asset.GetNodesInternal().Contains(node)) return;
            Undo.RecordObject((ScriptableObject)Asset, "DeleteNode");

            Asset.DeleteNode(node);

            Undo.DestroyObjectImmediate(node);

            Undo.CollapseUndoOperations(Undo.GetCurrentGroup());

            AssetDatabase.SaveAssets();
        }
        #endregion

        #region Node Callbacks
        void NodeSelected(NodeView node)
        {
            _selectedNode = node;
            OnNodeSelected?.Invoke(node);
        }

        void NodeUnselected(NodeView node)
        {
            if (_selectedNode == node)
                _selectedNode = null;

            OnNodeUnselected?.Invoke(node);
        }
        #endregion

        #region Graph Callbacks
        GraphViewChange OnChanges(GraphViewChange graphViewChange)
        {
            if (_clear)
            {
                _clear = false;
                return graphViewChange;
            }

            if (graphViewChange.edgesToCreate != null)
            {
                graphViewChange.edgesToCreate.ForEach(edge =>
                {
                    var parentNodeView = _portDictionary[edge.output];
                    var childNodeView = _portDictionary[edge.input];

                    parentNodeView.ConnectChild(edge, childNodeView);
                });
            }

            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(element =>
                {
                    if (element is Edge edge)
                    {
                        var parentNodeView = _portDictionary[edge.output];
                        var childNodeView = _portDictionary[edge.input];

                        parentNodeView.RemoveEdge(edge, childNodeView);
                    }
                    else if (element is NodeView nodeView)
                    {
                        DeleteNode(nodeView.Node);
                    }
                });
            }

            if(graphViewChange.movedElements != null)
            {
                Undo.IncrementCurrentGroup();
                foreach(var item in graphViewChange.movedElements)
                {
                    if(item is NodeView nodeView)
                    {
                        Undo.RecordObject(nodeView.Node, "Move node");
                        nodeView.Node.Position = nodeView.GetPosition().position;
                    }
                }

                Undo.SetCurrentGroupName("Move node(s)");
            }

            return graphViewChange;
        }

        string SerializeData(IEnumerable<GraphElement> elements)
        {
            var nodeDic = new Dictionary<NodeBase, NodeBase>();
            foreach(var element in elements)
            {
                if (element is NodeView nodeView)
                {
                    nodeDic.Add(nodeView.Node, nodeView.Node.Clone());
                }
            }

            foreach(var node in nodeDic)
            {
                for (int i = 0; i < node.Key.Ports.Length; i++)
                {
                    foreach(var connection in node.Key.Ports[i].Connections)
                    {
                        if (connection.Node == null) continue;
                        if(nodeDic.TryGetValue(connection.Node, out NodeBase otherNode))
                        {
                            node.Value.Ports[i].Connections.Add(new NodeConnection()
                            {
                                Node = otherNode,
                                PortIndex = connection.PortIndex
                            });
                        }
                    }
                }
            }

            var container = new CopyContainer();
            container.elements = nodeDic.Values.ToList();
            var str = JsonUtility.ToJson(container);
            return str;
        }

        void UnserializeAndPaste(string operationName, string data)
        {
            var elements = JsonUtility.FromJson<CopyContainer>(data);
            var createdNodeViews = new List<NodeView>();

            if (elements.elements != null && elements.elements.Count > 0)
            {
                Undo.IncrementCurrentGroup();

                foreach (var element in elements.elements)
                {
                    var factory = NodeViewFactoryCache.GetFactory(element.GetType());
                    createdNodeViews.Add(InsertNode(factory, element, element.Position));
                }

                Undo.SetCurrentGroupName("Paste nodes");

                ClearSelection();

                foreach (var node in createdNodeViews)
                {
                    node.ConnectOutputs();
                    AddToSelection(node);
                }
            }
        }
        #endregion

        #region Public Methods

        public void SetAsset(INodeContainerBase asset)
        {
            Asset = asset;
            PopulateView();
        }

        public void PopulateView()
        {
            _clear = true;

            DeleteElements(graphElements);
            _nodeDictionary.Clear();
            _portDictionary.Clear();
            _nodes.Clear();

            var factory = NodeViewFactoryCache.GetFactory(Asset.GetNodeType());
            foreach (var node in Asset.GetNodesInternal())
            {
                CreateNodeView(factory, node);
            }

            foreach (var node in _nodes)
            {
                node.ConnectOutputs();
            }
        }

        public NodeView Find(NodeBase item)
        {
            if (_nodeDictionary.ContainsKey(item))
                return _nodeDictionary[item];
            return null;
        }

        public void ConnectPorts(Port parent, Port child)
        {
            var edge = parent.ConnectTo(child);
            AddElement(edge);
        }

        #endregion

        #region Overrides

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

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var nodeView = _portDictionary[startPort];
            return nodeView.GetCompatiblePorts(startPort);
        }

        #endregion

        #region Node Creation
        NodeView InsertNode(NodeViewFactoryBase factory, NodeBase node, Vector2 position)
        {
            node.Position = position;

            Undo.RecordObject((ScriptableObject)Asset, "Add node");
            Asset.AddNode(node);

            //Undo.RegisterCompleteObjectUndo(node, "Insert node asset");
            AssetDatabase.AddObjectToAsset(node, (ScriptableObject)Asset);
            AssetDatabase.SaveAssets();

            Undo.RegisterCreatedObjectUndo(node, "Create node");


            return CreateNodeView(factory, node);
        }

        NodeView CreateNode(NodeViewFactoryBase factory, Type type, Vector2 position)
        {
            Undo.IncrementCurrentGroup();
            var node = Asset.CreateNode(type);
            node.Initialize(Asset);
            var nodeView = InsertNode(factory, node, position);

            Undo.SetCurrentGroupName("Create Node");
            return nodeView;
        }

        NodeView CreateNodeView(NodeViewFactoryBase factory, NodeBase node)
        {
            var nodeView = factory.CreateNodeView(node);
            nodeView.Parent = this;
            nodeView.SetPosition(new Rect(node.Position, Vector2.zero));

            _nodeDictionary.Add(node, nodeView);
            _nodes.Add(nodeView);

            AddElement(nodeView);

            var inputs = nodeView.CreateInputPorts();
            var outputs = nodeView.CreateOutputPorts();
            foreach (var input in inputs) _portDictionary.Add(input, nodeView);
            foreach (var output in outputs) _portDictionary.Add(output, nodeView);

            nodeView.RefreshPorts();
            nodeView.RefreshExpandedState();

            nodeView.OnNodeSelected += NodeSelected;
            nodeView.OnNodeUnselected += NodeUnselected;

            return nodeView;
        }

        #endregion
    }
}
