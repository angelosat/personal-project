using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI
{
    public class ListBoxCollapsible : ScrollableBoxNew
    {
        readonly int IndentWidth = UIManager.ArrowRight.Rectangle.Width;

        public ListBoxCollapsibleNode ARoot = new("root");
        public ListBoxCollapsible(int width, int height, ScrollModes mode = ScrollModes.Vertical) : base(new Rectangle(0, 0, width, height), mode)
        {
            this.AutoSize = false;
        }

        public ListBoxCollapsible AddNode(ListBoxCollapsibleNode node)
        {
            this.ARoot.AddNode(node);
            return this;
        }
        public ListBoxCollapsible Build()
        {
            this.Client.ClearControls();
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

                if (node.Parent is not null)
                    node.Parent.ChildControls.Add(container);
                
                label.LeftClickAction = expand;
                void expand()
                {
                    if (!node.Expanded)
                    {
                        node.Expanded = true;
                        node.Arrow.SetTexture(UIManager.ArrowDown);
                        var nodeindex = this.Client.Controls.IndexOf(container);
                        var indexToInsert = nodeindex + 1;
                        var allControls = node.ChildControls.Concat(node.LeafControls);
                        foreach (var lc in allControls)
                        {
                            this.Client.Controls.Insert(indexToInsert, lc);
                            lc.Location.X = node.GetDepth() * IndentWidth;
                        }
                        this.Client.AlignTopToBottom();
                        this.UpdateClientSize();

                        var lowestLeaf = allControls.First();
                        this.EnsureControlVisible(lowestLeaf);
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

                            this.Client.Controls.RemoveAll(c => current.LeafControls.Contains(c) || current.ChildControls.Contains(c));
                            foreach(var child in current.Children)
                                descendants.Enqueue(child);
                        }
                        this.Client.AlignTopToBottom();
                        this.UpdateClientSize();
                    }
                };

                node.Control = container;

                foreach (var child in node.Children)
                    queue.Enqueue(child);
            }

            foreach (var child in this.ARoot.Children)
            {
                this.Client.AddControlsBottomLeft(child.Control);
            }
            return this;

        }

        private void EnsureControlVisible(Control control)
        {
            var lowestVisiblePoint = control.Location.Y + control.Height;
            EnsureLocationVisible(lowestVisiblePoint);
        }

        public void Clear()
        {
            this.Client.Controls.Clear();
            this.ARoot.Clear();
        }

        public override void Remeasure()
        {
            base.Remeasure();
            // TODO: lol
            int oldH = Client.Height;
            Client.ClientSize = new Rectangle(0, 0, Client.Width, Client.ClientSize.Height);
            Client.Height = oldH;
            foreach (var btn in Client.Controls)
            {
                btn.Width = Client.Width;
            }
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

