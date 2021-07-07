﻿using System;
using System.Collections.Generic;

namespace Start_a_Town_.UI
{
    public class ListBoxCollapsibleNode
    {
        public List<Control> Leafs = new List<Control>();
        public List<ILabeled> CustomLeafs = new List<ILabeled>();
        public Control Control;
        public List<Control> LeafControls = new List<Control>();
        public List<Control> ChildControls = new List<Control>();
        public ListBoxCollapsibleNode Parent;
        public List<ListBoxCollapsibleNode> Children = new List<ListBoxCollapsibleNode>();
        public Func<ButtonBase> ControlGetter;
        public PictureBox Arrow;

        public string Name;
        public bool Expanded;

        public ListBoxCollapsibleNode(string name)
        {
            this.Name = name;
        }
        public ListBoxCollapsibleNode(string name, Func<ListBoxCollapsibleNode, ButtonBase> controlGetter) : this(name)
        {
            this.ControlGetter = () => controlGetter(this);
        }
        public ListBoxCollapsibleNode(string name, Func<ButtonBase> controlGetter):this(name)
        {
            this.ControlGetter = controlGetter;
        }
        public ListBoxCollapsibleNode AddNode(ListBoxCollapsibleNode node)
        {
            this.Children.Add(node);
            node.Parent = this;
            return this;
        }
        public ListBoxCollapsibleNode AddLeaf(ILabeled leaf)
        {
            this.CustomLeafs.Add(leaf);
            return this;
        }
        public ListBoxCollapsibleNode AddLeaf(Control leaf)
        {
            this.LeafControls.Add(leaf);
            leaf.Validate(true);
            return this;
        }
        public ListBoxCollapsibleNode Clear()
        {
            this.Children.Clear();
            return this;
        }
       
        public void Build<C>(Action<ILabeled, C> onControlInit, Action<ILabeled[]> onLeafSelect) where C : ButtonBase, new()
        {
            foreach (var item in this.CustomLeafs)
            {
                var checkbox = new C();
                checkbox.Tag = item;
                var label = new Label(item.Label);
                var box = new GroupBox();
                box.AddControlsHorizontally(checkbox, label);
                onControlInit(item, checkbox);
                checkbox.LeftClickAction = () => onLeafSelect(new ILabeled[] { item });
                this.LeafControls.Add(box);
            }
        }
        public int GetDepth()
        {
            var depth = 0;
            var parent = this.Parent;
            while (parent != null)
            {
                depth++;
                parent = parent.Parent;
            }
            return depth;
        }
        public override string ToString()
        {
            return this.Name;
        }

        internal void FindLeafIndex(ButtonBase c, ref int i)
        {
            foreach (var leaf in this.Leafs)
            {
                if (leaf == c)
                    return;
                i++;
            }
            foreach (var child in this.Children)
                child.FindLeafIndex(c, ref i);
        }

        internal ButtonBase GetLeaf(int i)
        {
            throw new NotImplementedException();
        }
    }
}

