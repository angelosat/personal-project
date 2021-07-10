﻿using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class SlotCustom<T> : ButtonBase, IDropTarget, ITooltippable, IContextable where T : class, ITooltippable
    {
        public Func<GameObject, bool> DragDropCondition = o => true;
        public Func<T, string> CornerTextFunc = (slot) => "";
        public Action<SpriteBatch, T> PaintAction = (s,t) => { };
        Func<DragEventArgs, DragDropEffects> _DragDropAction = (args) => DragDropEffects.None;
        public Func<DragEventArgs, DragDropEffects> DragDropAction 
        { get { return _DragDropAction; } set { _DragDropAction = value; } }
        protected override void OnTextChanged()
        {

        }

        public override void OnPaint(SpriteBatch sb)
        {
            float a = (this.MouseHover && Active) ? 1 : 0.5f;

            sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, this.Color * a, 0, new Vector2(0), 1, SprFx, Depth);
            if (this.Tag == null)
            {
                sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, this.Color * a, 0, new Vector2(0), 1, SprFx, Depth);
                return;
            }
            var color = Color.White;
            sb.Draw(UIManager.SlotSprite, Vector2.Zero, null, color * a, 0, new Vector2(0), 1, SprFx, Depth);
        }

        public override void OnAfterPaint(SpriteBatch sb)
        {
            if (this.Tag is null)
                return;
            this.PaintAction(sb, this.Tag);
            UIManager.DrawStringOutlined(sb, CornerTextFunc(Tag), new Vector2(SlotCustom<T>.DefaultHeight), Vector2.One, UIManager.FontBold);
        }

        public event EventHandler<EventArgs> ItemChanged;
        T LastObject = default;
        public override void Update()
        {
            base.Update();
            if (this.Tag == null)
                return;
            if (!this.Tag.Equals(this.LastObject))
                this.Invalidate();

            this.LastObject = this.Tag;
        }
        Action<ContextArgs> _ContextAction = (args) => { };
        public Action<ContextArgs> ContextAction
        {
            get { return _ContextAction; }
            set { _ContextAction = value; }
        }
        
        public static int DefaultHeight = 38;

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        T _Tag;
        public new T Tag
        {
            get { return this._Tag; }
            set
            {
                this._Tag = value;
            }
        }
        public void Clear()
        {
            Tag = default;
            IconIndex = -1;
        }

        public SlotCustom<T> SetBottomRightText(string text)
        {
            BottomRightLabel.Text = text;
            return this;
        }

        Label BottomRightLabel;
        
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

        public int IconIndex = -1;

        public event EventHandler<DragEventArgs> DragDrop;
        protected void OnDragDrop(DragEventArgs a)
        {
            DragDrop?.Invoke(this, a);
        }
        public virtual DragDropEffects Drop(DragEventArgs args)
        {
            OnDragDrop(args);
            return DragDropAction(args);
        }

        public override void GetTooltipInfo(Tooltip tooltip)
        {
            if (CustomTooltip)
                base.GetTooltipInfo(tooltip);
            else
                Tag.GetTooltipInfo(tooltip);
        }

        public void GetContextActions(GameObject playerEntity, ContextArgs a)
        {
        }
    }
}
