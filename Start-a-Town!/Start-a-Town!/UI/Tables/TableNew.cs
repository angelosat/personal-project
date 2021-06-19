﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class TableNew<TObject> : GroupBox
    {
        List<Column> Columns = new List<Column>();
        Panel ColumnLabels;
        GroupBox BoxItems;
        Dictionary<TObject, Dictionary<object, Control>> Inner = new Dictionary<TObject, Dictionary<object, Control>>();
        //public int MaxVisibleItems = -1;

        public TableNew()
        {
            this.BoxItems = new GroupBox();
            this.ColumnLabels = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.Window };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="spacing"></param>
        /// <param name="control"></param>
        /// <param name="anchor">Value between 0 and 1 for horizontal alignment</param>
        /// <returns></returns>
        public TableNew<TObject> AddColumn(object tag, string type, int spacing, Func<TObject, Control> control, float anchor = .5f)
        {
            this.Columns.Add(new Column(tag, type, spacing, control, anchor));
            return this;
        }
        public TableNew<TObject> AddColumn(object tag, Control columnHeader, int spacing, Func<TObject, Control> control, float anchor = .5f)
        {
            this.Columns.Add(new Column(tag, columnHeader, spacing, control, anchor));
            return this;
        }

        public void Build(IEnumerable<TObject> items)
        {
            this.Inner.Clear();
            this.Controls.Clear();
            this.ColumnLabels.Controls.Clear();
            this.BoxItems.Controls.Clear();
            int offset = 0;
            foreach (var c in this.Columns)
            {
                if (c.ColumnHeader != null)
                {
                    c.ColumnHeader.Location = new Vector2(offset + c.Width / 2f, 0);
                    c.ColumnHeader.Anchor = new Vector2(.5f, 0);
                    offset += c.Width;
                    this.ColumnLabels.AddControls(c.ColumnHeader);
                }
                else
                {
                    var label = new Label(new Vector2(offset + c.Width / 2f, this.ColumnLabels.Height / 2), c.Object);// { Location = new Vector2(offset + c.Spacing, 0), Halign = HorizontalAlignment.Center };//{ Location = new Vector2(offset + c.Spacing, 0) };
                    label.Anchor = new Vector2(.5f, 0);
                    //label.TextHAlign = HorizontalAlignment.Center;
                    offset += c.Width;
                    this.ColumnLabels.AddControls(label);
                }
            }
            this.ColumnLabels.ClientSize = new Rectangle(0, 0, this.Columns.Sum(c => c.Width), this.ColumnLabels.ClientSize.Height);
            this.Controls.Add(this.ColumnLabels);
            if (items.Count() == 0)
                return;
            this.BoxItems.Location = this.ColumnLabels.BottomLeft;
            foreach (var item in items)
            {

                var panel = new Panel() { Location = new Vector2(0, this.BoxItems.Controls.BottomLeft.Y) }; // Size = this.ColumnLabels.Size, BackgroundStyle = BackgroundStyle.Window, 

                panel.BackgroundStyle = BackgroundStyle.Window;
                //panel.Size = this.ColumnLabels.Size;
                panel.AutoSize = true;

                offset = 0;
                Dictionary<object, Control> controls = new Dictionary<object, Control>();
                foreach (var c in this.Columns)
                {
                    var control = c.ControlGetter(item);
                    control.Location = new Vector2(offset + c.Width * c.Anchor, 0);
                    control.Anchor = new Vector2(c.Anchor, 0);

                    offset += c.Width;
                    panel.AddControls(control);
                    if (c.Tag != null)
                        controls.Add(c.Tag, control);
                }
                panel.Width = this.ColumnLabels.Width;
                this.Inner.Add(item, controls);
                this.BoxItems.AddControls(panel);
            }
            this.Controls.Add(this.BoxItems);
        }

        public Control GetItem(TObject row, object column)
        {
            return this.Inner[row][column];
        }

        class Column
        {
            public object Tag;
            public string Object;
            public int Width;
            public Func<TObject, Control> ControlGetter;
            public float Anchor;
            //private object tag;
            //private Func<object, Control> ColumnHeaderGetter;
            public Control ColumnHeader;
            //private Func<TObject, Control> control;
            //private float anchor;
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
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            //viewport.DrawHighlight(sb, .5f);
            base.Draw(sb, viewport);
        }
    }
}
