using QuestGraph.Core;
using QuestGraph.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestGraph
{
    namespace Internal
    {
        public interface NodeContainerBase
        {
            string EditorWindowPrefix { get; }
            Type GetNodeType();
            IEnumerable<NodeBase> GetNodesInternal();
            NodeBase CreateNode(Type type);
        }
    }

    public interface INodeContainer<T> : NodeContainerBase where T : NodeBase
    {
        Type NodeContainerBase.GetNodeType()
        {
            return typeof(T);
        }

        IEnumerable<NodeBase> NodeContainerBase.GetNodesInternal()
        {
            return GetNodes();
        }

        IEnumerable<T> GetNodes();
    }
}
