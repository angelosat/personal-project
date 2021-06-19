using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class BarSmooth : ButtonBase
    {
        //float _Percentage;
        public bool Invert;
        public override Texture2D BackgroundTexture
        {
            get
            {
                return UIManager.DefaultProgressBar;
            }
            set
            {
              //  base.BackgroundTexture = value;
            }
        }

        public IProgressBar Object { get; set; }

        public float Percentage
        {
            get
            { return (Object != null) ? this.Object.Percentage : 0;}// _Percentage; }
            //set
            //{
            //    _Percentage = value;
            //}
        }
        //float LastPercentage = 0;
        //string LastText = "";
        float SmoothPercentage;

        //public override void OnPaint(SpriteBatch sb)
        //{
        //    var percentage = Invert ? (1 - this.Percentage) : this.Percentage;
        //    sb.Draw(this.BackgroundTexture, Vector2.Zero, new Rectangle(0, 0, (int)(this.Width * percentage), this.Height), Color);//Color.White);
        //    var txt = (this.TextFunc != null ? this.TextFunc() : "");
        //    UIManager.DrawStringOutlined(sb, Name + txt, Dimensions * 0.5f, new Vector2(0.5f));
        //}

        public BarSmooth()
        {
            this.Height = UIManager.DefaultProgressBarStrip.Bounds.Height;
            this.Width = 100;
            this.BackgroundColor = Color.Black * 0.5f;
           // this.ColorEmpty = Color.White;
        }
        public BarSmooth(IProgressBar progress)
            : this()
        {
            this.Object = progress;
            this.SmoothPercentage = progress.Percentage;
        }
        //public override void Update()
        //{
        //    ////if ((int)(this.LastPercentage * 100) != (int)(Percentage * 100))
        //    ////    this.Invalidate();

        //    var d = this.Object.Value - this.SmoothValue;
        //    this.SmoothValue += d > 0 ? 1 : -1;// Math.Abs(d) / d;

        //    if ((int)(this.LastPercentage * this.Width) != (int)(Percentage * this.Width))
        //        this.Invalidate();
        //    // TODO: optimize (redraw less often)
        //    //if (this.Object.Percentage != this.LastPercentage)
        //    //    this.Invalidate();
        //    if(this.Text != this.LastText)
        //        this.Invalidate();
        //    this.LastText = this.Text;

        //    base.Update();
        //    this.LastPercentage = this.Percentage;
        //}

        public void Draw(SpriteBatch sb, Rectangle viewport, Vector2 screenLoc, int width, float scale)
        {
            base.Draw(sb, viewport);
            var d = this.Percentage - this.SmoothPercentage;
            //if (d != 0)
                this.SmoothPercentage += d*.05f;// > 0 ? .01f : -.01f;// Math.Abs(d) / d;
            var percentage = this.Percentage;
            //var rect = new Rectangle((int)screenLoc.X, (int)screenLoc.Y, width, UIManager.DefaultProgressBar.Height);
            var w = (int)(width * scale);
            var h = (int)(UIManager.DefaultProgressBar.Height * scale);
            var x = (int)screenLoc.X - w / 2;
            var y = (int)screenLoc.Y - h / 2;
            var rect = new Rectangle(x, y, w, h);
            rect.DrawHighlight(sb, Color.Black * .5f);
            //var smoothperc = this.SmoothPercentage / (float)this.Object.Max;
            var vec = new Vector2(x, y);
            // TODO: OPTIMIZE: create a texture for the static graphics and only draw the smooth bar dynamically
            sb.Draw(UIManager.DefaultProgressBar, vec, new Rectangle(0, 0, (int)(w * percentage), h), Color.Orange * 0.5f, 0, Vector2.Zero, Vector2.One, SpriteEffects.FlipVertically, 0);
            sb.Draw(UIManager.DefaultProgressBar, vec, new Rectangle(0, 0, (int)(w * this.SmoothPercentage), h), Color.Orange);
            if (width == InteractionBar.DefaultWidth)
                sb.Draw(UIManager.ProgressBarBorder, vec, null, Color.White, 0, Vector2.Zero, scale, SpriteEffects.None, 0f);//0.05f);
        }

        static public void Draw(SpriteBatch sb, Camera camera, GameObject parent, string text, float percentage)
        {
            //Vector3 global = parent.Global;
            //Rectangle bounds = camera.GetScreenBounds(global, parent["Sprite"].GetProperty<Sprite>("Sprite").GetBounds());
            //Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            //Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            //Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            //InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, percentage);
            //UIManager.DrawStringOutlined(sb, text, textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
            Rectangle bounds = camera.GetScreenBounds(parent.Global, parent.GetSprite().GetBounds());
            Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, percentage);
            UIManager.DrawStringOutlined(sb, text, textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        }
        static public void Draw(SpriteBatch sb, Camera camera, GameObject parent, string text, float percentage, float scale)
        {
            Rectangle bounds = camera.GetScreenBounds(parent.Global, parent.GetSprite().GetBounds());
            Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y - bounds.Height / 4);//
            Vector2 barLoc = scrLoc;
            Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, percentage, scale);
            UIManager.DrawStringOutlined(sb, text, textLoc - new Vector2(InteractionBar.DefaultWidth/2,0), HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        }
        static public void Draw(SpriteBatch sb, Camera camera, Vector3 global, string text, float percentage, float scale)
        {
            Vector2 scrLoc = camera.GetScreenPositionFloat(global);
            Vector2 barLoc = scrLoc;
            Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, percentage, scale);
            var textlocloc = textLoc - new Vector2(InteractionBar.DefaultWidth / 2, 0);
            //UIManager.DrawStringOutlined(sb, text, textlocloc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
            UIManager.DrawStringOutlined(sb, text, textlocloc, Color.Black, Color.White, scale, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        }
    }
}
