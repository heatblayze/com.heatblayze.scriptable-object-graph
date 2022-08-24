using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestGraph.Core
{
    public class Questline : NodeBase, INodeContainer<QuestNodeBase>
    {
        public List<QuestNodeBase> Children => _children;
        [SerializeField]
        List<QuestNodeBase> _children;

        public NodeBase EntryNode => _entryNode;

        public string EditorWindowPrefix => "Quest";

        [SerializeField]
        NodeBase _entryNode;

        public IEnumerable<QuestNodeBase> GetNodes()
        {
            return _children;
        }

        public NodeBase CreateNode(Type type)
        {
            throw new NotImplementedException();
        }

        protected override void OnCreated()
        {
            _connections = new NodePort[] { new NodePort() };
        }
    }
}
