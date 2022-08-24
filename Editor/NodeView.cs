using QuestGraph.Core;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace QuestGraph.Editor
{
    public abstract class NodeView : Node
    {
        public NodeBase Node;

        public NodeView(NodeBase node)
        {
            Node = node;
            Undo.undoRedoPerformed += UndoPerformed;
        }

        ~NodeView()
        {
            Undo.undoRedoPerformed -= UndoPerformed;
        }

        protected virtual void UndoPerformed()
        {
            SetPosition(GetPosition());
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);
            Node.Position = newPos.position;
        }
    }
}
