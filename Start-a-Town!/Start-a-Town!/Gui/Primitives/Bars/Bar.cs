using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Bar : ButtonBase
    {
        public bool Invert;
        public override Texture2D BackgroundTexture
        {
            get
            {
                return UIManager.DefaultProgressBar;
            }
            set
            {
            }
        }

        public IProgressBar Object { get; set; }

        public float Percentage => (Object != null) ? this.Object.Percentage : 0;
        
        float LastPercentage = 0;
        string LastText = "";

        public override void OnPaint(SpriteBatch sb)
        {
            var percentage = Invert ? (1 - this.Percentage) : this.Percentage;
            var fill = (int)Math.Round(this.Width * percentage);
            sb.Draw(this.BackgroundTexture, Vector2.Zero, new Rectangle(0, 0, fill, this.Height), Color);//Color.White);
            var txt = (this.TextFunc != null ? this.TextFunc() : "");
            UIManager.DrawStringOutlined(sb, Name + txt, Dimensions * 0.5f, new Vector2(0.5f));
        }

        public Bar()
        {
            this.Height = UIManager.DefaultProgressBarStrip.Bounds.Height;
            this.Width = 100;
            this.BackgroundColor = Color.Black * 0.5f;
        }
        public Bar(IProgressBar progress)
            : this()
        {
            this.Object = progress;
        }
        public override void Update()
        {
            if (Math.Round(this.LastPercentage * this.Width) != Math.Round(Percentage * this.Width))
                this.Invalidate();
            
            var nextText = this.TextFunc?.Invoke() ?? this.Text;
            if(nextText != this.LastText)
            {
                this.Text = nextText;
                this.Invalidate();
            }
            this.LastText = this.Text;
            base.Update();
            this.LastPercentage = this.Percentage;
        }

        static public void Draw(SpriteBatch sb, Camera camera, GameObject parent, string text, float percentage)
        {
            Rectangle bounds = camera.GetScreenBounds(parent.Global, parent.GetSprite().GetBounds());
            Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, percentage);
            UIManager.DrawStringOutlined(sb, text, textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        }
        static public void Draw(SpriteBatch sb, Camera camera, Vector3 global, string text, float percentage, float scale)
        {
            Vector2 scrLoc = camera.GetScreenPositionFloat(global);
            Vector2 barLoc = scrLoc;
            Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, percentage, scale);
            var textlocloc = textLoc - new Vector2(InteractionBar.DefaultWidth / 2, 0);
            UIManager.DrawStringOutlined(sb, text, textlocloc, Color.Black, Color.White, scale, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        }
    }
}
