using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class TableScrollableCompactNewNew<TObject> : GroupBox where TObject : class
    {
        List<Column> Columns = new List<Column>();
        GroupBox ColumnLabels;
        ListBoxNewNoBtnBase<TObject, GroupBox> BoxItems;
        Dictionary<TObject, Dictionary<object, Control>> Rows = new();
        public int MaxVisibleItems;// = -1;
        BackgroundStyle ItemStyle = BackgroundStyle.Window;
        public bool ShowColumnLabels = true;
        public Color ClientBoxColor = Color.Black * .5f;
        private ScrollableBoxNew.ScrollModes ScrollBarMode = ScrollableBoxNew.ScrollModes.Vertical;
        ObservableCollection<TObject> BoundCollection;
        static readonly int ItemHeight = Label.DefaultHeight + ListBoxNewNoBtnBase<TObject, GroupBox>.Spacing;
        public TableScrollableCompactNewNew(int maxVisibleItems, BackgroundStyle style)
            : this(maxVisibleItems)
        {
            this.ItemStyle = style;
        }
        public TableScrollableCompactNewNew(int maxVisibleItems, bool showColumnLabels = false, ScrollableBoxNew.ScrollModes scrollbarMode = ScrollableBoxNew.ScrollModes.Vertical)//, BackgroundStyle itemStyle = BackgroundStyle.Window)
        {
            this.MaxVisibleItems = maxVisibleItems;
            this.ShowColumnLabels = showColumnLabels;
            //this.ItemStyle = itemStyle;
            this.ColumnLabels = new GroupBox() { AutoSize = true, BackgroundColor = Color.SlateGray * .5f };// Color.Black * .5f };//, BackgroundStyle = this.ItemStyle };//BackgroundStyle.Window };
        }
        public TableScrollableCompactNewNew<TObject> Bind(ObservableCollection<TObject> collection)
        {
            if (this.BoundCollection != null)
                this.BoundCollection.CollectionChanged -= Collection_CollectionChanged;
            collection.CollectionChanged += Collection_CollectionChanged;
            this.BoundCollection = collection;
            this.ClearItems();
            //this.AddItems(this.Sorter(collection));
            this.AddItems(collection);
            return this;
        }
        //public TableScrollableCompactNewNew<TObject> Bind<U>(ObservableCollection<TObject> collection, Func<TObject, U> keyGetter, IComparer<U> comparer)
        //{
        //    if (this.BoundCollection != null)
        //        this.BoundCollection.CollectionChanged -= Collection_CollectionChanged;
        //    collection.CollectionChanged += Collection_CollectionChanged;
        //    this.BoundCollection = collection;
        //    this.ClearItems();
        //    this.Sorter = l => l.OrderBy(keyGetter, comparer);
        //    this.AddItems(this.Sorter(collection));// collection.OrderBy(keyGetter, comparer));
        //    return this;
        //}
        //Func<ICollection<TObject>, IEnumerable<TObject>> Sorter = c => c;
        private void Collection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddItems(e.NewItems?.Cast<TObject>());
            this.RemoveItems(e.OldItems?.Cast<TObject>());
        }

        public TableScrollableCompactNewNew<TObject> AddColumn(object index, Control columnHeader, int width, Func<TObject, Control> control, float anchor = .5f)
        {
            this.Columns.Add(new Column(index, columnHeader, width, control, anchor));
            this.Build();//, showColumnLabels);
            return this;
        }
        //public TableScrollableCompactNewNew<TObject> AddColumn(object index, Func<TableScrollableCompactNewNew<TObject>, Control> columnHeader, int spacing, Func<TObject, Control> control, float anchor = .5f)
        //{
        //    this.Columns.Add(new Column(index, columnHeader(this), spacing, control, anchor));
        //    this.Build();//, showColumnLabels);
        //    return this;
        //}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="width"></param>
        /// <param name="control"></param>
        /// <param name="anchor">Value between 0 and 1 for horizontal alignment</param>
        /// <returns></returns>
        public TableScrollableCompactNewNew<TObject> AddColumn(object tag, string type, int width, Func<TObject, Control> control, float anchor = 0)//, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
        {
            //this.ShowColumnLabels = showColumnLabels;
            this.Columns.Add(new Column(tag, type, width, control, anchor));
            this.Build();//, showColumnLabels);
            return this;
        }
        public TableScrollableCompactNewNew<TObject> AddColumn(object tag, string type, int width, Func<TableScrollableCompactNewNew<TObject>, TObject, Control> control, float anchor = 0)//, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
        {
            //this.ShowColumnLabels = showColumnLabels;
            this.Columns.Add(new Column(tag, type, width, item => control(this, item), anchor));
            this.Build();//, showColumnLabels);
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
        public TableScrollableCompactNewNew<TObject> Build()
        {
            return this.Build(new List<TObject>());
        }
        public TableScrollableCompactNewNew<TObject> Build(IEnumerable<TObject> items)//, bool showColumnLabels = true)
        {
            //this.ShowColumnLabels = showColumnLabels;
            this.Rows.Clear();
            this.Controls.Clear();
            this.ColumnLabels.Controls.Clear();
            //this.ColumnLabels.BackgroundStyle = this.ItemStyle;
            int offset = 0;
            foreach (var c in this.Columns)
            {
                if (c.ColumnHeader != null)
                {
                    //c.ColumnHeader.Location = new Vector2(offset + c.Width * .5f, 0);
                    //c.ColumnHeader.Location = new Vector2(offset + c.ColumnHeader.Width * .5f, 0);
                    //c.ColumnHeader.Anchor = new Vector2(.5f, 0);

                    //c.ColumnHeader.Location = new Vector2(offset, 0);


                    c.ColumnHeader.Location = new Vector2(offset + c.Width * c.Anchor, 0);
                    c.ColumnHeader.Anchor = new Vector2(c.Anchor, 0);

                    //c.ColumnHeader.Location = new Vector2(offset + (c.Width - c.ColumnHeader.Width) / 2, 0);

                    offset += c.Width;
                    this.ColumnLabels.AddControls(c.ColumnHeader);
                }
                else
                {
                    //var label = new Label(new Vector2(offset + c.Width / 2f, 0), c.Object);
                    //label.Anchor = new Vector2(.5f, 0);
                    var label = new Label(new Vector2(offset, 0), c.Label);
                    offset += c.Width;
                    label.TextHAlign = HorizontalAlignment.Center;
                    this.ColumnLabels.AddControls(label);
                }
            }
            // HACK
            this.ColumnLabels.AutoSize = false;
            //this.ColumnLabels.ClientSize = new Rectangle(0, 0, this.Columns.Sum(c => c.Width), this.ColumnLabels.ClientSize.Height);
            this.ColumnLabels.ClientSize = new Rectangle(0, 0, offset, this.ColumnLabels.ClientSize.Height);
            this.ColumnLabels.Width = offset; 
            //

            if (this.ShowColumnLabels)
                this.Controls.Add(this.ColumnLabels);
            this.ColumnLabels.Controls.AlignCenterHorizontally();
            //this.BoxItems = new ScrollableBoxNew(new Rectangle(0, 0, offset, Label.DefaultHeight * this.MaxVisibleItems), this.ScrollBarMode);// ScrollableBoxNew.Modes.Vertical);
            this.BoxItems = new(offset + ScrollbarV.Width, ItemHeight * this.MaxVisibleItems, (TObject item) =>
            {
                var panel = new GroupBox() { BackgroundColor = this.ClientBoxColor };
                panel.AutoSize = true;
                panel.Tag = item;
                var offset = 0;
                Dictionary<object, Control> controls = new();
                foreach (var c in this.Columns)
                {
                    var control = c.ControlGetter(item);
                    control.Location = new Vector2(offset + c.Width * c.Anchor, 0);
                    control.Anchor = new Vector2(c.Anchor, 0);
                    offset += c.Width;
                    panel.AddControls(control);
                    controls.Add(c.Tag ?? c.Label, control);

                }
                panel.Size = new Rectangle(0, 0, this.ColumnLabels.Width, panel.Height);
                panel.Controls.AlignCenterHorizontally();

                this.Rows.Add(item, controls);
                //this.BoxItems.Add(panel);
                return panel;
            });// ScrollableBoxNew.Modes.Vertical);


            if (this.ShowColumnLabels)
                this.BoxItems.Location = this.ColumnLabels.BottomLeft + Vector2.UnitY;
            this.AddItems(items.ToArray());

            this.Controls.Add(this.BoxItems);
            return this;
            //this.Validate(true);
        }

        public Control GetItem(TObject row, object column)
        {
            return this.Rows[row][column];
        }
        public Control GetItem(Func<TObject, bool> row, object column)
        {
            if (!this.Rows.Any(v => row(v.Key)))
                return null;
            var r = this.Rows.FirstOrDefault(v => row(v.Key));
            return r.Value[column];
        }
        public void AddItem(TObject item)
        {
            //var control = this.BoxItems.AddItem(item);

            //this.Rows.Add(item, control);
            ////this.BoxItems.Add(panel);
            //this.BoxItems.Client.AddControlsBottomLeft(panel);
            ////this.BoxItems.UpdateClientSize();

            var panel = new GroupBox(this.BoxItems.Client.Width, UIManager.Icon16Background.Height) { Location = new Vector2(0, this.BoxItems.Client.Controls.BottomLeft.Y), BackgroundColor = this.ClientBoxColor };
            //panel.BackgroundStyle = this.ItemStyle;
            panel.AutoSize = true;
            //panel.Size = this.ColumnLabels.Size;
            panel.Tag = item;
            var offset = 0;
            Dictionary<object, Control> controls = new Dictionary<object, Control>();
            foreach (var c in this.Columns)
            {
                var control = c.ControlGetter(item);
                control.Location = new Vector2(offset + c.Width * c.Anchor, 0);
                control.Anchor = new Vector2(c.Anchor, 0);
                //control.Location = new Vector2(offset, 0);
                offset += c.Width;
                panel.AddControls(control);
                //if (c.Tag != null)
                //    controls.Add(c.Tag, control);
                controls.Add(c.Tag ?? c.Label, control);

            }
            //panel.Size = new Rectangle(0, 0, this.ColumnLabels.Width, (int)Math.Max(this.ColumnLabels.Height, panel.Height));
            //panel.Size = new Rectangle(0, 0, this.ColumnLabels.Width, panel.Height);
            //panel.Size = new Rectangle(0, 0, this.BoxItems.Client.Width, panel.Height);

            panel.Controls.AlignCenterHorizontally();
            this.Rows.Add(item, controls);
            this.BoxItems.Add(panel);
        }
        public TableScrollableCompactNewNew<TObject> AddItems(params TObject[] items)
        {
            return this.AddItems(items as IEnumerable<TObject>);
            //this.BoxItems.AddItems(items);
            //this.Validate(true);
            //return this;
        }
        public TableScrollableCompactNewNew<TObject> AddItems(IEnumerable<TObject> items)
        {
            if (items == null)
                return this;
            this.BoxItems.AddItems(items);
            //foreach (var item in items)
            //    this.AddItem(item);
            this.Validate(true);
            return this;
        }
        public void RemoveItems(params TObject[] items)
        {
            foreach (var item in items)
                this.RemoveItem(item);
        }
        public TableScrollableCompactNewNew<TObject> RemoveItems(IEnumerable<TObject> items)
        {
            if (items != null)
                foreach (var item in items)
                    this.RemoveItem(item);
            return this;
        }
        public void RemoveItem(TObject item)
        {
            var row = this.Rows[item];
            this.Rows.Remove(item);

            //var prev = 0;
            //foreach (var r in this.BoxItems.Client.Controls.Where(c => !c.Tag.Equals(item)))
            //{
            //    r.Location.Y = prev;
            //    prev = r.Bottom;
            //}
            //this.BoxItems.Remove(this.BoxItems.Client.Controls.First(c => c.Tag.Equals(item)));
            this.BoxItems.RemoveItem(item);
            //Rearrange();
        }
        public void RemoveItems(Func<TObject, bool> condition)
        {
            foreach (var row in this.Rows.Where(i => condition(i.Key)).ToList())
                this.RemoveItem(row.Key);
        }
        public TableScrollableCompactNewNew<TObject> ClearItems()
        {
            this.BoxItems?.Client.ClearControls();
            this.Rows.Clear();
            
            return this;
        }
        public TableScrollableCompactNewNew<TObject> Clear()
        {
            this.ClearItems();
            this.Columns.Clear();
            this.Controls.Clear();
            this.ColumnLabels.Controls.Clear();
            return this;
        }
        //private void Rearrange()
        //{
        //    var prev = 0;
        //    foreach (var row in this.BoxItems.Client.Controls)
        //    {
        //        row.Location.Y = prev;
        //        prev = row.Bottom;
        //    }
        //    this.BoxItems.Remeasure();
        //}
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
        class Column
        {
            public object Tag;
            public string Label;
            public int Width;
            public Func<TObject, Control> ControlGetter;
            public float Anchor;
            public Control ColumnHeader;

            public Column(object tag, string obj, int width, Func<TObject, Control> control, float anchor)
            {
                this.Tag = tag;
                this.Label = obj;
                this.Width = width;
                this.ControlGetter = control;
                this.Anchor = anchor;
            }
            public Column(object tag, Control columnHeader, int width, Func<TObject, Control> control, float anchor)
            {
                this.Tag = tag;
                this.ColumnHeader = columnHeader;
                this.Width = width;
                //this.Width = this.ColumnHeader.Width + width;
                this.ControlGetter = control;
                this.Anchor = anchor;
            }
        }
    }



    class TableScrollableCompactNew<TObject> : GroupBox
    {
        List<Column> Columns = new List<Column>();
        GroupBox ColumnLabels;
        ScrollableBoxNew BoxItems;
        readonly Dictionary<TObject, Dictionary<object, Control>> Rows = new Dictionary<TObject, Dictionary<object, Control>>();
        public int MaxVisibleItems;// = -1;
        BackgroundStyle ItemStyle = BackgroundStyle.Window;
        public bool ShowColumnLabels = true;
        public Color ClientBoxColor = Color.Black * .5f;
        private ScrollableBoxNew.ScrollModes ScrollBarMode;

        public TableScrollableCompactNew(int maxVisibleItems, BackgroundStyle style)
            : this(maxVisibleItems)
        {
            this.ItemStyle = style;
        }
        public TableScrollableCompactNew(int maxVisibleItems, bool showColumnLabels = false, ScrollableBoxNew.ScrollModes scrollbarMode = ScrollableBoxNew.ScrollModes.Vertical)//, BackgroundStyle itemStyle = BackgroundStyle.Window)
        {
            this.MaxVisibleItems = maxVisibleItems;
            this.ShowColumnLabels = showColumnLabels;
            //this.ItemStyle = itemStyle;
            this.ColumnLabels = new GroupBox() { AutoSize = true, BackgroundColor = Color.Black * .5f };//, BackgroundStyle = this.ItemStyle };//BackgroundStyle.Window };
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
        public TableScrollableCompactNew<TObject> AddColumn(object tag, string type, int width, Func<TObject, Control> control, float anchor = 0)//, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
        {
            //this.ShowColumnLabels = showColumnLabels;
            this.Columns.Add(new Column(tag, type, width, control, anchor));
            this.Build(new List<TObject>());//, showColumnLabels);
            return this;
        }
        public TableScrollableCompactNew<TObject> AddColumn(object tag, string type, int width, Func<TableScrollableCompactNew<TObject>, TObject, Control> control, float anchor = 0)//, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
        {
            //this.ShowColumnLabels = showColumnLabels;
            this.Columns.Add(new Column(tag, type, width, item => control(this, item), anchor));
            this.Build(new List<TObject>());//, showColumnLabels);
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
        public void Build(IEnumerable<TObject> items)//, bool showColumnLabels = true)
        {
            //this.ShowColumnLabels = showColumnLabels;
            this.Rows.Clear();
            this.Controls.Clear();
            this.ColumnLabels.Controls.Clear();
            //this.ColumnLabels.BackgroundStyle = this.ItemStyle;
            int offset = 0;
            foreach (var c in this.Columns)
            {
                if (c.ColumnHeader != null)
                {
                    c.ColumnHeader.Location = new Vector2(offset + c.Width * .5f, 0);
                    c.ColumnHeader.Anchor = new Vector2(.5f, 0);
                    //c.ColumnHeader.Location = new Vector2(offset, 0);

                    offset += c.Width;
                    this.ColumnLabels.AddControls(c.ColumnHeader);
                }
                else
                {
                    //var label = new Label(new Vector2(offset + c.Width / 2f, 0), c.Object);
                    //label.Anchor = new Vector2(.5f, 0);
                    var label = new Label(new Vector2(offset, 0), c.Object);
                    offset += c.Width;
                    label.TextHAlign = HorizontalAlignment.Center;
                    this.ColumnLabels.AddControls(label);
                }
            }
            this.ColumnLabels.ClientSize = new Rectangle(0, 0, this.Columns.Sum(c => c.Width), this.ColumnLabels.ClientSize.Height);
            if (this.ShowColumnLabels)
                this.Controls.Add(this.ColumnLabels);
            this.ColumnLabels.Controls.AlignCenterHorizontally();
            //this.BoxItems = new ScrollableBoxNew(new Rectangle(0, 0, this.ColumnLabels.Width, Label.DefaultHeight * this.MaxVisibleItems));
            //this.BoxItems = new ScrollableBoxNew(new Rectangle(0, 0, offset + ScrollbarV.Width, Label.DefaultHeight * this.MaxVisibleItems), this.ScrollBarMode);// ScrollableBoxNew.Modes.Vertical);
            this.BoxItems = new ScrollableBoxNew(new Rectangle(0, 0, offset, Label.DefaultHeight * this.MaxVisibleItems), this.ScrollBarMode);// ScrollableBoxNew.Modes.Vertical);

            if (this.ShowColumnLabels)
                this.BoxItems.Location = this.ColumnLabels.BottomLeft + Vector2.UnitY;
            this.AddItems(items.ToArray());

            this.Controls.Add(this.BoxItems);

            //this.Validate(true);
        }

        public Control GetItem(TObject row, object column)
        {
            return this.Rows[row][column];
        }
        public Control GetItem(Func<TObject, bool> row, object column)
        {
            if (!this.Rows.Any(v => row(v.Key)))
                return null;
            var r = this.Rows.FirstOrDefault(v => row(v.Key));
            return r.Value[column];
        }
        public void AddItem(TObject item)
        {
            var panel = new GroupBox() { BackgroundColor = this.ClientBoxColor };
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
            //this.BoxItems.Add(panel);
            this.BoxItems.Client.AddControlsBottomLeft(panel);
            this.BoxItems.UpdateClientSize();
            //var panel = new GroupBox() { Location = new Vector2(0, this.BoxItems.Client.Controls.BottomLeft.Y), BackgroundColor = this.ClientBoxColor };
            ////panel.BackgroundStyle = this.ItemStyle;
            //panel.AutoSize = true;
            ////panel.Size = this.ColumnLabels.Size;
            //panel.Tag = item;
            //var offset = 0;
            //Dictionary<object, Control> controls = new Dictionary<object, Control>();
            //foreach (var c in this.Columns)
            //{
            //    var control = c.ControlGetter(item);
            //    control.Location = new Vector2(offset + c.Width * c.Anchor, 0);
            //    control.Anchor = new Vector2(c.Anchor, 0);
            //    //control.Location = new Vector2(offset, 0);
            //    offset += c.Width;
            //    panel.AddControls(control);
            //    //if (c.Tag != null)
            //    //    controls.Add(c.Tag, control);
            //    controls.Add(c.Tag ?? c.Object, control);

            //}
            ////panel.Size = new Rectangle(0, 0, this.ColumnLabels.Width, (int)Math.Max(this.ColumnLabels.Height, panel.Height));
            //panel.Size = new Rectangle(0, 0, this.ColumnLabels.Width, panel.Height);
            //panel.Controls.AlignCenterHorizontally();
            //this.Rows.Add(item, controls);
            //this.BoxItems.Add(panel);
        }
        public void AddItems(params TObject[] items)
        {
            foreach (var item in items)
                this.AddItem(item);
        }
        public void AddItems(IEnumerable<TObject> items)
        {
            foreach (var item in items)
                this.AddItem(item);
        }
        public void RemoveItems(params TObject[] items)
        {
            foreach (var item in items)
                this.RemoveItem(item);
        }
        public void RemoveItems(IEnumerable<TObject> items)
        {
            foreach (var item in items)
                this.RemoveItem(item);
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
        public void RemoveItems(Func<TObject, bool> condition)
        {
            foreach (var row in this.Rows.Where(i => condition(i.Key)).ToList())
                this.RemoveItem(row.Key);
        }
        public void Clear()
        {
            this.BoxItems.Client.ClearControls();
            this.Rows.Clear();
        }
        //private void Rearrange()
        //{
        //    var prev = 0;
        //    foreach (var row in this.BoxItems.Client.Controls)
        //    {
        //        row.Location.Y = prev;
        //        prev = row.Bottom;
        //    }
        //    this.BoxItems.Remeasure();
        //}
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


    class TableScrollableCompact<TObject> : GroupBox
    {
        List<Column> Columns = new List<Column>();
        GroupBox ColumnLabels;
        ScrollableBoxNew BoxItems;
        Dictionary<TObject, Dictionary<object, Control>> Rows = new Dictionary<TObject, Dictionary<object, Control>>();
        public int MaxVisibleItems;// = -1;
        BackgroundStyle ItemStyle = BackgroundStyle.Window;
        public bool ShowColumnLabels = true;
        public Color ClientBoxColor = Color.Black * .5f;
        public TableScrollableCompact(int maxVisibleItems, BackgroundStyle style)
            : this(maxVisibleItems)
        {
            this.ItemStyle = style;
        }
        public TableScrollableCompact(int maxVisibleItems)//, BackgroundStyle itemStyle = BackgroundStyle.Window)
        {
            this.MaxVisibleItems = maxVisibleItems;
            //this.ItemStyle = itemStyle;
            this.ColumnLabels = new GroupBox() { AutoSize = true, BackgroundColor = Color.Black * .5f };//, BackgroundStyle = this.ItemStyle };//BackgroundStyle.Window };
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
        public TableScrollableCompact<TObject> AddColumn(object tag, string type, int spacing, Func<TObject, Control> control, float anchor = 0, bool showColumnLabels = true)//float anchor = .5f, bool showColumnLabels = true)
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
            //this.ColumnLabels.BackgroundStyle = this.ItemStyle;
            int offset = 0;
            foreach (var c in this.Columns)
            {
                if (c.ColumnHeader != null)
                {
                    c.ColumnHeader.Location = new Vector2(offset + c.Width * .5f, 0);
                    c.ColumnHeader.Anchor = new Vector2(.5f, 0);
                    //c.ColumnHeader.Location = new Vector2(offset, 0);

                    offset += c.Width;
                    this.ColumnLabels.AddControls(c.ColumnHeader);
                }
                else
                {
                    //var label = new Label(new Vector2(offset + c.Width / 2f, 0), c.Object);
                    //label.Anchor = new Vector2(.5f, 0);
                    var label = new Label(new Vector2(offset, 0), c.Object);
                    offset += c.Width;
                    label.TextHAlign = HorizontalAlignment.Center;
                    this.ColumnLabels.AddControls(label);
                }
            }
            this.ColumnLabels.ClientSize = new Rectangle(0, 0, this.Columns.Sum(c => c.Width), this.ColumnLabels.ClientSize.Height);
            if (this.ShowColumnLabels)
                this.Controls.Add(this.ColumnLabels);
            this.ColumnLabels.Controls.AlignCenterHorizontally();
            this.BoxItems = new ScrollableBoxNew(new Rectangle(0, 0, this.ColumnLabels.Width, Label.DefaultHeight * this.MaxVisibleItems));

            if (this.ShowColumnLabels)
                this.BoxItems.Location = this.ColumnLabels.BottomLeft + Vector2.UnitY;
            this.AddItems(items.ToArray());

            this.Controls.Add(this.BoxItems);

            //this.Validate(true);
        }

        public Control GetItem(TObject row, object column)
        {
            return this.Rows[row][column];
        }
        public Control GetItem(Func<TObject, bool> row, object column)
        {
            if (!this.Rows.Any(v => row(v.Key)))
                return null;
            var r = this.Rows.FirstOrDefault(v => row(v.Key));
            return r.Value[column];
        }
        public void AddItem(TObject item)
        {
            var panel = new GroupBox() { Location = new Vector2(0, this.BoxItems.Client.Controls.BottomLeft.Y), BackgroundColor = this.ClientBoxColor };
            //panel.BackgroundStyle = this.ItemStyle;
            panel.AutoSize = true;
            //panel.Size = this.ColumnLabels.Size;
            panel.Tag = item;
            var offset = 0;
            Dictionary<object, Control> controls = new Dictionary<object, Control>();
            foreach (var c in this.Columns)
            {
                var control = c.ControlGetter(item);
                control.Location = new Vector2(offset + c.Width * c.Anchor, 0);
                control.Anchor = new Vector2(c.Anchor, 0);
                //control.Location = new Vector2(offset, 0);
                offset += c.Width;
                panel.AddControls(control);
                //if (c.Tag != null)
                //    controls.Add(c.Tag, control);
                controls.Add(c.Tag ?? c.Object, control);

            }
            //panel.Size = new Rectangle(0, 0, this.ColumnLabels.Width, (int)Math.Max(this.ColumnLabels.Height, panel.Height));
            panel.Size = new Rectangle(0, 0, this.ColumnLabels.Width, panel.Height);
            panel.Controls.AlignCenterHorizontally();
            this.Rows.Add(item, controls);
            this.BoxItems.Add(panel);
        }
        public void AddItems(params TObject[] items)
        {
            foreach (var item in items)
                this.AddItem(item);
        }
        public void AddItems(IEnumerable<TObject> items)
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
        public void RemoveItems(Func<TObject, bool> condition)
        {
            foreach (var row in this.Rows.Where(i => condition(i.Key)).ToList())
                this.RemoveItem(row.Key);
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
