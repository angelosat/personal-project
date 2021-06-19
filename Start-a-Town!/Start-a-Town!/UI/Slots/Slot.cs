﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class Slot : ButtonBase, IDropTarget, ITooltippable, IContextable, IDebuggable
    {
        public GameObject Debug()
        {
            if (this.Tag is null)
                return null;
            if(this.Tag.HasValue)
                return this.Tag.Object;
            return null;
        }

        public Func<GameObject, bool> DragDropCondition = o => true;
        public Func<GameObjectSlot, string> CornerTextFunc = (slot) => slot.StackSize > 1 ? slot.StackSize.ToString() : "";// > 1 ? slot.StackSize.ToString() : "";
        Func<DragEventArgs, DragDropEffects> _DragDropAction = (args) => DragDropEffects.None;
        public Func<DragEventArgs, DragDropEffects> DragDropAction 
        { get { return _DragDropAction; } set { _DragDropAction = value; } }
        protected override void OnTextChanged()
        {

        }
        public Action DragDropCreateAction = () => { };

        public override void OnPaint(SpriteBatch sb)
        {
            float a = (this.MouseHover && Active) ? 1 : 0.5f;

            sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, this.Color * a, 0, new Vector2(0), 1, SprFx, Depth);
            if (!this.Tag.HasValue)
            {
                sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, this.Color * a, 0, new Vector2(0), 1, SprFx, Depth);
                return;
            }
            var color = this.Tag.Object.GetInfo().GetQualityColor();
            sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, color * a, 0, new Vector2(0), 1, SprFx, Depth);
            Rectangle rect = new Rectangle(3, 3, Width - 6, Height - 6);

            //Icon icon = this.Tag.GetIcon(); // TODO: cache the icon to optimize
            //sb.Draw(icon.SpriteSheet, rect, icon.SourceRect, icon.Tint);// Color.White);

            //var sprite = this.Tag.Object.Body.Sprite;
            //sprite.Draw(sb, new Vector2(rect.X, rect.Y), Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0); //new Vector2(rect.X, rect.Y)

            //var loc = new Vector2(rect.X, rect.Y);
            //Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            //MySpriteBatch mysb = new MySpriteBatch(sb.GraphicsDevice);
            //fx.CurrentTechnique = fx.Techniques["EntitiesUI"];//EntitiesFog"];
            ////sb.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
            //fx.CurrentTechnique.Passes["Pass1"].Apply();
            //sprite.Draw(mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
            //mysb.Flush();

            //this.Tag.Object.Body.DrawTree(this.Tag.Object, sb,new Vector2(rect.X, rect.Y), Color.White, Vector2.Zero, 0, 1, SpriteEffects.None, 0);


            //UIManager.DrawStringOutlined(sb, CornerTextFunc(Tag), new Vector2(Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        }

        public override void OnAfterPaint(SpriteBatch sb)//GraphicsDevice gd)
        {
            //if (this.Tag.Object == null)
            //    this.Tag.ID.ToConsole();
            if (!this.Tag.HasValue)
                return;
            GraphicsDevice gd = sb.GraphicsDevice;
            var sprite = this.Tag.Object.Body.Sprite;
            Rectangle rect = new Rectangle(3, 3, Width - 6, Height - 6);
            var loc = new Vector2(rect.X, rect.Y);
            Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            MySpriteBatch mysb = new MySpriteBatch(gd);
            fx.CurrentTechnique = fx.Techniques["EntitiesFog"]; //EntitiesUI"];//
            //sb.GraphicsDevice.DepthStencilState = new DepthStencilState() { DepthBufferWriteEnable = false };
            fx.Parameters["Viewport"].SetValue(new Vector2(this.Size.Width, this.Size.Height));
            gd.Textures[0] = Sprite.Atlas.Texture;
            gd.Textures[1] = Sprite.Atlas.DepthTexture;
            fx.CurrentTechnique.Passes["Pass1"].Apply();

            
            var body = this.Tag.Object.Body;
            var scale = 1;// (float)Math.Min(1, this.Width / sprite.GetSourceRect().Width);
            //body.DrawTree(this.Tag.Object, sb, loc * scale, Color.White, body.Sprite.Origin, 0, scale, SpriteEffects.None, 0.5f);
            //body.DrawTree(this.Tag.Object, sb, loc * scale, Color.White, body.Joint, 0, scale, SpriteEffects.None, 0.5f);
            //sprite.Draw(mysb, loc, Color.White, Color.White, Color.White, Color.Transparent, 0, Vector2.Zero, this.Width / sprite.GetSourceRect().Width , SpriteEffects.None, 0);

            //body.DrawTree(this.Tag.Object, mysb, loc * scale, sprite.Origin, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);



            loc += sprite.OriginGround;// new Vector2(rect.Width, rect.Height) / 2f;
            //body.DrawTree(this.Tag.Object, sb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, SpriteEffects.None, 1f, 0.5f);
            body.DrawGhost(this.Tag.Object, mysb, loc * scale, Color.White, Color.White, Color.White, Color.Transparent, 0, scale, 0, SpriteEffects.None, 1f, 0.5f);


            mysb.Flush();

            UIManager.DrawStringOutlined(sb, CornerTextFunc(Tag), new Vector2(Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        }

        public override void OnAfterDraw(SpriteBatch sb, Rectangle viewport)
        {
            //if (!this.Tag.HasValue)
            //    return;

            //// TODO: figure out betterway to restrict the icon size
            //Rectangle final, source;
            //Rectangle rect = new Rectangle((int)ScreenLocation.X + 3, (int)ScreenLocation.Y + 3, Width - 6, Height - 6);
            
            //Icon icon = Tag.GetIcon(); // TODO: cache the icon to optimize
            //rect.Clip(icon.SourceRect, viewport, out final, out source);

            //sb.Draw(icon.SpriteSheet, final, source, Color.White);

            //UIManager.DrawStringOutlined(sb, CornerTextFunc(Tag), ScreenLocation + new Vector2(Slot.DefaultHeight), Vector2.One, UIManager.FontBold);
        }

        public event EventHandler<EventArgs> ItemChanged;
        void OnItemChanged()
        {
            if (ItemChanged != null)
                ItemChanged(this, EventArgs.Empty);
        }
        GameObject LastObject = null;
        int LastStackSize = 0;
        public override void Update()
        {
            base.Update();
            if (this.Tag is null)
                return;
            if (this.Tag.Object != this.LastObject)
                this.Invalidate();
            else if (this.Tag.StackSize != this.LastStackSize)
                this.Invalidate();
            this.LastObject = this.Tag.Object;
            this.LastStackSize = this.Tag.StackSize;
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

        public virtual Slot SetTag(GameObjectSlot tag)
        {
            this.Tag = tag;
            return this;
        }
        GameObjectSlot _Tag;
        public new GameObjectSlot Tag
        {
            get { return this._Tag; }
            set
            {
                this._Tag = value;
                //TintFunc = () => value.IsNull() ? Color.White : (value.HasValue ? value.Object.GetInfo().GetQualityColor() : Color.White);
                //this.Color = value.HasValue ? value.Object.GetInfo().GetQualityColor() : Color.White;
            }
        }

        public virtual void Clear()
        {
            Tag = null;
            IconIndex = -1;
        }

        public Slot SetBottomRightText(string text)
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
            Tag = GameObjectSlot.Empty;
            Text = "";
            //State = States.Up;

            //Controls = new List<Control>();
        }
        public Slot(GameObjectSlot objSlot)
            : this()
        {
            this.Tag = objSlot;
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
            if (!Tag.HasValue)
                return;

            Tag.Object["Gui"].GetProperty<Icon>("Icon").Draw(sb, loc + new Vector2(3), sourceRect);
           // sb.Draw(TextSprite, loc + BottomRight, sourceRect, color, 0, new Vector2(TextSprite.Width, TextSprite.Height), 1, SpriteEffects.None, 0);
        }


        static public void Draw(SpriteBatch sb, GameObjectSlot slot, Vector2 loc, SpriteEffects sprFx, float opacity, string text = "")
        {
            Color color = slot.Object.GetInfo().GetQualityColor();

            sb.Draw(UIManager.SlotSprite, loc, null, color * opacity, 0, new Vector2(0), 1, sprFx, 0);

            if (slot.Object == null)
                return;

            slot.Object["Gui"].GetProperty<Icon>("Icon").Draw(sb, loc + new Vector2(3));

            if (text != "")
                UIManager.DrawStringOutlined(sb, text, loc + new Vector2(Slot.DefaultHeight), Vector2.One);
            else
                if (slot.StackSize > 1)
                    UIManager.DrawStringOutlined(sb, slot.StackSize.ToString(), loc + new Vector2(Slot.DefaultHeight), Vector2.One);
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
        protected virtual void OnDragDrop(DragEventArgs a)
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

        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            base.OnMouseLeftPress(e);
            if (this.Pressed)
                DragDropManager.Start(this, this.DragDropCreateAction);
        }
    }
}
