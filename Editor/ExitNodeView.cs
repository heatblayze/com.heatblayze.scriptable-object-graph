using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectGraph.Editor
{
    public class ExitNodeView : Node
    {
        public ScriptableGraphView Parent;
        public Port Port;
        public event Action OnDoubleClick;

        public ExitNodeView()
        {
            Port = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));
            Port.portName = "";
            outputContainer.Add(Port);
            title = "Exit";

            ColorUtility.TryParseHtmlString("#6A0000", out Color color);
            titleContainer.style.backgroundColor = color;
            titleContainer.style.width = 68;

            this.RegisterCallback<PointerDownEvent>(MouseDown, TrickleDown.TrickleDown);
        }

        void MouseDown(PointerDownEvent evt)
        {
            if (evt.clickCount >= 2)
            {
                OnDoubleClick?.Invoke();
            }
        }
    }
}
