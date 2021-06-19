using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Start_a_Town_.Components;
using Start_a_Town_.Rooms;

namespace Start_a_Town_.UI
{
    public class InteractionBar : Label//, ITooltippable
    {
        GameObject _Object;
        public GameObject Object
        {
            get { return _Object; }
            set
            {
                _Object = value;
                //SpriteComponent = _Object.GetComponent<SpriteComponent>("Sprite");
                PositionComponent = _Object.Transform; 
            }
        }
        string _DisplayName;
        public string DisplayName
        {
            get { return _DisplayName; }
            set
            {
                _DisplayName = value;
                if (value != null)
                    if (value.Length > 0)
                        Text = (string)Object.GetType().GetProperty(value).GetValue(Object, null);
            }
        }
        string _DisplayPercentage;
        public string DisplayPercentage
        {
            get
            {
                return _DisplayPercentage;
            }
            set
            {
                _DisplayPercentage = value;
                if (value != null)
                    if (value.Length > 0)
                        Percentage = (float)Object.GetType().GetProperty(value).GetValue(Object, null);
            }
        }
        protected double _t = 0;
        public double Percentage
        {
            get
            {
                //if(Object!=null)
                //    return (float)Object.GetType().GetProperty(DisplayPercentage).GetValue(Object, null);
                return _t;
            }
            set
            {
                _t = Math.Min(Math.Max(value, 0), 1);
            }
        }
        //Texture2D TextSprite;
        //protected string _Text;
        //public string Text
        //{
        //    get { return _Text; }
        //    set
        //    {
        //        if (value != _Text)
        //            if (value != "")
        //            {
        //                TextSprite = UIManager.DrawTextOutlined(value);
        //                _Text = value;
        //            }
        //    }
        //}

        public new static int DefaultHeight = UIManager.DefaultProgressBar.Height;
        public static int DefaultWidth = UIManager.DefaultProgressBar.Width;

        PositionComponent PositionComponent;
        //SpriteComponent SpriteComponent;
        public override void Draw(SpriteBatch sb)
        {
            Vector2 screenLoc;
            Rectangle bounds;
            if (PositionComponent != null)
            {
                Vector3 global = PositionComponent.Global;
                bounds = (ScreenManager.CurrentScreen as Ingame).Camera.GetScreenBounds((int)global.X, (int)global.Y, (int)global.Z, new Rectangle(0, 0, 1, 1));
                screenLoc = new Vector2(bounds.X + bounds.Width/2 - Width / 2, bounds.Top - Height);

            }
            else
                screenLoc = ScreenLocation;
            
            sb.Draw(UIManager.DefaultProgressBar, new Rectangle((int)screenLoc.X, (int)screenLoc.Y, Width, UIManager.DefaultProgressBar.Height), null, Color.Lerp(Color.Orange, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.FlipVertically, 1);
            sb.Draw(UIManager.DefaultProgressBar, screenLoc, new Rectangle(0, 0, (int)(Width * Percentage), UIManager.DefaultProgressBar.Height), Color.Orange);
            if (Width == DefaultWidth)
                sb.Draw(UIManager.ProgressBarBorder, screenLoc, Color.White);
            //if (TextSprite != null)
            //    sb.Draw(TextSprite, screenLoc + new Vector2(Width / 2, UIManager.ProgressBarBorder.Height / 2), null, Color.White, 0, new Vector2(TextSprite.Width / 2, TextSprite.Height / 2), 1, SpriteEffects.None, 0);

        }

        static public void Draw(SpriteBatch sb, Vector2 screenLoc, int width, float percentage)
        { 
            var rect = new Rectangle((int)screenLoc.X, (int)screenLoc.Y, width, UIManager.DefaultProgressBar.Height);
            sb.Draw(UIManager.DefaultProgressBar, rect, null, Color.Lerp(Color.Orange, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.FlipVertically, 1);
            sb.Draw(UIManager.DefaultProgressBar, screenLoc, new Rectangle(0, 0, (int)(width * percentage), UIManager.DefaultProgressBar.Height), Color.Orange);
            if (width == DefaultWidth)
                sb.Draw(UIManager.ProgressBarBorder, screenLoc, Color.White);
        }
        static public void Draw(SpriteBatch sb, Vector2 screenLoc, int width, float percentage, float scale)
        {
            //sb.Draw(UIManager.DefaultProgressBar, new Rectangle(200, 200, 64, 64), null, Color.Lerp(Color.Orange, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.FlipVertically, 1);
            var w = (int)(width * scale);
            var h = (int)(UIManager.DefaultProgressBar.Height * scale);
            var x = (int)screenLoc.X - w/2;
            var y = (int)screenLoc.Y - h/2;
            var rect = new Rectangle(x, y, w, h);
            sb.Draw(UIManager.DefaultProgressBar, rect, null, Color.Lerp(Color.Orange, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.FlipVertically, 1);//0.1f);
            sb.Draw(UIManager.DefaultProgressBar, new Vector2(x, y), new Rectangle(0, 0, (int)(w * percentage), h), Color.Orange);
            if (width == DefaultWidth)
                //sb.Draw(UIManager.ProgressBarBorder, new Vector2(x, y), Color.White);
                sb.Draw(UIManager.ProgressBarBorder, new Vector2(x, y), null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0f);//0.05f);
        }
        public InteractionBar()
        {
            Size = new Rectangle(0, 0, UIManager.ProgressBarBorder.Width, UIManager.ProgressBarBorder.Height);
            Width = UIManager.ProgressBarBorder.Width;
        }
        public InteractionBar(Vector2 location)
            : base(location)
        { 
            Size = new Rectangle(0, 0, UIManager.ProgressBarBorder.Width, UIManager.ProgressBarBorder.Height);
            Width = DefaultWidth;
        }

        public ProgressOld Meter;
        public void Track(ProgressOld meter)
        {
            Meter = meter;
            Percentage = meter.Percentage;
            Meter.ValueChanged += new EventHandler<EventArgs>(Meter_ValueChanged);
        }

        void Meter_ValueChanged(object sender, EventArgs e)
        {
            Percentage = Meter.Percentage;
        }

        public void Stop()
        {
            Meter.ValueChanged -= Meter_ValueChanged;
            Meter = null;
        }

        public override void Dispose()
        {
            if (Meter != null)
                Meter.ValueChanged -= Meter_ValueChanged;
            base.Dispose();
        }

        //public override Texture2D Tooltip
        //{
        //    get
        //    {
        //        string text = ((int)(Percentage * 100)).ToString() + "%";
        //        Texture2D textsprite = UIManager.DrawTextOutlined(UIManager.WrapText(text, TooltipManager.Width));
        //        SpriteBatch sb = Game1.Instance.spriteBatch;
        //        GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
        //        BorderStyleRegions regions = BorderStyleRegions.Tooltip;
        //        //RenderTarget2D tooltip = new RenderTarget2D(gfx, textsprite.Width, textsprite.Height);
        //        RenderTarget2D tooltip = new RenderTarget2D(gfx, textsprite.Width + regions.Left.Width + regions.Right.Width, textsprite.Height + regions.Top.Height + regions.Bottom.Height);

        //        gfx.SetRenderTarget(tooltip);
        //        gfx.Clear(Color.Transparent);
        //        sb.Begin();

        //        UIManager.DrawBorder(sb, regions, tooltip.Width, tooltip.Height);
        //        sb.Draw(textsprite, new Vector2(tooltip.Width / 2, tooltip.Height / 2), null, Color.White, 0, new Vector2(textsprite.Width / 2, textsprite.Height / 2), 1, SpriteEffects.None, 1f);

        //        sb.End();
        //        gfx.SetRenderTarget(null);
        //        return tooltip;
        //    }
        //}
    }
}
