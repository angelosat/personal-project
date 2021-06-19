using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI
{
    public enum View { Text, Icon, IconOnly }

    public class ListView : Control
    {
        public class ListViewItemCollection
        {
            Vector2 NextPosition = Vector2.Zero;
            ListView Owner;
            List<ListViewItem> Collection;
            public ListViewItemCollection(ListView owner)
            {
                Collection = new List<ListViewItem>();
                Owner = owner;
            }

            public void Add(ListViewItem item)
            {
                item.ListView = Owner;
                item.Position = NextPosition;
                Collection.Add(item);
                NextPosition.X += Owner.ItemBackground.Width;
                if (NextPosition.X >= Owner.Width)
                {
                    NextPosition.X = 0;
                    NextPosition.Y += Owner.ItemBackground.Height;
                }
            }

            public List<ListViewItem>.Enumerator GetEnumerator()
            {
                return Collection.GetEnumerator();
            }

            public void Clear()
            {
                Collection.Clear();
            }

            public int Count
            {
                get { return Collection.Count; }
            }
        }

        //public Rectangle ItemSize = new Rectangle(0, 0, 32, 32);
        public Texture2D ItemBackground;
        public ListViewItem MouseHoverItem;
        public event EventHandler<DrawListViewItemEventArgs> DrawItem;
        void OnDrawItem(DrawListViewItemEventArgs e)
        {
            if (DrawItem != null)
                DrawItem(this, e);
        }
        public bool OwnerDraw;
        public event EventHandler ItemActivate;
        protected void OnItemActivate()
        {
            if (ItemActivate != null)
                ItemActivate(this, EventArgs.Empty);
        }

        public List<ListViewItem> SelectedItems = new List<ListViewItem>();

        public View View { get; set; }
        public string IconPropertyName, LabelPropertyName;

        public ListViewItemCollection Items;
        public ListViewGroupCollection Groups;


        //RenderTarget2D box;
        int Unit;
        public int BoxY = 0;

        public int ItemHeight { get; set; }
        public int ItemWidth { get; set; }

        public DrawMode DrawMode = DrawMode.Normal;


        public ListViewItem FocusedItem { get; set; }

        public ListView()
        {
            Items = new ListViewItemCollection(this);
            AutoSize = true;
        }
        public ListView(Vector2 location)
            : base(location)
        {
            AutoSize = true;
            location = location - new Vector2(UIManager.BorderPx);
            ItemHeight = Label.DefaultHeight;
            Unit = UIManager.DefaultButtonHeight;
        }

        public ListView(Vector2 location, Rectangle size)
            : base(location)
        {
            location = location - new Vector2(UIManager.BorderPx);
            Size = size;
            ItemHeight = Label.DefaultHeight;
            Unit = UIManager.DefaultButtonHeight;

            Items = new ListViewItemCollection(this);
            //box = new RenderTarget2D(Game1.Instance.graphics.GraphicsDevice, Width, Height, false, SurfaceFormat.Color, DepthFormat.None, 1, RenderTargetUsage.PreserveContents);
        }

        void listitem_Click(object sender, EventArgs e)
        {
            SelectedItems.Add(sender as ListViewItem);
            OnItemActivate();
        }

        public int HoverIndex;
        void label_KeyPress(object sender, KeyPressEventArgs2 e)
        {
            OnKeyPress(e);
        }

        void scrollbar_Scroll(Object sender, ScrollEventArgs e)
        {
            BoxY = e.NewValue;
        }

        public override void Dispose()
        {
            foreach (Control control in Controls)
            {
                control.LeftClick -= listitem_Click;
                control.Dispose();
            }
        }

        public override void Validate(bool cascade = false)
        {
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            SpriteBatch sb = Game1.Instance.spriteBatch;
            RenderTarget2D Texture;
            Texture = new RenderTarget2D(gfx, Width, Height);
            gfx.SetRenderTarget(Texture);
            gfx.Clear(Color.Transparent);
            sb.Begin();

            sb.End();
            BackgroundTexture = Texture;
            base.Validate(cascade);
        }

        public virtual void Clear()
        {
            foreach (ListItem item in Controls)
            {
                item.LeftClick -= listitem_Click;
                item.KeyPress -= label_KeyPress;
                item.Dispose();
            }
            Controls.Clear();

            Items.Clear();
        }

        public void Add(Object obj, ListViewGroup group = null)
        {
            ListViewItem listitem = new ListViewItem();
            listitem.Icon = (Icon)obj.GetType().GetProperty(IconPropertyName).GetValue(obj, null);
            listitem.ListView = this;
            listitem.Index = Controls.Count;
            listitem.Location = new Vector2(0, listitem.Index * UIManager.LargeButton.Height);
            listitem.Tag = obj;
            listitem.LeftClick+=new UIEvent(listitem_Click);
            switch (View)
            {
                case View.IconOnly:
                    listitem.Size = new Rectangle(0, 0, UIManager.LargeButton.Height, UIManager.LargeButton.Height);
                    break;
                default:
                    break;
            }
            listitem.Validate();
            Controls.Add(listitem);
        }

        public void AddRange(Object[] objects, ListViewGroup group = null)
        {
            foreach (Object obj in objects)
                Add(obj, group);
        }

        public override void Draw(SpriteBatch sb)
        {
            MouseHoverItem = null;
            if (OwnerDraw)
                foreach (ListViewItem listItem in Items)
                    OnDrawItem(new DrawListViewItemEventArgs(sb, new Rectangle((int)ScreenLocation.X, (int)ScreenLocation.Y, ItemBackground.Width, ItemBackground.Height), listItem));
            else
            {

                foreach (ListViewItem item in Items)
                {
                    
                    Color color;
                    if (Controller.Instance.MouseRect.Intersects(item.BoundsScreen))
                    {
                        MouseHoverItem = item;
                        color = Color.White;
                    }
                    else
                        color = Color.Lerp(Color.Transparent, Color.White, 0.5f);
                    Vector2 screenLoc = item.Position + ScreenLocation;
                    sb.Draw(ItemBackground, screenLoc, color);

                    if (item.IconIndex > -1)
                        sb.Draw(Map.ItemSheet, screenLoc + new Vector2(ItemBackground.Width / 2, ItemBackground.Height / 2), Map.Icons[item.IconIndex], Color.White, 0, new Vector2(Map.Icons[item.IconIndex].Width / 2, Map.Icons[item.IconIndex].Height / 2), 1, SpriteEffects.None, 0);

                }
            }
            //base.Draw(sb);
        }
    }
}

