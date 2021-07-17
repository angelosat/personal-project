using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ButtonColor : IconButton
    {
        public Color SelectedColor = Color.White;
        Rectangle Rect = new Rectangle(UIManager.DefaultIconButtonSprite.Width / 2 - 5, UIManager.DefaultIconButtonSprite.Height / 2 - 5, 10, 10);

        public ButtonColor()
        {
            BackgroundTexture = UIManager.Icon16Background;
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            this.Rect.X = (int)(this.ScreenLocation.X + UIManager.Icon16Background.Width / 2 - 5);
            this.Rect.Y = (this.Pressed ? 1 : 0) + (int)(this.ScreenLocation.Y + UIManager.Icon16Background.Height / 2 - 5);
            var screenRect = Rectangle.Intersect(viewport, this.Rect);
            this.DrawHighlight(sb, screenRect, this.SelectedColor);
        }
    }
}
