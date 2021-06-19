using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class Table<TObject> : GroupBox
    {
        List<Column> Columns = new List<Column>();
        Panel ColumnLabels;
        GroupBox BoxItems;

        public Table()
        {
            //this.ColumnLabels = new GroupBox();
            this.BoxItems = new GroupBox();
            this.ColumnLabels = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.Window };
            //this.BoxItems = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.Window };

        }

        public Table<TObject> AddColumn(string type, int spacing, Func<Control> control)
        {
            this.Columns.Add(new Column(type, spacing, control));
            return this;
        }

        public void Build(IEnumerable<TObject> items, Func<TObject, string> nameGetter)
        {
            this.Controls.Clear();
            this.ColumnLabels.Controls.Clear();
            this.BoxItems.Controls.Clear();
            int offset = 0;
            foreach(var c in this.Columns)
            {
                //var label = new Label(c.Object) { Location = new Vector2(this.ColumnLabels.Controls.TopRight.X + c.Spacing, 0) };//{ Location = new Vector2(offset + c.Spacing, 0) };
                //var label = new Label(c.Object) { Location = new Vector2(offset + c.Spacing, 0) , Halign = HorizontalAlignment.Center};//{ Location = new Vector2(offset + c.Spacing, 0) };
                var label = new Label(new Vector2(offset + c.Spacing, 0), c.Object);// { Location = new Vector2(offset + c.Spacing, 0), Halign = HorizontalAlignment.Center };//{ Location = new Vector2(offset + c.Spacing, 0) };
                label.Anchor = new Vector2(.5f, 0);
                offset += c.Spacing;
                this.ColumnLabels.AddControls(label);
            }
            this.Controls.Add(this.ColumnLabels);

            this.BoxItems.Location = this.ColumnLabels.BottomLeft;
            foreach(var item in items)
            {
                var panel = new Panel() { Location = new Vector2(0, this.BoxItems.Controls.BottomLeft.Y) }; // Size = this.ColumnLabels.Size, BackgroundStyle = BackgroundStyle.Window, 
             
                panel.BackgroundStyle = BackgroundStyle.Window;
                //panel.Width = this.ColumnLabels.Width;
                //panel.Height = this.ColumnLabels.Height;
                //panel.Size = new Rectangle(0, 0, this.ColumnLabels.Width, this.ColumnLabels.Height);
                //panel.ClientSize = this.ColumnLabels.ClientSize;
                panel.Size = this.ColumnLabels.Size;
                var lbl = new Label(nameGetter(item));
                panel.AddControls(lbl);
                offset = 0;
                foreach(var c in this.Columns)
                {
                    var control = c.ControlGetter();
                    //control.Location = new Vector2(offset + c.Spacing, 0);
                    //control.Location = new Vector2(panel.Controls.TopRight.X + c.Spacing, 0);
                    control.Location = new Vector2(offset + c.Spacing, 0);
                    control.Anchor = new Vector2(.5f, 0);
                    offset += c.Spacing;

                    //offset += (int)control.Location.X;
                    panel.AddControls(control);
                }
                this.BoxItems.AddControls(panel);
            }
            this.Controls.Add(this.BoxItems);

        }

        class Column
        {
            //TObject Object;
            public string Object;
            public int Spacing;
            public Func<Control> ControlGetter;
            public Column(string obj, int spacing, Func<Control> control)
            {
                this.Object = obj;
                this.Spacing = spacing;
                this.ControlGetter = control;
            }
        }
    }
}
