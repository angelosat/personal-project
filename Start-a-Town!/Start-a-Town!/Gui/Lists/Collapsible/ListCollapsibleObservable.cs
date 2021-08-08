using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public partial class ListCollapsibleObservable : GroupBox
    {
        [Obsolete]
        public Node ARoot = new("root");
        readonly List<Node> RootNodes = new();
        int Spacing = 1;
        new IListCollapsibleDataSourceObservable DataSource;
        public ListCollapsibleObservable(IListCollapsibleDataSourceObservable root)
        {
            this.Bind(root);
        }
        
        void Bind(IListCollapsibleDataSourceObservable dataSource)
        {
            if(this.DataSource is not null)
            {
                this.DataSource.ListBranches.CollectionChanged -= this.ListBranches_CollectionChanged;
                this.DataSource.ListLeafs.CollectionChanged -= this.ListLeafs_CollectionChanged;
            }
            dataSource.ListBranches.CollectionChanged += this.ListBranches_CollectionChanged;
            dataSource.ListLeafs.CollectionChanged += this.ListLeafs_CollectionChanged;
            this.DataSource = dataSource;
            this.Build();
        }

        private void ListBranches_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var added = e.NewItems?.Cast<IListCollapsibleDataSourceObservable>();
            var removed = e.OldItems?.Cast<IListCollapsibleDataSourceObservable>();
            if (added is not null)
                foreach (var i in added)
                {
                    var node = this.Add(i);
                    this.RootNodes.Add(node);
                    this.AddControlsBottomLeft(node.Control);
                }
            if (removed is not null)
                foreach (var node in removed)
                    this.Remove(node);
        }

        private void ListLeafs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var added = e.NewItems?.Cast<IListable>();
            var removed = e.OldItems?.Cast<IListable>();
            foreach (var i in added)
            {

            }
            foreach(var i in removed)
            {

            }
        }

        public ListCollapsibleObservable Build()
        {
            this.ClearControls();
            var queue = new Queue<IListCollapsibleDataSourceObservable>(this.DataSource.ListBranches);

            while (queue.Any())
            {
                var branch = queue.Dequeue();
                this.Add(branch);
                foreach (var child in branch.ListBranches)
                    queue.Enqueue(child);
            }

            foreach (var child in this.ARoot.ChildrenNodes)
                this.AddControlsBottomLeft(child.Control);
            return this;
        }
        
        private Node Add(IListCollapsibleDataSourceObservable branch)
        {
            var node = new Node(branch);
            var nodeContainer = new GroupBox() { Name = "container", BackgroundColor = Color.Black * .2f };// UIManager.DefaultListItemBackgroundColor };
            var nodeItem = new GroupBox() { Name = "item" };
            node.Arrow = new PictureBox(UIManager.ArrowRight) { LeftDownAction = expand };// { LeftClickAction = expand };
            var label = new Label(node.Name) { Active = true };
            var control = node.ControlGetter?.Invoke();
            if (control is not null)
                nodeItem.AddControlsHorizontally(node.Arrow, control, label);
            else
                nodeItem.AddControlsHorizontally(node.Arrow, label);
            nodeItem.CenterControlsAlignmentVertically();
            nodeItem.Validate(true);
            nodeContainer.AddControls(nodeItem);

            node.ParentNode?.ChildControls.Add(nodeContainer);
            node.ParentNode?.ChildGroupBox.Controls.Insert(0, nodeContainer);

            label.LeftClickAction = expand;
            void expand()
            {
                if (!node.Expanded)
                {
                    node.Expanded = true;
                    node.Arrow.SetTexture(UIManager.ArrowDown);
                    //node.ChildGroupBox.Location = nodeItem.BottomLeft + new Vector2(Node.IndentWidth, Spacing);
                    node.ChildGroupBox.Location = nodeItem.BottomLeft;// + new Vector2(0, Spacing);
                    nodeContainer.AddControls(node.ChildGroupBox);
                }
                else
                {
                    node.Expanded = false;
                    node.Arrow.SetTexture(UIManager.ArrowRight);
                    nodeContainer.RemoveControls(node.ChildGroupBox);
                }
                var parent = node;
                while (parent is not null)
                {
                    parent.ChildGroupBox?.AlignTopToBottom(this.Spacing);
                    parent = parent.ParentNode;
                }
                this.AlignTopToBottom(this.Spacing);
            };
            node.Control = nodeContainer;
            return node;
        }
        private void Remove(IListCollapsibleDataSourceObservable branch)
        {
            var node = this.RootNodes.Find(n => n.Source == branch);
            this.RootNodes.Remove(node);
            this.Controls.Remove(node.Control);
        }
        internal override void OnControlAdded(Control control)
        {
            base.OnControlAdded(control);
            this.AlignTopToBottom(this.Spacing);
        }
        internal override void OnControlRemoved(Control control)
        {
            base.OnControlRemoved(control);
            this.AlignTopToBottom(this.Spacing);
        }
        internal override void OnControlResized(Control control)
        {
            base.OnControlResized(control);
            this.AlignTopToBottom(this.Spacing);
        }

        internal bool FindLeafIndex(Control c, out int i)
        {
            i = 0;
            foreach (var item in this.GetEnumerable())
            {
                if (c == item)
                    return true;
                i++;
            }
            return false;
        }
        internal Control GetLeafByIndex(int i)
        {
            var n = 0;
            var enumerator = this.GetEnumerable().GetEnumerator();
            do { enumerator.MoveNext(); } while (n++ != i);
            return enumerator.Current;
        }
        IEnumerable<Control> GetEnumerable()
        {
            var queue = new Queue<Node>();
            queue.Enqueue(this.ARoot);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var leaf in current.Leafs)
                    yield return leaf;
                foreach (var child in current.ChildrenNodes)
                    queue.Enqueue(child);
            }
        }
    }
}

