# Scriptable Object Graph
A (soon to be) easily expandable implementation of Unity's experiemental GraphView system, built specifically for Scriptable Objects. 

Setting up the GraphView every time seemed like a chore, so I decided to make a package that would enable me to simple inherit from a few classes/interfaces and have a brand new node-based system without having to touch the GraphView code.

## How-to

There are essentially two main types to inherit from: NodeBase, and INodeContainer.

NodeBase is a class that inherits from ScriptableObject. This will automatically store things like the connections and attributes.

INodeContainer is an interface with a generic argument (where you list the Node type it contains). The interface **does not** store any information. You store the nodes yourself and implement the INodeContainer methods to provide the GraphView access to your nodes.

This way you can have a Node that is also a NodeContainer 😉

**Proper documentation will be provided upon initial release!**

# Support me
The tools I make are free, but if you're feeling generous you can...

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/C0C8EKRNY)
