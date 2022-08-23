using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestGraph.Core
{
    public class Node : ScriptableObject
    {
        public QuestAttributeList Attributes => _attributes;
        [SerializeField]
        QuestAttributeList _attributes;
    }
}
