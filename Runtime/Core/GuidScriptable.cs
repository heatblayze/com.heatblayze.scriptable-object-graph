using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectGraph
{
    public class GuidScriptable : ScriptableObject
    {
        public string GuidString => guid;
        public Guid Guid => Guid.Parse(guid);
        [SerializeField]
        internal string guid;
    }
}
