using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestGraph.Core
{
    public class Questline : ScriptableObject
    {
        public List<Node> Nodes => _nodes;
        [SerializeField]
        List<Node> _nodes = new List<Node>();

        public QuestAttributeList Attributes => _attributes;
        [SerializeField]
        QuestAttributeList _attributes;
    }
}
