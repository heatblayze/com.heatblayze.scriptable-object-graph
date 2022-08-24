using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace QuestGraph.Core
{
    [System.Serializable]
    public class VariableSet : ISerializationCallbackReceiver
    {
        [System.Serializable]
        class VariableDictionary
        {
            public string Name;
            public List<Variable> Variables = new List<Variable>();
        }

        public Dictionary<string, List<Variable>> Variables { get; private set; } = new Dictionary<string, List<Variable>>();
        [SerializeField]
        private VariableDictionary[] _variables;

        public void OnBeforeSerialize()
        {
            _variables = new VariableDictionary[Variables.Count];

            int i = 0;
            foreach (var variable in Variables)
            {
                _variables[i++] = new VariableDictionary()
                {
                    Name = variable.Key,
                    Variables = variable.Value
                };
            }
        }

        public void OnAfterDeserialize()
        {
            Variables = new Dictionary<string, List<Variable>>();
            foreach (var variable in _variables)
            {
                Variables.Add(variable.Name, variable.Variables);
            }

            _variables = null;
        }
    }

    public class Variable : GuidScriptable
    {
        public string Name;
    }
}
