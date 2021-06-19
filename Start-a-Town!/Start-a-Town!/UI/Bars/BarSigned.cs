using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class BarSigned : ButtonBase
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

        public override void OnPaint(SpriteBatch sb)
        {
            var percentage = Invert ? (1 - this.Percentage) : this.Percentage;
            //var middle = this.Width * (-this.Object.Min) / (this.Object.Max - this.Object.Min);
            var middle = this.Width * .5f;// (-this.Object.Min) / (this.Object.Max - this.Object.Min);



            //var x = this.Object.Value > 0 ? middle : middle * (1 - percentage);
            //var color = this.Object.Value > 0 ? Color.Lime : Color.Red;
            var x = this.Object.Percentage > 0 ? middle : middle * (1 + percentage);
            var color = this.Object.Percentage > 0 ? Color.Lime : Color.Red;
            var w = (int)(this.Width * Math.Abs(percentage) / 2f);
            sb.Draw(this.BackgroundTexture, new Vector2(x, 0), new Rectangle(0, 0, w, this.Height), color);//Color.White);
            var txt = (this.TextFunc != null ? this.TextFunc() : "");
            UIManager.DrawStringOutlined(sb, Name + txt, Dimensions * 0.5f, new Vector2(0.5f));


            //var percentage = Invert ? (1 - this.Percentage) : this.Percentage;
            //var middle = this.Width * (-this.Object.Min) / (this.Object.Max - this.Object.Min);
            ////var x = this.Object.Value > 0 ? middle : middle * (1 - percentage);
            ////var color = this.Object.Value > 0 ? Color.Lime : Color.Red;
            //var x = this.Object.Percentage > 0 ? middle : middle * (1 + percentage);
            //var color = this.Object.Percentage > 0 ? Color.Lime : Color.Red;
            //var w = (int)(this.Width * percentage / 2f);
            //sb.Draw(this.BackgroundTexture, new Vector2(x, 0), new Rectangle(0, 0, w, this.Height), color);//Color.White);
            //var txt = (this.TextFunc != null ? this.TextFunc() : "");
            //UIManager.DrawStringOutlined(sb, Name + txt, Dimensions * 0.5f, new Vector2(0.5f));
        }

        public BarSigned()
        {
            this.Height = UIManager.DefaultProgressBarStrip.Bounds.Height;
            this.Width = 200;// 100;
            this.BackgroundColor = Color.Black * 0.5f;
           // this.ColorEmpty = Color.White;
        }
        public BarSigned(IProgressBar progress)
            : this()
        {
            this.Object = progress;
        }
        public override void Update()
        {
            ////if ((int)(this.LastPercentage * 100) != (int)(Percentage * 100))
            ////    this.Invalidate();

            if ((int)(this.LastPercentage * this.Width) != (int)(Percentage * this.Width))
                this.Invalidate();

            // TODO: optimize (redraw less often)
            //if (this.Object.Percentage != this.LastPercentage)
            //    this.Invalidate();
            if(this.Text != this.LastText)
                this.Invalidate();
            this.LastText = this.Text;

            base.Update();
            this.LastPercentage = this.Percentage;
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
