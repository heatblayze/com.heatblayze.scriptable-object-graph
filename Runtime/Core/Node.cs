using ScriptableObjectGraph.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectGraph.Core
{
    [System.Serializable]
    public class NodeConnection
    {
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
        public Vector2 Position;

        public INodeContainerBase Parent => _parent;
        [SerializeField]
        INodeContainerBase _parent;

        public NodeComponentList Components => _components;
        [SerializeField]
        NodeComponentList _components;

        public NodePort[] Ports => _connections;
        [SerializeField]
        protected NodePort[] _connections;

        public void Initialize(INodeContainerBase parent)
        {
            _parent = parent;
            OnCreated();
        }

        protected abstract void OnCreated();
    }
}
