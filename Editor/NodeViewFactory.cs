using ScriptableObjectGraph.Editor.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ScriptableObjectGraph.Editor
{
    namespace Internal
    {
        public abstract class NodeViewFactoryBase
        {
            public abstract string ContextMenuName { get; }
            internal abstract NodeView CreateNodeView(GuidScriptable guidScriptable);
        }

        internal static class NodeViewFactoryCache
        {
            static Dictionary<Type, NodeViewFactoryBase> _factories;
            public static Dictionary<Type, NodeViewFactoryBase> Factories
            {
                get
                {
                    if (_factories == null)
                        FillFactories();
                    return _factories;
                }
            }

            public static NodeViewFactoryBase GetFactory(Type type)
            {
                foreach (var factory in Factories)
                {
                    var matchingBase = GetBaseType(factory.Key);
                    var args = matchingBase.GetGenericArguments();
                    if (args[0] == type)
                        return factory.Value;
                }
                return null;
            }

            static Type GetBaseType(Type type)
            {
                if (type.BaseType.BaseType == typeof(NodeViewFactoryBase))
                    return type.BaseType;
                return GetBaseType(type.BaseType);
            }

            static void FillFactories()
            {
                _factories = new Dictionary<Type, NodeViewFactoryBase>();

                var coreType = typeof(NodeViewFactoryBase);
                var types = AppDomain.CurrentDomain.GetAssemblies()
                    .SelectMany(a =>
                        a.GetTypes()
                        .Where(t =>
                            coreType.IsAssignableFrom(t)
                            && !t.IsAbstract
                            && !t.ContainsGenericParameters));

                foreach(var type in types)
                {
                    _factories.Add(type, Activator.CreateInstance(type) as NodeViewFactoryBase);
                }
            }
        }
    }

    public abstract class NodeViewFactory<T> : NodeViewFactoryBase where T : GuidScriptable
    {
        internal override NodeView CreateNodeView(GuidScriptable guidScriptable)
        {
            return GenerateNodeView((T)guidScriptable);
        }

        protected abstract NodeView GenerateNodeView(T asset);
    }
}
