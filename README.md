This project has been archived as I am no longer using Unity.

# Scriptable Object Graph
An easily expandable implementation of Unity's experiemental GraphView system, built specifically for Scriptable Objects. 

Setting up the GraphView every time seemed like a chore, so I decided to make a package that would enable me to simple inherit from a few classes/interfaces and have a brand new node-based system without having to touch the GraphView code.

![image](https://user-images.githubusercontent.com/10677646/187116635-de6705d9-e27d-457a-9220-66598013beef.png)

## How-to
Setting up a new node system is relatively straightforward, no major coding is required!

### Setting up the Node
The first thing you'll need is the Node asset. The simplest way to do this is to inherit from `NodeBase`.

```
using ScriptableObjectGraph;

public class QuestNodeBase : NodeBase { }
```

If you want to create a Node that contains other Nodes, you can instead inherit from the `NodeContainerNode<>` class, specifying the contained Node type with the generic argument.

```
using ScriptableObjectGraph;
public class Questline : NodeContainerNode<QuestNode>
{
    public override string EditorWindowPrefix => "Quest";
}
```

### Setting up the asset
Next you'll need to create the root asset. Simply inherit from the `NodeContainerAsset<>` class, specifying the contained Node type with the generic argument.

In the example below, I use the `CreateAssetMenu` attribute to allow the user to create the asset, but this is not required.
```
using ScriptableObjectGraph;
using UnityEngine;

[CreateAssetMenu(menuName = "RPG Tools/Quest Graph", order = 120)]
public class QuestGraphAsset : NodeContainerAsset<Questline>
{
    public override string EditorWindowPrefix => "Questline";
}
```

### Setting up the NodeView
The one final thing required is to create a `NodeView` type for each of the Node types you've made. Here's an example:

```
using ScriptableObjectGraph.Editor;
public class QuestlineNodeView : NodeView
{
    public new class NodeViewFactory : NodeViewFactory<QuestlineNodeView, Questline, NodeViewTraits>
    {
        public override string ContextMenuName => "Questline";
    }
}
```
Note that `QuestlineNodeView` has a sub-class of `NodeViewFactory`. **This is required.** Ensure you create one as shown above, using the `new` keyword.

This system works very similarly to Unity's UIElements system if you've worked with that.
If not, here's a brief description of the `NodeViewFactory`:
- The three generic arguments are:
  - The `NodeView` type this factory generates.
  - The `NodeBase` type that the `NodeView` represents.
  - The final argument, `NodeViewTraits`, can be left as the base class *or* you can specify your own. Creating your own `NodeViewTraits` type will allow you to alter each `NodeView` when it is created, without having to specify a new `NodeView` type.


### What's next?
You're all set! Now you can create an instance of your root asset and then double-click to open the Editor window.

Note: currently only one instance of the window is supported (including across different asset types).

## Extending the data contained within a node
If you want to include more information inside a node, the easiest way is to just store it in the script for the asset itself. Nodes *do* have a custom inspector, but all of your custom fields should render exactly like normal.

If you instead want to create a data component that can be placed onto any number of types of Nodes (and their ports as well), you can create a `NodeComponent` class.

```
using ScriptableObjectGraph;
using UnityEngine;

[System.Serializable]
public class DemoNodeComponent : NodeComponent
{
    [Multiline]
    public string Content = "This is a multi-line string\nSee!";
    [Range(0f, 1f)]
    public float Range = 0.3f;
}
```
**Take note of the `[System.Serializable]` attribute. This is required.**

Currently the "Add Component" button just shows a simple dropdown menu with all the Type names. This will be expanded in the future to be something that more closely resembles the Monobehaviour "Add Component" button.

# Support me
The tools I make are free, but if you're feeling generous

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/C0C8EKRNY)
