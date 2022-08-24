using ScriptableObjectGraph.Core;
using ScriptableObjectGraph.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectGraph
{
    namespace Internal
    {
        public interface INodeContainerBase
        {
            string EditorWindowPrefix { get; }
            Type GetNodeType();
            IEnumerable<NodeBase> GetNodesInternal();
            NodeBase CreateNode(Type type);
        }
    }

    public interface INodeContainer<T> : INodeContainerBase where T : NodeBase
    {
        Type INodeContainerBase.GetNodeType()
        {
            return typeof(T);
        }

        IEnumerable<NodeBase> INodeContainerBase.GetNodesInternal()
        {
            return GetNodes();
        }

        IEnumerable<T> GetNodes();
    }
}
