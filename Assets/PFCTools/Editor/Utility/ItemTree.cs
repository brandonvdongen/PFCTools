using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace PFCTools.Utils {

    public class ItemTree<T> {

        public delegate ItemTreeNode<T> nodeProcessor(ItemTreeNode<T> Node);
        public delegate void genericEvent();

        public nodeProcessor onAddNode;
        public nodeProcessor onRemoveNode;
        public genericEvent onClear;
        public genericEvent onChanged;

        public readonly Dictionary<T,ItemTreeNode<T>> Nodes = new Dictionary<T,ItemTreeNode<T>>();

        //Constructor
        public ItemTree() {
            return;
        }
        public ItemTree(T[] Content) {
            foreach (var Item in Content) {
                AddNode(new ItemTreeNode<T>(Item));
            }
        }
        public ItemTreeNode<T> this[T Content] {
            get { return this.Nodes[Content]; }
        }
        //AddNode
        public void AddNode(T Node) {
            if (Node == null) Debug.LogWarning("Attempted to add empty node");
            else {
                var NewNode = new ItemTreeNode<T>(Node);
                if (onAddNode != null) NewNode = onAddNode(NewNode);
                if (NewNode != null) Nodes.Add(Node,NewNode);
                onChanged?.Invoke();
            }
        }
        public void AddNode(ItemTreeNode<T> Node) {
            if (Node == null) {
                Debug.LogWarning("Attempted to add empty node");
                return;
            }
            else {
                if (onAddNode != null) Node = onAddNode(Node);
                if (Node != null) Nodes.Add(Node.Value,Node);
                onChanged?.Invoke();
            }
        }
        public void AddNodes(T[] Nodes) {
            if (Nodes.Length < 1) Debug.LogWarning("Attempted to add empty Nodes");
            else {
                foreach (var Node in Nodes) {
                    var NewNode = new ItemTreeNode<T>(Node);
                    if (onAddNode != null) NewNode = onAddNode(NewNode);
                    if(NewNode != null)this.Nodes.Add(Node, NewNode);
                }
                onChanged?.Invoke();
            }            
        }
        //RemoveNode
        public void RemoveNode(T Node) {
            if (Nodes.ContainsKey(Node)) {
                onRemoveNode?.Invoke(Nodes[Node]);
                Nodes.Remove(Node);
                onChanged?.Invoke();
            }
        }
        public void RemoveNode(ItemTreeNode<T> Node) {
            if (Nodes.ContainsKey(Node.Value)) {
                onRemoveNode?.Invoke(Nodes[Node.Value]);
                Nodes.Remove(Node.Value);
                onChanged?.Invoke();
            }
        }

        //ChangeParent
        public void SetParent(ItemTreeNode<T> Node, T newParent) {
            if (Nodes.ContainsKey(Node.Value)) Nodes[Node.Value] = Nodes[newParent];
        }
        public void SetParent(ItemTreeNode<T> Node, ItemTreeNode<T> newParent) {
            if (Nodes.ContainsKey(Node.Value)) Nodes[Node.Value] = newParent;
        }
        public void ClearParent(ItemTreeNode<T> Node) {
            if (Nodes.ContainsKey(Node.Value)) Nodes[Node.Value] = null;
        }
        //ChangeChild
        public void AddChild(ItemTreeNode<T> Parent, ItemTreeNode<T> Child) {
            if (Nodes.ContainsKey(Parent.Value)) Nodes[Parent.Value].Children.Add(Child);
        }
        public void RemoveChild(ItemTreeNode<T> Parent, ItemTreeNode<T> Child) {
            if (Nodes.ContainsKey(Parent.Value)) Nodes[Parent.Value].Children.Remove(Child);
        }
        //Contains
        public bool Contains(ItemTreeNode<T> Node) {
            if (Node == null) return false;
            if (Nodes.ContainsKey(Node.Value)) return true;
            else return false;
        }
        public bool Contains(T Node) {
            if (Nodes.ContainsKey(Node)) return true;
            else return false;
        }

        public void Clear() {
            if (onClear != null) onClear();
            Nodes.Clear();
            onChanged?.Invoke();
        }
    }

    public class ItemTreeNode<T> {

        readonly T _value = default;
        public T Value { get { return _value; } }

        public ItemTreeNode<T> Parent = null;
        readonly List<ItemTreeNode<T>> _children = new List<ItemTreeNode<T>>();
        public List<ItemTreeNode<T>> Children { get { return _children; } }

        //ItemTreeNode
        public ItemTreeNode(T content) {
            _value = content;
        }
        public ItemTreeNode(T content, ItemTreeNode<T> Parent) {
            _value = content;
            this.Parent = Parent;
        }
        public ItemTreeNode(T content, ItemTreeNode<T> Parent, List<ItemTreeNode<T>> Children) {
            _value = content;
            this.Parent = Parent;
            _children = Children;
        }
        public ItemTreeNode(T content, List<ItemTreeNode<T>> Children) {
            _value = content;
            _children = Children;
        }
    }
}