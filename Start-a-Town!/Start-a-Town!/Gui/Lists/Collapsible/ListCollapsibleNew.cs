using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    public class ListCollapsibleNew : GroupBox
    {
        readonly int IndentWidth = UIManager.ArrowRight.Rectangle.Width;

        public ListBoxCollapsibleNode ARoot = new("root");
        int Spacing = 1;
        public ListCollapsibleNew AddNode(ListBoxCollapsibleNode node)
        {
            this.ARoot.AddNode(node);
            return this;
        }
        public ListCollapsibleNew Build()
        {
            this.ClearControls();
            var queue = new Queue<ListBoxCollapsibleNode>(this.ARoot.Children);
            
            while (queue.Any())
            {
                var node = queue.Dequeue();
                var container = new GroupBox();
                node.Arrow = new PictureBox(UIManager.ArrowRight) { LeftClickAction = expand };
                var label = new Label(node.Name) { Active = true };
                var control = node.ControlGetter?.Invoke();
                if (control is not null)
                    container.AddControlsHorizontally(node.Arrow, control, label);
                else
                    container.AddControlsHorizontally(node.Arrow, label);
                container.CenterControlsAlignmentVertically();
                container.Validate(true);

                node.Parent?.ChildControls.Add(container);
                
                label.LeftClickAction = expand;
                void expand()
                {
                    if (!node.Expanded)
                    {
                        node.Expanded = true;
                        node.Arrow.SetTexture(UIManager.ArrowDown);
                        var nodeindex = this.Controls.IndexOf(container);
                        var indexToInsert = nodeindex + 1;
                        var depth = node.GetDepth();
                        foreach (var lc in node.ChildControls)
                        {
                            this.Controls.Insert(indexToInsert++, lc);
                            lc.Location.X = depth * IndentWidth;
                        }
                        foreach (var lc in node.LeafControls)
                        {
                            this.Controls.Insert(indexToInsert++, lc);
                            lc.Location.X = depth * IndentWidth  + UIManager.ArrowDown.Rectangle.Width;
                        }
                        this.AlignTopToBottom(this.Spacing);
                    }
                    else
                    {
                        var descendants = new Queue<ListBoxCollapsibleNode>();
                        descendants.Enqueue(node);
                        while (descendants.Any())
                        {
                            var current = descendants.Dequeue();
                            current.Expanded = false;
                            current.Arrow.SetTexture(UIManager.ArrowRight);

                            this.Controls.RemoveAll(c => current.LeafControls.Contains(c) || current.ChildControls.Contains(c));
                            foreach(var child in current.Children)
                                descendants.Enqueue(child);
                        }
                        this.AlignTopToBottom(this.Spacing);
                    }
                };

                node.Control = container;

                foreach (var child in node.Children)
                    queue.Enqueue(child);
            }

            foreach (var child in this.ARoot.Children)
                this.AddControlsBottomLeft(child.Control);
            return this;
        }

        public void Clear()
        {
            this.Controls.Clear();
            this.ARoot.Clear();
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
            var queue = new Queue<ListBoxCollapsibleNode>();
            queue.Enqueue(this.ARoot);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var leaf in current.Leafs)
                    yield return leaf;
                foreach (var child in current.Children)
                    queue.Enqueue(child);
            }
        }
    }
}

