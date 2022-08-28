using ScriptableObjectGraph.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectGraph
{
    [System.Serializable]
    public class NodeConnection
    {
        public bool ConnectsToExit;
        public NodeBase Node;
        public int PortIndex;
    }

    [System.Serializable]
    public class NodePort
    {
        public List<NodeConnection> Connections = new List<NodeConnection>();
    }

    public abstract class NodeBase : GuidScriptable
    {
        public Vector2 Position { get => _position; set => _position = value; }
        [SerializeField, HideInInspector]
        Vector2 _position;

        public GuidScriptable Parent => _parent;
        [SerializeField, HideInInspector]
        GuidScriptable _parent;

        public NodeComponentList Components => _components;
        [SerializeField]
        NodeComponentList _components;

        public NodePort[] Ports { get => _ports; set => _ports = value; }
        [SerializeField, HideInInspector]
        protected NodePort[] _ports;

        public void Initialize(INodeContainerBase parent)
        {
            _parent = (GuidScriptable)parent;
            OnCreated();
        }

        protected abstract void OnCreated();

        public virtual NodeBase Clone()
        {
            var clone = (NodeBase)CreateInstance(GetType());
            clone.name = name + " Copy";
            clone._position = _position + new Vector2(20, 20);
            clone._parent = _parent;
            clone._components = Components.Clone();
            clone._ports = new NodePort[Ports.Length];
            for (int i = 0; i < Ports.Length; i++)
            {
                clone._ports[i] = new NodePort();
            }
            return clone;
        }
    }
}
