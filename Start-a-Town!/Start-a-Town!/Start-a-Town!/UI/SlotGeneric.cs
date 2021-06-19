using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class Slot<T> : ButtonBase, IDropTarget, ITooltippable, IContextable where T: class, ISlottable
    {
        public Func<T, string> CornerTextFunc = item => "";
        Func<DragEventArgs, DragDropEffects> _DragDropAction = (args) => DragDropEffects.None;
        public Func<DragEventArgs, DragDropEffects> DragDropAction 
        { get { return _DragDropAction; } set { _DragDropAction = value; } }
        protected override void OnTextChanged()
        {

        }

        public override void OnPaint(SpriteBatch sb)
        {
            float a = (this.MouseHover && Active) ? 1 : 0.5f;
            a *= this.Alpha.A;
            //if (this.Tag.HasValue)
            //    Slot.Draw(sb, Tag, Vector2.Zero, SprFx, a, BottomRightLabel.Text); //
            //else


            
                sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, this.Color * a, 0, new Vector2(0), 1, SprFx, Depth);
                if (this.Tag.IsNull())
                {
                    sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, this.Color * a, 0, new Vector2(0), 1, SprFx, Depth);
                    return;
                }
                var color = this.Tag.GetSlotColor();
                sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, color * a, 0, new Vector2(0), 1, SprFx, Depth);
                Rectangle rect = new Rectangle(3, 3, Width - 6, Height - 6);

                this.Tag.DrawUI(sb, new Vector2(Width, Height) * 0.5f);
                //Icon icon = this.Tag.GetIcon();
                //sb.Draw(icon.SpriteSheet, new Vector2(Width, Height) * 0.5f, icon.SourceRect, Color.White, 0, new Vector2(icon.SourceRect.Width, icon.SourceRect.Height) * 0.5f, 1, SpriteEffects.None, 0);

                UIManager.DrawStringOutlined(sb, CornerTextFunc(Tag), new Vector2(Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        }

        //public override void OnAfterDraw(SpriteBatch sb, Rectangle viewport)
        //{


        //    //if (!this.Tag.HasValue)
        //    //    return;

        //    //// TODO: figure out betterway to restrict the icon size
        //    //Rectangle final, source;
        //    //Rectangle rect = new Rectangle((int)ScreenLocation.X + 3, (int)ScreenLocation.Y + 3, Width - 6, Height - 6);
            
        //    //Icon icon = Tag.GetIcon(); // TODO: cache the icon to optimize
        //    //rect.Clip(icon.SourceRect, viewport, out final, out source);

        //    //sb.Draw(icon.SpriteSheet, final, source, Color.White);

        //    //UIManager.DrawStringOutlined(sb, CornerTextFunc(Tag), ScreenLocation + new Vector2(Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        //}

        public event EventHandler<EventArgs> ItemChanged;
        void OnItemChanged()
        {
            if (ItemChanged != null)
                ItemChanged(this, EventArgs.Empty);
        }
        //T LastTag = null;// 
        T LastTag = default(T);
        string LastText = "";
        int LastStackSize = 0;
        public override void Update()
        {
            base.Update();
            if (this.Tag.IsNull())
                return;
            string newtext = this.Tag.GetCornerText();
            if (this.Tag != this.LastTag)
                this.Invalidate();
            else if (newtext != this.LastText)
                this.Invalidate();
            this.LastTag = this.Tag;
            this.LastText = newtext;
        }
        Action<ContextArgs> _ContextAction = (args) => { };
        public Action<ContextArgs> ContextAction
        {
            get { return _ContextAction; }
            set { _ContextAction = value; }
        }
        public event EventHandler<ContextArgs> ContextRequest;
        protected virtual void OnContextRequest(ContextArgs a)
        {
            ContextAction(a);
            if (ContextRequest != null)
                ContextRequest(this, a);
        }

        public static int DefaultHeight = 38;

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        public virtual Slot<T> SetTag(T tag)
        {
            this.Tag = tag;
            return this;
        }
        T _Tag;
        public new T Tag
        {
            get { return this._Tag; }
            set
            {
                this._Tag = value;
                //TintFunc = () => value.IsNull() ? Color.White : (value.HasValue ? value.Object.GetInfo().GetQualityColor() : Color.White);
                //this.Color = value.HasValue ? value.Object.GetInfo().GetQualityColor() : Color.White;
            }
        }

        public void Clear()
        {
            Tag = null;
            IconIndex = -1;
        }

        public Slot<T> SetBottomRightText(string text)
        {
            BottomRightLabel.Text = text;
            return this;
        }

        Label BottomRightLabel;
        public Slot() : this(Vector2.Zero)
        {
            //Opacity = 0.5f;
            //Blend = Color.White;
            //Background = UIManager.SlotSprite;
            //Alpha = Color.Lerp(Color.Transparent, Blend, 0.5f);
            Width = UIManager.SlotSprite.Width;
            Height = UIManager.SlotSprite.Height;
            //BottomRightLabel = new Label();
        }
        public Slot(Vector2 location)
            : base(location)
        {
            //Slot = slot;
            //Opacity = 0.5f;
            Blend = Color.White;
            BackgroundTexture = UIManager.SlotSprite;
            Alpha = Color.Lerp(Color.Transparent, Blend, 0.5f);
            Width = UIManager.SlotSprite.Width;
            Height = UIManager.SlotSprite.Height;
            BottomRightLabel = new Label(new Vector2(UIManager.SlotSprite.Width), "", HorizontalAlignment.Right, VerticalAlignment.Bottom);
            Controls.Add(BottomRightLabel);
            Tag = null;// T.Empty;
            Text = "";
            //State = States.Up;

            //Controls = new List<Control>();
        }

        public bool AutoText = true;


        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            this.Invalidate();
        }
        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            this.Invalidate();
        }

        public override void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity)
        {
            Vector2 loc = new Vector2(destRect.X, destRect.Y);
            Color c = Color.Lerp(Color.Transparent, color, opacity);
            sb.Draw(UIManager.SlotSprite, loc, sourceRect, c, 0, new Vector2(0), 1, SprFx, Depth);
            if (Tag.IsNull())
                return;

            Tag.GetIcon().Draw(sb, loc + new Vector2(3), sourceRect);
           // sb.Draw(TextSprite, loc + BottomRight, sourceRect, color, 0, new Vector2(TextSprite.Width, TextSprite.Height), 1, SpriteEffects.None, 0);
        }


        public Texture2D IconSheet = Map.ItemSheet;
        public List<Rectangle> IconList = Map.Icons;
        public int IconIndex = -1;

        //public override void DoDragDrop(object obj, DragDropEffects effects)
        //{
        //    //ItemData itemSlot = data as ItemData;
        //    //if (itemSlot != null)
        //    if (obj != null)
        //    {
        //        DragDropManager.Instance.Item = obj;// itemSlot;//.Item;
        //        DragDropManager.Instance.Effects = effects;
        //        DragDropManager.Instance.Source = this;// obj;// this;
        //    }
        //    //if (effects == DragDropEffects.Move)
        //    //    ItemSlot = null;
        //    //DragDrop.Instance.Sprite = ItemSlot.Item.SpriteSheet;
        //    //DragDrop.Instance.SourceRect = ItemSlot.Item.SourceRect;
        //}

        public event EventHandler<DragEventArgs> DragDrop;
        protected void OnDragDrop(DragEventArgs a)
        {
            if (DragDrop != null)
                DragDrop(this, a);
        }
        public virtual DragDropEffects Drop(DragEventArgs args)
        {
            //Console.WriteLine("DROP");
           
            OnDragDrop(args);
            return DragDropAction(args);
        }

        public override void GetTooltipInfo(Tooltip tooltip)
        {
            if (CustomTooltip)
                base.GetTooltipInfo(tooltip);
            else
            //if (Tag.Object != null)
            //    Tag.Object.GetTooltipInfo(tooltip);
                Tag.GetTooltipInfo(tooltip);
        }

        //public IEnumerable<ContextAction> GetContextActions(params object[] p)
        //{
        //    ContextArgs a = new ContextArgs() { Actions = new List<ContextAction>() };
        //    OnContextRequest(a);
        //    return a.Actions;
        //}
        public void GetContextActions(ContextArgs a)
        {
           // ContextArgs a = new ContextArgs() { Actions = new List<ContextAction>() };
            OnContextRequest(a);
           // return a.Actions;
        }
    }
}
