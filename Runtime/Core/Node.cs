using QuestGraph.Internal;
using System.Collections.Generic;
using UnityEngine;

namespace QuestGraph.Core
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

        public NodeContainerBase Parent => _parent;
        [SerializeField]
        NodeContainerBase _parent;

        public QuestAttributeList Attributes => _attributes;
        [SerializeField]
        QuestAttributeList _attributes;

        public NodePort[] Ports => _connections;
        [SerializeField]
        protected NodePort[] _connections;

        public void Initialize(NodeContainerBase parent)
        {
            _parent = parent;
            OnCreated();
        }

        protected abstract void OnCreated();
    }
}
