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
        [SerializeField, HideInInspector]
        List<T> _children = new List<T>();

        public NodeComponentList Components => _components;
        [SerializeField, HideInInspector]
        NodeComponentList _components;

        public T EntryNode => _entryNode;

        [SerializeField, HideInInspector]
        T _entryNode;

        public Vector2 EntryNodePosition { get => _entryNodePosition; set => _entryNodePosition = value; }
        [SerializeField, HideInInspector]
        Vector2 _entryNodePosition = new Vector2(140, 375);

        public Vector2 ExitNodePosition { get => _exitNodePosition; set => _exitNodePosition = value; }
        [SerializeField, HideInInspector]
        Vector2 _exitNodePosition = new Vector2(1000, 375);

        public List<PlacematData> Placemats => _placemats;
        [SerializeField, SerializeReference, HideInInspector]
        List<PlacematData> _placemats = new List<PlacematData>();

        public IEnumerable<PlacematData> GetPlacemats()
        {
            return Placemats;
        }

        public void AddPlacemat(PlacematData placemat)
        {
            Placemats.Add(placemat);
        }

        public void DeletePlacemat(PlacematData placemat)
        {
            Placemats.Remove(placemat);
        }

        public NodeBase GetEntryNode()
        {
            return _entryNode;
        }

        public void SetEntryNode(T node)
        {
            _entryNode = node;
        }

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
