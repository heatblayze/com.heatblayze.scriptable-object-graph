using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectGraph.Editor
{
    public class CustomPlacemat : Placemat
    {
        public PlacematData PlacematData { get; set; }
        public Action<CustomPlacemat> OnCollapseChange;
        public Action<CustomPlacemat> OnTitleChange;
        public Action<CustomPlacemat> OnPointerUp;
        public Action<CustomPlacemat> OnChangeColor;

        public override bool Collapsed
        {
            get => base.Collapsed;
            set
            {
                base.Collapsed = value;
                OnCollapseChange?.Invoke(this);
            }
        }

        public override string title
        {
            get => base.title;
            set
            {
                base.title = value;
                OnTitleChange?.Invoke(this);
            }
        }

        public override Color Color
        {
            get => base.Color;
            set
            {
                base.Color = value;
                OnChangeColor?.Invoke(this);
            }
        }

        public CustomPlacemat()
        {
            RegisterCallback<ClickEvent>(PointerUpCallback, TrickleDown.TrickleDown);
        }

        public void Init()
        {
            base.Collapsed = PlacematData.Collapsed;
            base.Color = PlacematData.Color;
        }

        void PointerUpCallback(ClickEvent evt)
        {
            OnPointerUp?.Invoke(this);
        }
    }
}
