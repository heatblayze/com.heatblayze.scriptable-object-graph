using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectGraph.Editor
{
    public abstract class NodeView : Node
    {
        public NodeBase Node;
        public ScriptableGraphView Parent;

        public event Action<NodeView> OnNodeSelected;
        public event Action<NodeView> OnNodeUnselected;

        public event Action<NodeView> OnNodeDoubleClick;

        public Port[] InputPorts;
        public Port[] OutputPorts;

        public virtual int InputPortCount => 1;

        DateTime _clickTime;

        public NodeView(NodeBase node)
        {
            Node = node;

            this.RegisterCallback<PointerDownEvent>(MouseDown, TrickleDown.TrickleDown);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            OnNodeSelected?.Invoke(this);
        }

        public override void OnUnselected()
        {
            base.OnUnselected();
            OnNodeUnselected?.Invoke(this);
        }

        void MouseDown(PointerDownEvent evt)
        {
            if (evt.clickCount >= 2)
            {
                OnNodeDoubleClick?.Invoke(this);
            }
            _clickTime = DateTime.Now;
        }

        #region Ports
        public virtual Port[] CreateInputPorts()
        {
            InputPorts = new Port[InputPortCount];
            for (int i = 0; i < InputPortCount; i++)
            {
                InputPorts[i] = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
                InputPorts[i].portName = "";
                inputContainer.Add(InputPorts[i]);
            }
            return InputPorts;
        }

        public virtual Port[] CreateOutputPorts()
        {
            OutputPorts = new Port[Node.Ports.Length];
            for (int i = 0; i < Node.Ports.Length; i++)
            {
                OutputPorts[i] = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
                OutputPorts[i].portName = "";
                outputContainer.Add(OutputPorts[i]);
            }
            return OutputPorts;
        }

        public virtual void ConnectOutputs()
        {
            for (int i = 0; i < Node.Ports.Length; i++)
            {
                foreach (var connection in Node.Ports[i].Connections)
                {
                    var otherNodeView = Parent.Find(connection.Node);
                    if (otherNodeView != null)
                        Parent.ConnectPorts(OutputPorts[i], otherNodeView.InputPorts[connection.PortIndex]);
                }
            }
        }

        public virtual List<Port> GetCompatiblePorts(Port startPort)
        {
            List<Port> ports = new List<Port>();

            foreach (var node in Parent.NodeViews)
            {
                if (node == this) continue;
                if (startPort.direction == Direction.Output)
                {
                    ports.AddRange(node.InputPorts);
                }
                else
                {
                    ports.AddRange(node.OutputPorts);
                }
            }

            return ports;
        }

        public virtual int GetInputPortIndex(Port port)
        {
            for (int i = 0; i < InputPorts.Length; i++)
            {
                if (InputPorts[i] == port) return i;
            }
            return -1;
        }

        public virtual void ConnectChild(Edge edge, NodeView child)
        {
            var inputIndex = child.GetInputPortIndex(edge.input);
            if (inputIndex < 0)
            {
                Debug.LogError("Child does not match edge's input port");
                return;
            }

            for (int i = 0; i < OutputPorts.Length; i++)
            {
                if (OutputPorts[i] == edge.output)
                {
                    Undo.RegisterCompleteObjectUndo(Node, "Connect port");

                    if (OutputPorts[i].capacity == Port.Capacity.Single)
                    {
                        Node.Ports[i].Connections.Clear();
                    }

                    Node.Ports[i].Connections.Add(new NodeConnection()
                    {
                        Node = child.Node,
                        PortIndex = inputIndex
                    });
                    break;
                }
            }
        }

        public virtual void RemoveEdge(Edge edge, NodeView child)
        {
            var inputIndex = child.GetInputPortIndex(edge.input);
            if (inputIndex < 0)
            {
                Debug.LogError("Child does not match edge's input port");
                return;
            }

            for (int i = 0; i < OutputPorts.Length; i++)
            {
                if (OutputPorts[i] == edge.output)
                {
                    for (int x = 0; x < Node.Ports[i].Connections.Count; x++)
                    {
                        if (Node.Ports[i].Connections[x].PortIndex == inputIndex)
                        {
                            Undo.RegisterCompleteObjectUndo(Node, "Remove port");
                            Node.Ports[i].Connections.RemoveAt(x);
                            break;
                        }
                    }
                    break;
                }
            }
        }
        #endregion
    }
}
