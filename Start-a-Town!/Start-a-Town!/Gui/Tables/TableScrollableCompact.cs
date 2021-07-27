using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    [Obsolete]
    class TableScrollableCompact<TObject> : GroupBox
    {
        readonly List<Column> Columns = new();
        readonly GroupBox ColumnLabels;
        ScrollableBoxNew BoxItems;
        readonly Dictionary<TObject, Dictionary<object, Control>> Rows = new();
        public int MaxVisibleItems;// = -1;
        public bool ShowColumnLabels = true;
        public Color ClientBoxColor = Color.Black * .5f;
        public TableScrollableCompact(int maxVisibleItems, BackgroundStyle style)
            : this(maxVisibleItems)
        {
        }
        public TableScrollableCompact(int maxVisibleItems)
        {
            this.MaxVisibleItems = maxVisibleItems;
            this.ColumnLabels = new GroupBox() { AutoSize = true, BackgroundColor = Color.Black * .5f };
        }
        public TableScrollableCompact<TObject> AddColumn(object tag, Control columnHeader, int spacing, Func<TObject, Control> control, float anchor = .5f)
        {
            this.Columns.Add(new Column(tag, columnHeader, spacing, control, anchor));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="spacing"></param>
        /// <param name="control"></param>
        /// <param name="anchor">Value between 0 and 1 for horizontal alignment</param>
        /// <returns></returns>
        public TableScrollableCompact<TObject> AddColumn(object tag, string type, int spacing, Func<TObject, Control> control, float anchor = 0, bool showColumnLabels = true)
        {
            this.ShowColumnLabels = showColumnLabels;
            this.Columns.Add(new Column(tag, type, spacing, control, anchor));
            this.Build(new List<TObject>(), showColumnLabels);
            return this;
        }
        public void ToggleColumnLabels(bool show)
        {
            if (show && !this.ShowColumnLabels)
            {
                this.AddControls(this.ColumnLabels);
                this.BoxItems.Location = this.ColumnLabels.BottomLeft;
            }
            else if (!show && this.ShowColumnLabels)
            {
                this.RemoveControls(this.ColumnLabels);
                this.BoxItems.Location = Vector2.Zero;
            }
            this.ShowColumnLabels = show;
        }
        public void Build(IEnumerable<TObject> items, bool showColumnLabels = true)
        {
            this.ShowColumnLabels = showColumnLabels;
            this.Rows.Clear();
            this.Controls.Clear();
            this.ColumnLabels.Controls.Clear();
            int offset = 0;
            foreach (var c in this.Columns)
            {
                if (c.ColumnHeader != null)
                {
                    c.ColumnHeader.Location = new Vector2(offset + c.Width * .5f, 0);
                    c.ColumnHeader.Anchor = new Vector2(.5f, 0);
                    offset += c.Width;
                    this.ColumnLabels.AddControls(c.ColumnHeader);
                }
                else
                {
                    var label = new Label(new Vector2(offset, 0), c.Object);
                    offset += c.Width;
                    label.TextHAlign = HorizontalAlignment.Center;
                    this.ColumnLabels.AddControls(label);
                }
            }
            this.ColumnLabels.ClientSize = new Rectangle(0, 0, this.Columns.Sum(c => c.Width), this.ColumnLabels.ClientSize.Height);
            if (this.ShowColumnLabels)
            {
                this.Controls.Add(this.ColumnLabels);
            }

            this.ColumnLabels.Controls.AlignCenterHorizontally();
            this.BoxItems = new ScrollableBoxNew(new Rectangle(0, 0, this.ColumnLabels.Width, Label.DefaultHeight * this.MaxVisibleItems));

            if (this.ShowColumnLabels)
            {
                this.BoxItems.Location = this.ColumnLabels.BottomLeft + Vector2.UnitY;
            }

            this.AddItems(items.ToArray());

            this.Controls.Add(this.BoxItems);
        }

        public Control GetItem(TObject row, object column)
        {
            return this.Rows[row][column];
        }
        public Control GetItem(Func<TObject, bool> row, object column)
        {
            if (!this.Rows.Any(v => row(v.Key)))
            {
                return null;
            }

            var r = this.Rows.FirstOrDefault(v => row(v.Key));
            return r.Value[column];
        }
        public void AddItem(TObject item)
        {
            var panel = new GroupBox() { Location = new Vector2(0, this.BoxItems.Client.Controls.BottomLeft.Y), BackgroundColor = this.ClientBoxColor };
            panel.AutoSize = true;
            panel.Tag = item;
            var offset = 0;
            Dictionary<object, Control> controls = new Dictionary<object, Control>();
            foreach (var c in this.Columns)
            {
                var control = c.ControlGetter(item);
                control.Location = new Vector2(offset + c.Width * c.Anchor, 0);
                control.Anchor = new Vector2(c.Anchor, 0);
                offset += c.Width;
                panel.AddControls(control);
                controls.Add(c.Tag ?? c.Object, control);

            }
            panel.Size = new Rectangle(0, 0, this.ColumnLabels.Width, panel.Height);
            panel.Controls.AlignCenterHorizontally();
            this.Rows.Add(item, controls);
            this.BoxItems.Add(panel);
        }
        public void AddItems(params TObject[] items)
        {
            foreach (var item in items)
            {
                this.AddItem(item);
            }
        }
        public void AddItems(IEnumerable<TObject> items)
        {
            foreach (var item in items)
            {
                this.AddItem(item);
            }
        }
        public void RemoveItem(TObject item)
        {
            var row = this.Rows[item];
            this.Rows.Remove(item);
            var prev = 0;
            foreach (var r in this.BoxItems.Client.Controls.Where(c => !c.Tag.Equals(item)))
            {
                r.Location.Y = prev;
                prev = r.Bottom;
            }
            this.BoxItems.Remove(this.BoxItems.Client.Controls.First(c => c.Tag.Equals(item)));
        }
        public void RemoveItems(Func<TObject, bool> condition)
        {
            foreach (var row in this.Rows.Where(i => condition(i.Key)).ToList())
            {
                this.RemoveItem(row.Key);
            }
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
        class Column
        {
            public object Tag;
            public string Object;
            public int Width;
            public Func<TObject, Control> ControlGetter;
            public float Anchor;
            public Control ColumnHeader;

            public Column(object tag, string obj, int width, Func<TObject, Control> control, float anchor)
            {
                this.Tag = tag;
                this.Object = obj;
                this.Width = width;
                this.ControlGetter = control;
                this.Anchor = anchor;
            }
            public Column(object tag, Control columnHeader, int spacing, Func<TObject, Control> control, float anchor)
            {
                this.Tag = tag;
                this.ColumnHeader = columnHeader;
                this.Width = this.ColumnHeader.Width + spacing;
                this.ControlGetter = control;
                this.Anchor = anchor;
            }
        }
    }

}
