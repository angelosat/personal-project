using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class TableScrollable<TObject> : GroupBox
    {
        List<Column> Columns = new List<Column>();
        Panel ColumnLabels;
        ScrollableBox BoxItems;
        Dictionary<TObject, Dictionary<object, Control>> Rows = new Dictionary<TObject, Dictionary<object, Control>>();
        public int MaxVisibleItems;// = -1;
        BackgroundStyle ItemStyle = BackgroundStyle.Window;

        public TableScrollable(int maxVisibleItems, BackgroundStyle style)
            : this(maxVisibleItems)
        {
            this.ItemStyle = style;
        }
        public TableScrollable(int maxVisibleItems)//, BackgroundStyle itemStyle = BackgroundStyle.Window)
        {
            this.MaxVisibleItems = maxVisibleItems;
            //this.ItemStyle = itemStyle;
            this.ColumnLabels = new Panel() { AutoSize = true };//, BackgroundStyle = this.ItemStyle };//BackgroundStyle.Window };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="spacing"></param>
        /// <param name="control"></param>
        /// <param name="anchor">Value between 0 and 1 for horizontal alignment</param>
        /// <returns></returns>
        public TableScrollable<TObject> AddColumn(object tag, string type, int spacing, Func<TObject, Control> control, float anchor = .5f)
        {
            this.Columns.Add(new Column(tag, type, spacing, control, anchor));
            this.Build(new List<TObject>());
            return this;
        }

        public void Build(IEnumerable<TObject> items)
        {
            this.Rows.Clear();
            this.Controls.Clear();
            this.ColumnLabels.Controls.Clear();
            this.ColumnLabels.BackgroundStyle = this.ItemStyle;
            int offset = 0;
            foreach (var c in this.Columns)
            {
                //var label = new Label(new Vector2(offset + c.Width / 2f, 0), c.Object);// { Location = new Vector2(offset + c.Spacing, 0), Halign = HorizontalAlignment.Center };//{ Location = new Vector2(offset + c.Spacing, 0) };
                //label.Anchor = new Vector2(.5f, 0);
                var label = new Label(new Vector2(offset + c.Width / 2f, 0), c.Object);// { Location = new Vector2(offset + c.Spacing, 0), Halign = HorizontalAlignment.Center };//{ Location = new Vector2(offset + c.Spacing, 0) };
                label.Anchor = new Vector2(.5f, 0);
                offset += c.Width;
                this.ColumnLabels.AddControls(label);

            }
            this.ColumnLabels.ClientSize = new Rectangle(0, 0, this.Columns.Sum(c => c.Width), this.ColumnLabels.ClientSize.Height);
            this.Controls.Add(this.ColumnLabels);

            this.BoxItems = new ScrollableBox(new Rectangle(0, 0, this.ColumnLabels.Width, this.ColumnLabels.Height * this.MaxVisibleItems));
            this.BoxItems.Location = this.ColumnLabels.BottomLeft;
            this.AddItems(items.ToArray());
            //foreach (var item in items)
            //{
            //    var panel = new Panel() { Location = new Vector2(0, this.BoxItems.Client.Controls.BottomLeft.Y) }; // Size = this.ColumnLabels.Size, BackgroundStyle = BackgroundStyle.Window, 

            //    panel.BackgroundStyle = this.ItemStyle;// BackgroundStyle.Window;
            //    panel.Size = this.ColumnLabels.Size;
            //    panel.Tag = item;
            //    //var lbl = new Label(nameGetter(item));
            //    //panel.AddControls(lbl);
            //    offset = 0;
            //    Dictionary<object, Control> controls = new Dictionary<object, Control>();
            //    foreach (var c in this.Columns)
            //    {
            //        var control = c.ControlGetter(item);
            //        //control.Tag = c.Tag;
            //        //control.Location = new Vector2(offset + c.Width / 2f, 0);
            //        //control.Anchor = new Vector2(.5f, 0);
            //        control.Location = new Vector2(offset + c.Width * c.Anchor, 0);
            //        control.Anchor = new Vector2(c.Anchor, 0);

            //        offset += c.Width;
            //        panel.AddControls(control);
            //        //if (c.Tag != null)
            //            controls.Add(c.Tag ?? c.Object, control);
            //    }
            //    this.Rows.Add(item, controls);
            //    this.BoxItems.Add(panel);//.AddControls(panel);
            //}
            ////if (this.MaxVisibleItems > 0)
            ////{
            ////    this.BoxItems.AutoSize = false;
            ////    this.BoxItems.Size = new Rectangle(0, 0, this.ColumnLabels.Width, this.ColumnLabels.Height * this.MaxVisibleItems);
            ////}
            this.Controls.Add(this.BoxItems);

        }

        public Control GetElement(TObject row, object column)
        {
            return this.Rows[row][column];
        }
        public Control GetElement(Func<TObject, bool> row, object column)
        {
            if(!this.Rows.Any(v => row(v.Key)))
                return null;
            var r = this.Rows.FirstOrDefault(v => row(v.Key));
            return r.Value[column];
        }
        public void AddItem(TObject item)
        {
            var panel = new Panel() { Location = new Vector2(0, this.BoxItems.Client.Controls.BottomLeft.Y) };
            panel.BackgroundStyle = this.ItemStyle;
            panel.Size = this.ColumnLabels.Size;
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
                //if (c.Tag != null)
                //    controls.Add(c.Tag, control);
                controls.Add(c.Tag ?? c.Object, control);

            }
            this.Rows.Add(item, controls);
            this.BoxItems.Add(panel);
        }
        public void AddItems(params TObject[] items)
        {
            foreach (var item in items)
                this.AddItem(item);
        }
        public void RemoveItem(TObject item)
        {
            var row = this.Rows[item];
            this.Rows.Remove(item);
            //this.BoxItems.Client.Controls.RemoveAll(c => c.Tag.Equals(item));
            //this.BoxItems.Client.Controls.RemoveAll(c => c.Tag.Equals(item));

            var prev = 0;
            foreach (var r in this.BoxItems.Client.Controls.Where(c => !c.Tag.Equals(item)))
            {
                r.Location.Y = prev;
                prev = r.Bottom;
            }
            this.BoxItems.Remove(this.BoxItems.Client.Controls.First(c => c.Tag.Equals(item)));
            //Rearrange();
        }

        private void Rearrange()
        {
            var prev = 0;
            foreach (var row in this.BoxItems.Client.Controls)
            {
                row.Location.Y = prev;
                prev = row.Bottom;
            }
            this.BoxItems.Remeasure();
        }

        class Column
        {
            public object Tag;
            public string Object;
            public int Width;
            public Func<TObject, Control> ControlGetter;
            public float Anchor;
            public Column(object tag, string obj, int width, Func<TObject, Control> control, float anchor)
            {
                this.Tag = tag;
                this.Object = obj;
                this.Width = width;
                this.ControlGetter = control;
                this.Anchor = anchor;
            }
        }

    }
}
