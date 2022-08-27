using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace ScriptableObjectGraph.Editor
{
    public interface INodeViewFactory
    {
        string ContextMenuName { get; }
        NodeView Create(NodeBase node);
    }

    public abstract class NodeViewTraitsBase
    {
        public virtual void Init(NodeView nodeView, NodeBase node)
        {
            nodeView.Init(node);
        }
    }

    public abstract class NodeViewFactory<TCreatedType, TNodeType, TTraits> : INodeViewFactory
        where TCreatedType : NodeView, new() where TTraits : NodeViewTraitsBase, new()
    {
        public abstract string ContextMenuName { get; }

        private TTraits _traits;

        public virtual NodeView Create(NodeBase node)
        {
            TCreatedType nodeView = new TCreatedType();
            if (_traits == null)
                _traits = new TTraits();
            _traits.Init(nodeView, node);
            return nodeView;
        }
    }

    public static class NodeViewFactoryCache
    {
        static Dictionary<Type, INodeViewFactory> factories;

        static NodeViewFactoryCache()
        {
            factories = new Dictionary<Type, INodeViewFactory>();

            var types = TypeCache.GetTypesDerivedFrom(typeof(NodeView))
                .Where(x => x.GetNestedTypes().Any(n => typeof(INodeViewFactory).IsAssignableFrom(n))).ToList();
            types.Add(typeof(NodeView));

            foreach (var type in types)
            {
                var factoryType = type.GetNestedTypes().First(x => typeof(INodeViewFactory).IsAssignableFrom(x));
                var factory = (INodeViewFactory)Activator.CreateInstance(factoryType);
                var nodeType = factoryType.BaseType.GetGenericArguments().First(x => typeof(NodeBase).IsAssignableFrom(x));
                factories.Add(nodeType, factory);
            }
        }

        public static INodeViewFactory GetFactory(Type nodeType)
        {
            if (factories.TryGetValue(nodeType, out INodeViewFactory factory))
                return factory;
            return null;
        }
    }
}
