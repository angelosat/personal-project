using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Notification : Label
    {
        public Vector2 Offset = Vector2.Zero;
        public static int Duration = 10;
        float t, tmax;
        public bool WarpText;
        public static int WidthMax = 100;


        public event EventHandler<EventArgs> DurationFinished;
        protected void OnDurationFinished()
        {
            if (DurationFinished != null)
                DurationFinished(this, EventArgs.Empty);

            Hide();// Destroy();
        }

        public Notification(string text)
        {
            Text = text;
            //BorderStyle = BorderStyle.None;
            WarpText = false;
            //t = (float)Text.Length * Notification.Duration;
            t = 60 * Notification.Duration;
            tmax = t;
            //Paint();
            Location =  - new Vector2(0, UIManager.Height / 4);
            //UIManager.InGameElements.Add(this);
        }
        public Notification(string text, bool warptext)
        {
            Text = text;
            //BorderStyle = BorderStyle.None;
            WarpText = warptext;
            t = (float)Text.Length * Notification.Duration;
            tmax = t;
            //Paint();
            //Location = CenterScreen;
            Location = -new Vector2(0, UIManager.Height / 4);
         //   ScreenManager.CurrentScreen.WindowManager.InGameElements.Add(this);
        }
        public override void Update()
        {
            base.Update();
            t -= 1;//GlobalVars.DeltaTime;
            if (t < 0)
                OnDurationFinished();
        }
        //public void Paint()
        //{
        //    TextSprite = UIManager.DrawTextOutlined(Text);
        //}

        //public void Paint()
        //{
        //    GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
        //    SpriteBatch sb = Game1.Instance.spriteBatch;
        //    Sprite = new RenderTarget2D(gfx, Width, Height);
        //    gfx.SetRenderTarget(Sprite);
        //    gfx.Clear(Color.Transparent);
        //    sb.Begin();
        //    //BackgroundStyle.Panel.Draw(sb, Width, Height);
        //    //BackgroundStyle regions = new BackgroundStyle(BorderStyle);

        //    //sb.Draw(UIManager.frameSprite, new Vector2(0), regions.TopLeft, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.frameSprite, new Vector2(Width - 11, 0), regions.TopRight, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.frameSprite, new Vector2(0, Height - 11), regions.BottomLeft, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.frameSprite, new Vector2(Width - 11, Height - 11), regions.BottomRight, regions.Color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

        //    ////top, left, right, bottom
        //    //sb.Draw(UIManager.frameSprite, new Rectangle(11, 0, Width - 22, 11), regions.Top, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.frameSprite, new Rectangle(0, 11, 11, Height - 22), regions.Left, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.frameSprite, new Rectangle(Width - 11, 11, 11, Height - 22), regions.Right, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.frameSprite, new Rectangle(11, Height - 11, Width - 22, 11), regions.Bottom, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

        //    ////center
        //    //sb.Draw(UIManager.frameSprite, new Rectangle(11, 11, Width - 22, Height - 22), regions.Center, regions.Color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        //    sb.Draw(TextSprite, new Vector2(Width / 2, Height / 2), null, Color.White, 0, new Vector2(TextSprite.Width / 2, TextSprite.Height / 2), 1, SpriteEffects.None, 1f);
        //    sb.End();
        //    gfx.SetRenderTarget(null);
        //}

        //public override void Draw(SpriteBatch sb)
        //{

        //    float alpha = Duration * (float)Math.Sin(Math.PI * (1 - t / (float)tmax));
        //    sb.Draw(TextSprite, new Vector2((UIManager.Width - TextSprite.Width) / 2, UIManager.Height / 4) + Offset, null, Color.Lerp(Color.Transparent, Color.White, alpha), 0, new Vector2(0), 1, SpriteEffects.None, 0);//Parent.DrawDepth);

        //}
    }
}
