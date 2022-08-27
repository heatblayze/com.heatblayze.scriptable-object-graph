using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectGraph.Editor
{
    public class EntryNodeView : Node
    {
        public ScriptableGraphView Parent;
        public Port Port;
        public event Action OnDoubleClick;

        public EntryNodeView()
        {
            Port = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Single, typeof(bool));
            Port.portName = "";
            outputContainer.Add(Port);
            title = "Entry";

            this.RegisterCallback<PointerDownEvent>(MouseDown, TrickleDown.TrickleDown);
        }

        void MouseDown(PointerDownEvent evt)
        {
            if (evt.clickCount >= 2)
            {
                OnDoubleClick?.Invoke();
            }
        }

        public void Connect(Node node)
        {
            Parent.ConnectPorts(Port, ((NodeView)node).InputPorts[0]);
        }

        public void SetEntryNode(Node node)
        {
            if(node is NodeView nodeView)
            {
                Undo.RecordObject((UnityEngine.Object)Parent.Asset, "Set entry node");
                Parent.Asset.SetEntryNode(nodeView.Node);

                Connect(node);
            }
        }
    }
}
