using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestGraph.Core
{
    [CreateAssetMenu(menuName = "RPG Tools/Quest Graph", order = 120)]
    public class QuestGraph : ScriptableObject
    {
        public List<Questline> Questlines => _questlines;
        [SerializeField]
        List<Questline> _questlines = new List<Questline>();

        public QuestAttributeList Attributes => _attributes;
        [SerializeField]
        QuestAttributeList _attributes;
    }
}
