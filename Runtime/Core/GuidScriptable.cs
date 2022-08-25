using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectGraph
{
    public class GuidScriptable : ScriptableObject
    {
        public string GuidString => _guid;
        public Guid Guid => Guid.Parse(_guid);
        [SerializeField]
        internal string _guid = Guid.NewGuid().ToString();
    }
}
