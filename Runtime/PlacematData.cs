using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ScriptableObjectGraph
{
    [System.Serializable]
    public class PlacematData
    {
        public int ZOrder;
        public Rect Position;
        public string Title;
        public bool Collapsed;
        public Color Color = new Color(0.15f, 0.19f, 0.19f);
    }
}
