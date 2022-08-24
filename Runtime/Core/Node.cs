using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestGraph.Core
{
    public class NodeBase : GuidScriptable
    {
        public QuestAttributeList Attributes => _attributes;
        [SerializeField]
        QuestAttributeList _attributes;

        public List<NodeBase> Connections => _connections;
        [SerializeField]
        List<NodeBase> _connections;
    }
}
