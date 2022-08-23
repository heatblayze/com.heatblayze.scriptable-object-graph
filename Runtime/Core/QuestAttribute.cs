using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestGraph.Core
{
    [Serializable]
    public abstract class QuestAttribute { }

    [Serializable]
    public class QuestAttributeList
    {
        public List<QuestAttribute> Components => _components;

        [SerializeField, SerializeReference]
        List<QuestAttribute> _components = new List<QuestAttribute>();

        public T GetAttribute<T>() where T : QuestAttribute
        {
            foreach (var component in Components)
            {
                if (component is T)
                    return (T)component;
            }
            return null;
        }

        public void GetAttribute<T>(List<T> components) where T : QuestAttribute
        {
            foreach (var component in Components)
            {
                if (component is T)
                    components.Add((T)component);
            }
        }

        public List<T> GetComponents<T>() where T : QuestAttribute
        {
            List<T> components = new List<T>();
            GetAttribute<T>(components);
            return components;
        }
    }
}
