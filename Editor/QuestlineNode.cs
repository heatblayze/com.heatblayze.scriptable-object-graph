using QuestGraph.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace QuestGraph.Editor
{
    public class QuestlineNode : NodeView
    {
        Questline _questline;

        public QuestlineNode(Questline questline)
        {
            _questline = questline;
            title = _questline.name;
        }
    }

    public class QuestlineNodeFactory : NodeViewFactory<Questline>
    {
        public override string ContextMenuName => "Questline";

        protected override NodeView GenerateNodeView(Questline asset)
        {
            return new QuestlineNode(asset);
        }
    }
}
