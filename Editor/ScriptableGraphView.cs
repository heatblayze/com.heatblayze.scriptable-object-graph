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

        public List<Node> NodeViews => _nodes;

        public event Action<NodeView> OnNodeSelected;
        public event Action<NodeView> OnNodeUnselected;
        public event Action<INodeContainerBase> OnSelectAsset;

        public EntryNodeView EntryNode { get; private set; }
        public ExitNodeView ExitNode { get; private set; }

        public NodeView SelectedNode => _selectedNode;
        NodeView _selectedNode;

        Dictionary<NodeBase, NodeView> _nodeDictionary = new Dictionary<NodeBase, NodeView>();
        Dictionary<Port, Node> _portDictionary = new Dictionary<Port, Node>();
        List<Node> _nodes = new List<Node>();

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
            Undo.RegisterCompleteObjectUndo((ScriptableObject)Asset, "DeleteNode");

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

        void NodeDoubleClick(NodeView node)
        {
            if (node.Node is INodeContainerBase container)
            {
                OnSelectAsset(container);
            }
        }

        void SelectParentAsset()
        {
            if (Asset is NodeBase node)
            {
                OnSelectAsset((INodeContainerBase)node.Parent);
            }
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
                    var parentNode = _portDictionary[edge.output];
                    var childNode = _portDictionary[edge.input];

                    if (parentNode is NodeView nodeView)
                        nodeView.ConnectChild(edge, childNode);
                    else if (parentNode is EntryNodeView entryNode)
                        entryNode.SetEntryNode(childNode);
                });
            }

            if (graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(element =>
                {
                    if (element is Edge edge)
                    {
                        var parentNode = _portDictionary[edge.output];
                        var childNode = _portDictionary[edge.input];

                        if (parentNode is NodeView nodeView)
                            nodeView.RemoveEdge(edge, childNode);
                    }
                    else if (element is NodeView nodeView)
                    {
                        DeleteNode(nodeView.Node);
                    }
                });
            }

            if (graphViewChange.movedElements != null)
            {
                Undo.IncrementCurrentGroup();
                foreach (var item in graphViewChange.movedElements)
                {
                    if (item is NodeView nodeView)
                    {
                        Undo.RecordObject(nodeView.Node, "Move node");
                        nodeView.Node.Position = nodeView.GetPosition().position;
                    }
                    else if (item is EntryNodeView entryNode)
                    {
                        Asset.EntryNodePosition = entryNode.GetPosition().position;
                    }
                    else if (item is ExitNodeView exitNodeView)
                    {
                        Asset.ExitNodePosition = exitNodeView.GetPosition().position;
                    }
                }

                Undo.SetCurrentGroupName("Move node(s)");
            }

            return graphViewChange;
        }

        string SerializeData(IEnumerable<GraphElement> elements)
        {
            var nodeDic = new Dictionary<NodeBase, NodeBase>();
            foreach (var element in elements)
            {
                if (element is NodeView nodeView)
                {
                    nodeDic.Add(nodeView.Node, nodeView.Node.Clone());
                }
            }

            foreach (var node in nodeDic)
            {
                for (int i = 0; i < node.Key.Ports.Length; i++)
                {
                    foreach (var connection in node.Key.Ports[i].Connections)
                    {
                        if (connection.Node == null) continue;
                        if (nodeDic.TryGetValue(connection.Node, out NodeBase otherNode))
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

            CreateEntryExitNodes();

            foreach (var node in Asset.GetNodesInternal())
            {
                var factory = NodeViewFactoryCache.GetFactory(node.GetType());
                CreateNodeView(factory, node);
            }

            var entryDataNode = Asset.GetEntryNode();
            if (entryDataNode != null)
            {
                var entryNodeView = _nodeDictionary[entryDataNode];
                EntryNode.Connect(entryNodeView);
            }

            foreach (var node in _nodes)
            {
                if (node is NodeView nodeView)
                    nodeView.ConnectOutputs();
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
                        evt.menu.AppendAction($"Add {factory.ContextMenuName}", (a) =>
                        {
                            CreateNodeContext(a, factory, type);
                        });
                    }
                }
            }

            // The root node type
            {
                var type = Asset.GetNodeType();
                var factory = NodeViewFactoryCache.GetFactory(type);
                if (factory != null && !type.IsAbstract)
                {
                    createdAnyAction = true;
                    evt.menu.AppendAction($"Add {factory.ContextMenuName}", (a) =>
                    {
                        CreateNodeContext(a, factory, type);
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
            var node = _portDictionary[startPort];
            if (node is NodeView nodeView)
            {
                return nodeView.GetCompatiblePorts(startPort);
            }
            else if (node is EntryNodeView entryNodeView)
            {
                return _portDictionary.Values.Where(x => x is NodeView).Select(x => ((NodeView)x).InputPorts[0]).ToList();
            }
            else if (node is ExitNodeView exitNodeView)
            {
                return _portDictionary.Values.Where(x => x is NodeView).SelectMany(x => ((NodeView)x).OutputPorts).ToList();
            }
            return null;
        }

        #endregion

        #region Node Creation
        void CreateEntryExitNodes()
        {
            EntryNode = new EntryNodeView();
            EntryNode.Parent = this;
            ExitNode = new ExitNodeView();
            ExitNode.Parent = this;

            EntryNode.SetPosition(new Rect(Asset.EntryNodePosition, Vector2.zero));
            ExitNode.SetPosition(new Rect(Asset.ExitNodePosition, Vector2.zero));

            EntryNode.RefreshPorts();
            EntryNode.RefreshExpandedState();
            ExitNode.RefreshPorts();
            ExitNode.RefreshExpandedState();

            AddElement(EntryNode);
            AddElement(ExitNode);

            _portDictionary.Add(EntryNode.Port, EntryNode);
            _portDictionary.Add(ExitNode.Port, ExitNode);

            _nodes.Add(EntryNode);
            _nodes.Add(ExitNode);

            EntryNode.OnDoubleClick += SelectParentAsset;
            ExitNode.OnDoubleClick += SelectParentAsset;
        }

        void CreateNodeContext(DropdownMenuAction a, INodeViewFactory factory, Type type)
        {
            var nodeView = CreateNode(factory, type, viewTransform.matrix.inverse.MultiplyPoint(a.eventInfo.localMousePosition));
            if (Asset.GetEntryNode() == nodeView.Node)
            {
                EntryNode.Connect(nodeView);
            }
        }

        NodeView InsertNode(INodeViewFactory factory, NodeBase node, Vector2 position)
        {
            node.Position = position;

            Undo.RegisterCompleteObjectUndo((ScriptableObject)Asset, "Add node");
            Asset.AddNode(node);

            AssetDatabase.AddObjectToAsset(node, (ScriptableObject)Asset);
            AssetDatabase.SaveAssets();

            Undo.RegisterCreatedObjectUndo(node, "Create node");

            return CreateNodeView(factory, node);
        }

        NodeView CreateNode(INodeViewFactory factory, Type type, Vector2 position)
        {
            Undo.IncrementCurrentGroup();
            var node = Asset.CreateNode(type);
            node.Initialize(Asset);
            var nodeView = InsertNode(factory, node, position);

            Undo.SetCurrentGroupName("Create Node");
            return nodeView;
        }

        NodeView CreateNodeView(INodeViewFactory factory, NodeBase node)
        {
            var nodeView = factory.Create(node);
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
            nodeView.OnNodeDoubleClick += NodeDoubleClick;

            return nodeView;
        }

        #endregion
    }
}
