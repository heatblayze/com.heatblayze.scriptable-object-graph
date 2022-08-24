using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestGraph.Core
{
    public class Questline : NodeBase, INodeContainer<QuestNodeBase>
    {
        public List<QuestNodeBase> Nodes => _nodes;
        [SerializeField]
        List<QuestNodeBase> _nodes;

        public NodeBase EntryNode => _entryNode;
        [SerializeField]
        NodeBase _entryNode;

        public IEnumerable<QuestNodeBase> GetNodes()
        {
            return _nodes;
        }

        public NodeBase CreateNode(Type type)
        {
            throw new NotImplementedException();
        }
    }
}
