using ScriptableObjectGraph;
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
            void AddNode(NodeBase node);
            void DeleteNode(NodeBase node);
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

        void AddNode(T node);
        void INodeContainerBase.AddNode(NodeBase node)
        {
            AddNode((T)node);
        }

        void DeleteNode(T node);
        void INodeContainerBase.DeleteNode(NodeBase node)
        {
            DeleteNode((T)node);
        }
    }
}
