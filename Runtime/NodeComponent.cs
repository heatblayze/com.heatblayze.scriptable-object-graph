using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectGraph
{
    [Serializable]
    public abstract class NodeComponent : ICloneable
    {
        [SerializeField, HideInInspector]
        string m_Name;

        public NodeBase ParentNode { get => _parentNode; set => _parentNode = value; }
        [SerializeField, HideInInspector]
        NodeBase _parentNode;

        public virtual object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    [Serializable]
    public class NodeComponentList
    {
        public List<NodeComponent> Components => _components;

        [SerializeField, SerializeReference]
        List<NodeComponent> _components = new List<NodeComponent>();

        public T GetComponent<T>() where T : NodeComponent
        {
            foreach (var attribute in Components)
            {
                if (attribute is T)
                    return (T)attribute;
            }
            return null;
        }

        public void GetComponents<T>(List<T> attributes) where T : NodeComponent
        {
            foreach (var attribute in Components)
            {
                if (attribute is T)
                    attributes.Add((T)attribute);
            }
        }

        public List<T> GetComponents<T>() where T : NodeComponent
        {
            List<T> attributes = new List<T>();
            GetComponents<T>(attributes);
            return attributes;
        }

        public NodeComponentList Clone()
        {
            var list = new NodeComponentList();
            foreach(var component in Components)
            {
                list.Components.Add((NodeComponent)component.Clone());
            }
            return list;
        }
    }
}
