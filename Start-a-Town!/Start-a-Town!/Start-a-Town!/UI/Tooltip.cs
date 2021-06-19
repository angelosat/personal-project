using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class TooltipEventArgs : EventArgs
    {
        public List<GroupBox> TooltipGroups;
        public object Source;
        public TooltipEventArgs(object source, List<GroupBox> tooltipGroups)
        {
            Source = source;
            TooltipGroups = tooltipGroups;
        }
    }

    public class Tooltip : Control
    {
        //public bool Interactive;
        Vector2 Offset = new Vector2(16);

        public Tooltip(ITooltippable obj)
        {
            this.Tag = obj;
            ClientLocation = new Vector2(UIManager.BorderPx);
            AutoSize = true;
            MouseThrough = true;
        }

        public override void OnMouseEnter()
        {

        }

        public override bool HitTest()
        {
            return false;
        }
        //public override void UpdateScreenLocation()
        //{
        //    if (Interactive)
        //        if (Controller.msCurrent.RightButton != Microsoft.Xna.Framework.Input.ButtonState.Pressed)
        //            base.UpdateScreenLocation();
        //}

        //public override void Paint()
        //{
        //    SpriteBatch sb = Game1.Instance.spriteBatch;
        //    GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
        //    BackgroundStyle regions = BackgroundStyle.Tooltip;

        //    RenderTarget2D tooltip = new RenderTarget2D(gfx, Width + regions.Left.Width + regions.Right.Width, Height + regions.Top.Height + regions.Bottom.Height);

        //    gfx.SetRenderTarget(tooltip);
        //    gfx.Clear(Color.Transparent);
        //    sb.Begin();
        //    UIManager.DrawBorder(sb, regions, Size);

        //    sb.End();
        //    gfx.SetRenderTarget(null);
        //    Background = tooltip;
        //}

        //public override void Update()
        //{
        //    base.Update(); 



        //    foreach (Control control in Controls)
        //        control.Update();
        //}

        public override Vector2 ScreenLocation
        {
            get
            {
                //if ((bool)Engine.Instance["MouseTooltip"])
                if (TooltipManager.MouseTooltips)
                    return new Vector2(Math.Max(Math.Min(Controller.Instance.msCurrent.X / UIManager.Scale + Offset.X, UIManager.Width - Width), 0), Math.Max(Math.Min(Controller.Instance.msCurrent.Y / UIManager.Scale + Offset.Y, UIManager.Height - Height), 0));
                else
                    return new Vector2(UIManager.Width - Width - UIManager.BorderPx, UIManager.Height - Height - UIManager.BorderPx);
            }
        }

        public override void OnPaint(SpriteBatch sb)
        {
            BackgroundStyle.Tooltip.Draw(sb, Size, Color, 1);
        }
        //public override void Paint()
        //{
        //    //base.Paint();
        //}
        public override void Draw(SpriteBatch sb)
        {

           // BackgroundStyle.Tooltip.Draw(sb, Bounds, Color, 1);

            base.Draw(sb, this.ScreenBounds);

        }

        //internal override void OnControlAdded()
        //{
        //    base.OnControlAdded(); 
        //    Paint();
        //}

        protected override void OnClientSizeChanged()
        {
            Size = new Rectangle(0, 0, ClientSize.Width + 2 * UIManager.BorderPx, ClientSize.Height + 2 * UIManager.BorderPx);
            base.OnClientSizeChanged();
        }

        //public void Add(GroupBox groupbox)
        //{
        //    Controls.Add(groupbox);
        //}
    }
}
