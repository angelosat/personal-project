using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_
{
    public static class UIExtensions
    {
        public static void DrawHighlight(this Rectangle bounds, SpriteBatch sb, Vector2 origin, float rotation, float alpha = 0.5f)
        {
            bounds.DrawHighlight(sb, Color.Lerp(Color.Transparent, Color.White, alpha), origin, rotation);
        }
        public static void DrawHighlight(this Rectangle bounds, SpriteBatch sb, Color color, Vector2 origin, float rotation)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, color, rotation, origin, SpriteEffects.None, 0);
        }
        public static void DrawHighlight(this Vector4 bounds, SpriteBatch sb, Color color, Vector2 origin, float rotation)
        {
            sb.Draw(UI.UIManager.Highlight, new Vector2(bounds.X, bounds.Y), null, color, rotation, origin, new Vector2(bounds.Z, bounds.W), SpriteEffects.None, 0);
        }
        public static void DrawHighlight(this Rectangle bounds, SpriteBatch sb, float alpha = 0.5f)
        {
            bounds.DrawHighlight(sb, Vector2.Zero, 0, alpha);
        }
        public static void DrawHighlight(this Rectangle bounds, SpriteBatch sb, Color color)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, color, 0, Vector2.Zero, SpriteEffects.None, 0);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, SpriteBatch sb, float alpha = .5f, float thickness = 1, int padding = 0)
        {
            bounds.DrawHighlightBorder(sb, Color.White * alpha, Vector2.Zero, thickness, padding);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, SpriteBatch sb, Color color, float thickness = 1)
        {
            bounds.DrawHighlightBorder(sb, color, Vector2.Zero, thickness);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, SpriteBatch sb)
        {
            bounds.DrawHighlightBorder(sb, Color.White, Vector2.Zero);
        }
        public static void DrawHighlightBorder(this Rectangle bounds, SpriteBatch sb, Color color, Vector2 origin, float thickness = 1, int padding = 0)
        {
            var intthickness = (int)Math.Max(1, thickness);
            var padpad = 2 * padding;
            // Draw top line
            sb.Draw(UI.UIManager.Highlight, new Rectangle(bounds.X - padding + intthickness, bounds.Y - padding, bounds.Width + padpad - intthickness, intthickness), color);

            // Draw left line
            sb.Draw(UI.UIManager.Highlight, new Rectangle(bounds.X - padding, bounds.Y - padding, intthickness, bounds.Height + padpad - intthickness), color);

            // Draw bottom line
            sb.Draw(UI.UIManager.Highlight, new Rectangle(bounds.X - padding,
                                            bounds.Y + padding + bounds.Height - intthickness,
                                            bounds.Width - intthickness + padpad,
                                            intthickness), color);

            // Draw right line
            sb.Draw(UI.UIManager.Highlight, new Rectangle((bounds.X + padding + bounds.Width - intthickness),
                                            bounds.Y - padding + intthickness,
                                            intthickness,
                                            bounds.Height - intthickness + padpad), color);
        }


        public static UI.Label ToLabel(this string text) { return new UI.Label(Vector2.Zero, text); }
        public static UI.Label ToLabel(this string text, Vector2 location) { return new UI.Label(location, text); }
        public static UI.Label ToLabel(this string text, Vector2 location, int width) { return new UI.Label(location, text) { Width = width }; }

    }
}