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
        public List<QuestAttribute> Attributes => _attributes;

        [SerializeField, SerializeReference]
        List<QuestAttribute> _attributes = new List<QuestAttribute>();

        public T GetAttribute<T>() where T : QuestAttribute
        {
            foreach (var attribute in Attributes)
            {
                if (attribute is T)
                    return (T)attribute;
            }
            return null;
        }

        public void GetAttribute<T>(List<T> attributes) where T : QuestAttribute
        {
            foreach (var attribute in Attributes)
            {
                if (attribute is T)
                    attributes.Add((T)attribute);
            }
        }

        public List<T> GetAttributes<T>() where T : QuestAttribute
        {
            List<T> attributes = new List<T>();
            GetAttribute<T>(attributes);
            return attributes;
        }
    }
}
