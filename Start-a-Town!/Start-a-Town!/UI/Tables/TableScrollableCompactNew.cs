using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    class TableScrollableCompactNew<TObject> : GroupBox
    {
        readonly List<Column> Columns = new List<Column>();
        readonly GroupBox ColumnLabels;
        ScrollableBoxNew BoxItems;
        readonly Dictionary<TObject, Dictionary<object, Control>> Rows = new Dictionary<TObject, Dictionary<object, Control>>();
        public int MaxVisibleItems;
        public bool ShowColumnLabels = true;
        public Color ClientBoxColor = Color.Black * .5f;
        private readonly ScrollableBoxNew.ScrollModes ScrollBarMode;

        public TableScrollableCompactNew(int maxVisibleItems, bool showColumnLabels = false, ScrollableBoxNew.ScrollModes scrollbarMode = ScrollableBoxNew.ScrollModes.Vertical)
        {
            this.MaxVisibleItems = maxVisibleItems;
            this.ShowColumnLabels = showColumnLabels;
            this.ColumnLabels = new GroupBox() { AutoSize = true, BackgroundColor = Color.Black * .5f };
            this.ScrollBarMode = scrollbarMode;
        }
        public TableScrollableCompactNew<TObject> AddColumn(object index, Control columnHeader, int spacing, Func<TObject, Control> control, float anchor = .5f)
        {
            this.Columns.Add(new Column(index, columnHeader, spacing, control, anchor));
            return this;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="width"></param>
        /// <param name="control"></param>
        /// <param name="anchor">Value between 0 and 1 for horizontal alignment</param>
        /// <returns></returns>
        public TableScrollableCompactNew<TObject> AddColumn(object tag, string type, int width, Func<TObject, Control> control, float anchor = 0)
        {
            this.Columns.Add(new Column(tag, type, width, control, anchor));
            this.Build(new List<TObject>());
            return this;
        }
        public TableScrollableCompactNew<TObject> AddColumn(object tag, string type, int width, Func<TableScrollableCompactNew<TObject>, TObject, Control> control, float anchor = 0)
        {
            this.Columns.Add(new Column(tag, type, width, item => control(this, item), anchor));
            this.Build(new List<TObject>());
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
        public void Build(IEnumerable<TObject> items)
        {
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
            this.BoxItems = new ScrollableBoxNew(new Rectangle(0, 0, offset, Label.DefaultHeight * this.MaxVisibleItems), this.ScrollBarMode);// ScrollableBoxNew.Modes.Vertical);

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
            var panel = new GroupBox() { BackgroundColor = this.ClientBoxColor };
            panel.AutoSize = true;
            panel.Tag = item;
            var offset = 0;
            var controls = new Dictionary<object, Control>();
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
            this.BoxItems.Client.AddControlsBottomLeft(panel);
            this.BoxItems.UpdateClientSize();
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
        public void RemoveItems(params TObject[] items)
        {
            foreach (var item in items)
            {
                this.RemoveItem(item);
            }
        }
        public void RemoveItems(IEnumerable<TObject> items)
        {
            foreach (var item in items)
            {
                this.RemoveItem(item);
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
        public void Clear()
        {
            this.BoxItems.Client.ClearControls();
            this.Rows.Clear();
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
