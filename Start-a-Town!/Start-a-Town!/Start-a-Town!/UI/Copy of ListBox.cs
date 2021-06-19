using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI
{
    public class ListBox : ListControl
    {
        Object _SelecetedItem;
        public Object SelectedItem
        {
            get { return _SelecetedItem; }
            set { _SelecetedItem = value; }
        }

        RenderTarget2D box;
        int Unit;
        public int BoxY = 0;

        public int ItemHeight { get; set; }
        public int ItemWidth { get; set; }
        public event EventHandler<DrawItemEventArgs> DrawItem;
        public event EventHandler<MeasureItemEventArgs> MeasureItem;
        public DrawMode DrawMode = DrawMode.Normal;
        public List<Control> Clickies = new List<Control>();

        public ListBox(Vector2 location)
            : base(location)
        {
            //Controls = new List<Control>();
            AutoSize = true;
            location = location - new Vector2(UIManager.BorderPx);
            ItemHeight = Label.DefaultHeight;
            Unit = UIManager.DefaultButtonHeight;

            Items = new ObjectCollection(this);
            //box = new RenderTarget2D(Game1.Instance.graphics.GraphicsDevice, Width, Height, false, SurfaceFormat.Color, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
        }

        public ListBox(Vector2 location, Rectangle size)
            : base(location)
        {
            //Controls = new List<Control>();
            location = location - new Vector2(UIManager.BorderPx);
            Size = size;
            //ItemHeight = UIManager.Font.LineSpacing;
            ItemHeight = Label.DefaultHeight;
            Unit = UIManager.DefaultButtonHeight;

            Items = new ObjectCollection(this);
            box = new RenderTarget2D(Game1.Instance.graphics.GraphicsDevice, Width, Height, false, SurfaceFormat.Color, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
        }

        public override void Build()
        {
            if(AutoSize)
                Height = 0;
            if (Controls.Count > 0)
                foreach (Control control in Controls)
                    control.Dispose();
            Controls.Clear();
            int n = 0;
            foreach (object item in Items)
            {
                ListItem label;
                if (DisplayMember != "")
                {
                    label = new ListItem(new Vector2(0, n * ItemHeight), (string)item.GetType().GetProperty(DisplayMember).GetValue(item, null), HorizontalAlignment.Left);
                }
                else
                {
                    if (DisplayMemberFunc != null)
                        label = new ListItem(new Vector2(0, n * ItemHeight), DisplayMemberFunc(item), HorizontalAlignment.Left);
                    else
                        label = new ListItem(new Vector2(0, n * ItemHeight), item.ToString(), HorizontalAlignment.Left);
                }
                label.Index = n++;
                label.Item = item;
                label.CustomTooltip = CustomTooltip;
                label.DrawTooltip += new EventHandler<TooltipArgs>(label_DrawTooltip);
                label.Tag = Items[label.Index];
                //label.Width = Math.Max(label.Width, Width);
                //Height += label.Height;
                if (AutoSize)
                {
                    Height += ItemHeight; // label.Height;
                    Width = Math.Max(Width, label.Width);
                }
                else
                {
                    label.Width = Width;
                }
                Controls.Add(label);
                //label.MouseLeftPress += new EventHandler<EventArgs>(label_MouseLeftPress);
                label.Click += new UIEvent(label_Click);
                label.KeyPress += new EventHandler<KeyPressEventArgs2>(label_KeyPress);
            }
            foreach (Control c in Controls)
                c.Width = Width;

            //if (AutoSize)
                Height = Math.Max(Height, Controls.Count * ItemHeight);
        }

        void label_DrawTooltip(object sender, TooltipArgs e)
        {
            OnDrawTooltip(e);
        }

        public int HoverIndex;
        void label_KeyPress(object sender, KeyPressEventArgs2 e)
        {
            //Console.WriteLine(e.Key + " " + (sender as ListItem).Index);
            OnKeyPress(e);
        }

        void label_Click(object sender, EventArgs e)
        {
            SelectedIndex = Controls.FindIndex(foo => foo == sender as Label);
            object item = Items[SelectedIndex];
            if (ValueMember != "")
            {
                Type type = item.GetType();
                foreach (System.Reflection.PropertyInfo info in type.GetProperties())
                {
                    Console.WriteLine(info.Name);
                }
                System.Reflection.PropertyInfo property = type.GetProperty(ValueMember);
                SelectedValue = property.GetValue(item, null);
                //SelectedValue = item.GetType().GetProperty(ValueMember).GetValue(item, null);
            }
            else
                SelectedValue = item;
            //OnMouseLeftPress();
            OnClick();
        }

        //void label_MouseLeftUp(object sender, EventArgs e)
        //{
        //    OnMouseLeftUp();
        //}

        void label_MouseLeftPress(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            SelectedIndex = Controls.FindIndex(foo => foo == sender as Label);
            object item = Items[SelectedIndex];
            if (ValueMember != "")
                SelectedValue = item.GetType().GetProperty(ValueMember).GetValue(item, null);
            else
                SelectedValue = item;
            OnMouseLeftPress(e);
        }

        void scrollbar_Scroll(Object sender, ScrollEventArgs e)
        {
            BoxY = e.NewValue;
        }

        public override void Dispose()
        {
            foreach (Control control in Controls)
            {
                control.Click -= label_Click;
                //control.MouseLeftPress -= label_MouseLeftPress;
                //control.MouseLeftUp -= label_MouseLeftUp;
                control.Dispose();
            }
        }

        //protected override void OnClick()
        //{
        //    foreach (Control item in Controls)
        //    {
        //        if (item.HitTest())
        //        {
        //            SelectedIndex = Controls.FindIndex(foo => foo == item);
        //            SelectedItem = Items[SelectedIndex];
        //        }
        //    }
        //    //base.OnClick();
        //}

        //protected override void Parent_MouseLeftPress(object sender, EventArgs e)
        //{
        //    foreach (Control item in Controls)
        //    {
        //        //if (item.HitTest())
        //        if ((new Rectangle(Controller.msCurrent.X, Controller.msCurrent.Y, 1, 1)).Intersects(new Rectangle((int)item.ScreenLocation.X, (int)item.ScreenLocation.Y, Width, item.Height)))
        //        {
        //            SelectedIndex = Controls.FindIndex(foo => foo == item);
        //            SelectedValue = Items[SelectedIndex].GetType().GetProperty(ValueMember).GetValue(Items[SelectedIndex], null);
        //            //Console.WriteLine((string)SelectedValue);
        //            //SelectedItem = Items[SelectedIndex];
        //        }
        //    }
        //    base.Parent_MouseLeftPress(sender, e);
        //}

        //protected override void OnMouseLeftPress()
        //{
        //    foreach (Control item in Controls)
        //    {
        //        if ((new Rectangle(Controller.msCurrent.X, Controller.msCurrent.Y, 1, 1)).Intersects(new Rectangle((int)item.ScreenLocation.X, (int)item.ScreenLocation.Y, Width, item.Height)))
        //        {
        //            SelectedIndex = Controls.FindIndex(foo => foo == item);
        //            SelectedValue = Items[SelectedIndex].GetType().GetProperty(ValueMember).GetValue(Items[SelectedIndex], null);
        //        }
        //    }
        //    base.OnMouseLeftPress();
        //}

        //void Item_Click(Object sender, EventArgs e)
        //{
        //    //SelectedValue = ObjectCollection.FindIndex(foo => foo == sender);
        //    OnSelectedValueChanged();
        //}


        void scrollbar_ScrollbarChange(Control sender)
        {
            Console.WriteLine("yeah baben");
        }
        public override void Update()
        {
            base.Update();
            //if (scrollbar != null)
            //    scrollbar.Update();
            if (Controls.Count < Items.Count)
                Build();
            foreach (Control item in Controls)
                item.Update();
        }

        public override void Paint()
        {
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            SpriteBatch sb = Game1.Instance.spriteBatch;
            RenderTarget2D Texture;
            Texture = new RenderTarget2D(gfx, Width, Height);
            gfx.SetRenderTarget(Texture);
            gfx.Clear(Color.Transparent);
            sb.Begin();

            sb.End();
            Background = Texture;
            base.Paint();
        }

        public override void Draw(SpriteBatch sb)
        {
            //DrawHighlight(sb);
            //for (int i = 0; i < Controls.Count; i++)
            HoverIndex = -1;
            for (int i = 0; i < Items.Count; i++)
            {
                Object item = Items[i];
                if (DrawMode == DrawMode.Normal)
                {
                    Control listitem = Controls[i];

                    if (i == SelectedIndex)
                        listitem.DrawHighlight(sb, 0.5f);
                        //sb.Draw(UIManager.Highlight, new Rectangle((int)Controls[i].ScreenLocation.X, (int)Controls[i].ScreenLocation.Y, Width, listitem.Height), Color.White);



                    if (WindowManager.ActiveControl == listitem)
                    {
                        listitem.DrawHighlight(sb, 0.2f);
                        HoverIndex = i;
                    }
                        //sb.Draw(UIManager.Highlight, listitem.Bounds, Color.Lerp(Color.Transparent, Color.White, 0.5f));
                    listitem.Draw(sb);
                    //if (DisplayMember != "")
                    //{
                    //    //UIManager.DrawTextOutlined((string)item.GetType().GetProperty(DisplayMember).GetValue(item, null), TextAlignment.Left);
                    //    sb.DrawString(UIManager.Font, (string)item.GetType().GetProperty(DisplayMember).GetValue(item, null), ScreenLocation + new Vector2(0, i * ItemHeight), Color.White);
                    //}
                    //else
                    //{
                    //    if (DisplayMemberFunc != null)
                    //        //UIManager.DrawTextOutlined(DisplayMemberFunc(item), TextAlignment.Left);
                    //        sb.DrawString(UIManager.Font, DisplayMemberFunc(item), ScreenLocation + new Vector2(0, i * ItemHeight), Color.White);
                    //    else
                    //        //UIManager.DrawTextOutlined(item.ToString(), TextAlignment.Left);
                    //        sb.DrawString(UIManager.Font, item.ToString(), ScreenLocation + new Vector2(0, i * ItemHeight), Color.White);
                    //}
                }
                else
                {
                    if (DrawItem != null)
                    {
                        if (DrawMode == DrawMode.OwnerDrawVariable)
                            if (MeasureItem != null)
                                MeasureItem(this, new MeasureItemEventArgs(i));
                        DrawItem(Items[i], new DrawItemEventArgs(sb, new Rectangle(0, i * ItemHeight, ItemWidth, ItemHeight)));
                    }
                }
            }
        }

        //public override void Draw(SpriteBatch sb)
        //{
        //    //for (int i = 0; i < Controls.Count; i++)
        //    if (DrawMode == DrawMode.Normal)
        //        sb.Draw(Sprite, ScreenLocation, Color.White);

        //    else
        //        if (DrawItem != null)
        //            for (int i = 0; i < Items.Count; i++)
        //            {
        //                Object item = Items[i];

        //                if (DrawMode == DrawMode.OwnerDrawVariable)
        //                    if (MeasureItem != null)
        //                        MeasureItem(this, new MeasureItemEventArgs(i));
        //                DrawItem(Items[i], new DrawItemEventArgs(sb, new Rectangle(0, i * ItemHeight, ItemWidth, ItemHeight)));

        //            }
        //}

        public virtual void Clear()
        {
            foreach (ListItem item in Controls)
            {
                item.Click -= label_Click;
                item.KeyPress -= label_KeyPress;
                item.Dispose();
            }
            Controls.Clear();

            Items.Clear();

            //Width = 0;
            //Height = 0;
        }
    }
}

