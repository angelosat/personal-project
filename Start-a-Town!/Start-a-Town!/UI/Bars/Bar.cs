using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Bar : ButtonBase
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
        float LastPercentage = 0;
        string LastText = "";
        //public Func<float> PercFunc = () => 1;

        public override void OnPaint(SpriteBatch sb)
        {
            //sb.Draw(this.BackgroundTexture, Vector2.Zero, new Rectangle(0, 0, (int)(this.Width * Percentage), this.Height), Color.Lerp(ColorEmpty, ColorFull, Percentage));
            var percentage = Invert ? (1 - this.Percentage) : this.Percentage;
            var fill = (int)Math.Round(this.Width * percentage);
            sb.Draw(this.BackgroundTexture, Vector2.Zero, new Rectangle(0, 0, fill, this.Height), Color);//Color.White);
            //UIManager.DrawStringOutlined(sb, Name, Dimensions * 0.5f, new Vector2(0.5f));
            var txt = (this.TextFunc != null ? this.TextFunc() : "");
            //string.Format("{0} {1}", txt, fill).ToConsole();
            UIManager.DrawStringOutlined(sb, Name + txt, Dimensions * 0.5f, new Vector2(0.5f));
        }

        //public override void Draw(SpriteBatch sb, Rectangle viewport)
        //{
        //    base.Draw(sb, viewport);
        //    UIManager.DrawStringOutlined(sb, this.Text, this.ScreenLocation + Dimensions * 0.5f, new Vector2(0.5f));//new Vector2(Dimensions.X, 0), new Vector2(1,0));
        //}

        public Bar()
        {
            this.Height = UIManager.DefaultProgressBarStrip.Bounds.Height;
            this.Width = 100;
            this.BackgroundColor = Color.Black * 0.5f;
           // this.ColorEmpty = Color.White;
        }
        public Bar(IProgressBar progress)
            : this()
        {
            this.Object = progress;
        }
        public override void Update()
        {
            ////if ((int)(this.LastPercentage * 100) != (int)(Percentage * 100))
            ////    this.Invalidate();

            if (Math.Round(this.LastPercentage * this.Width) != Math.Round(Percentage * this.Width))
                this.Invalidate();
            //if ((int)(this.LastPercentage * this.Width) != (int)(Percentage * this.Width))
            //    this.Invalidate();
            //if (this.LastPercentage * this.Width != Percentage * this.Width)
            //    this.Invalidate();

            // TODO: optimize (redraw less often)
            //if (this.Object.Percentage != this.LastPercentage)
            //    this.Invalidate();

            var nextText = this.TextFunc?.Invoke() ?? this.Text;
            if(nextText != this.LastText)
            {
                this.Text = nextText;
                this.Invalidate();
            }

            //if (this.Text != this.LastText)
            //    this.Invalidate();
            this.LastText = this.Text;

            base.Update();
            this.LastPercentage = this.Percentage;
        }
        //public override void Update()
        //{
        //    // TODO: slow 
        //    float oldPerc = Percentage;
        //    //Percentage = MathHelper.Clamp(PercFunc(this.Tag), 0, 1);
        //    Percentage = MathHelper.Clamp(PercFunc(), 0, 1);
        //    if ((int)(oldPerc * 100) != (int)(Percentage * 100))
        //        this.Invalidate();
        //    base.Update();
        //}

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
