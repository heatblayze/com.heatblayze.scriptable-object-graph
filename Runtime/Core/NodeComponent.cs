using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectGraph.Core
{
    [Serializable]
    public abstract class NodeComponent { }

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
    }
}
