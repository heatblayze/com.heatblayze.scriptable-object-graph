using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace ScriptableObjectGraph.Editor
{
    public class BreadcrumbContainer : VisualElement
    {
        public new class UxmlFactory : UxmlFactory<BreadcrumbContainer, VisualElement.UxmlTraits> { }        

        Toolbar _toolbar;
        ToolbarBreadcrumbs _toolbarBreadcrumbs;

        public delegate void BreadcrumbSelectedDelegate(string title, int selectedIndex, int totalCrumbs);
        public event BreadcrumbSelectedDelegate BreadcrumbSelected;

        public string[] Breadcrumbs
        {
            set
            {
                _breadcrumbs = value;
                _toolbarBreadcrumbs.Clear();
                for (int i = 0; i < _breadcrumbs.Length; i++)
                {
                    int index = i;
                    _toolbarBreadcrumbs.PushItem(_breadcrumbs[i], () => { BreadcrumbSelected?.Invoke(_breadcrumbs[index], index, _breadcrumbs.Length); });
                }
            }
        }
        string[] _breadcrumbs;

        public BreadcrumbContainer()
        {
            _toolbar = new Toolbar();
            Add(_toolbar);
            _toolbarBreadcrumbs = new ToolbarBreadcrumbs();
            _toolbar.Add(_toolbarBreadcrumbs);
        }
    }
}
