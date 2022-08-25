using ScriptableObjectGraph.Core;
using ScriptableObjectGraph.Editor.Internal;
using ScriptableObjectGraph.Internal;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectGraph.Editor
{    
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
            // TODO: implement my own SelectionDragger to allow undo/redo of content position
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            this.AddManipulator(new ContentZoomer());

            styleSheets.Add((StyleSheet)EditorGUIUtility.Load(ScriptableGraphWindow.PackageRoot + "GraphStyles.uss"));

            graphViewChanged += OnChanges;
            Undo.undoRedoPerformed += OnUndo;
        }

        void OnUndo()
        {
            NodeBase selectedNode = _selectedNode?.Node;

            if (Asset != null)
                PopulateView();

            if(selectedNode != null)
            {
                this.AddToSelection(_nodeDictionary[selectedNode]);
            }
        }

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

            if(graphViewChange.elementsToRemove != null)
            {
                graphViewChange.elementsToRemove.ForEach(element =>
                {
                    if (element is Edge edge)
                    {
                        var parentNodeView = _portDictionary[edge.output];
                        var childNodeView = _portDictionary[edge.input];

                        parentNodeView.RemoveEdge(edge, childNodeView);
                    }
                    else if(element is NodeView nodeView)
                    {
                        // TODO: hmmm..... might need to rework this system a little to allow undo/redo deletion stuff
                    }
                });
            }

            return graphViewChange;
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
            return _nodeDictionary[item];
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
        void CreateNode(NodeViewFactoryBase factory, System.Type type, Vector2 position)
        {
            var node = Asset.CreateNode(type);
            node.Initialize(Asset);
            node.Position = position;
            CreateNodeView(factory, node);
        }

        void CreateNodeView(NodeViewFactoryBase factory, NodeBase node)
        {
            var nodeView = factory.CreateNodeView(node);
            nodeView.Parent = this;
            nodeView.SetPosition(new Rect(node.Position, Vector2.zero));
            
            _nodeDictionary.Add(node, nodeView);
            _nodes.Add(nodeView);

            AddElement(nodeView);

            var inputs = nodeView.CreateInputPorts();
            var outputs = nodeView.CreateOutputPorts();
            foreach(var input in inputs) _portDictionary.Add(input, nodeView);
            foreach(var output in outputs) _portDictionary.Add(output, nodeView);            

            nodeView.RefreshPorts();
            nodeView.RefreshExpandedState();

            nodeView.OnNodeSelected += NodeSelected;
            nodeView.OnNodeUnselected += NodeUnselected;
        }

        #endregion
    }
}
