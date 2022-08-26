using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectGraph
{
    public abstract class NodeContainerAsset<T> : GuidScriptable, INodeContainer<T> where T : NodeBase
    {
        public abstract string EditorWindowPrefix { get; }

        public List<T> Children => _children;
        [SerializeField]
        List<T> _children = new List<T>();

        public NodeComponentList Components => _components;
        [SerializeField]
        NodeComponentList _components;

        public T EntryNode => _entryNode;

        [SerializeField]
        T _entryNode;

        public IEnumerable<T> GetNodes()
        {
            return _children;
        }

        protected virtual void OnNodeCreated(T node) { }
        public NodeBase CreateNode(Type type)
        {
            T node = CreateInstance(type) as T;
            node.name = type.Name;

            OnNodeCreated(node);
            return node;
        }

        public void AddNode(T node)
        {
            if (_entryNode == null)
                _entryNode = node;
            _children.Add(node);
        }

        public void DeleteNode(T node)
        {
            if (_entryNode == node)
                _entryNode = null;

            _children.Remove(node);
        }
    }
}
