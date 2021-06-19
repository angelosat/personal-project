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
    public class FloatingBar : InteractionBar
    {
        public event EventHandler<EventArgs> Finished;
        void OnFinished()
        {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
        }

        static public float Speed = 0.5f, Life = 300f;
        float CurrentLife;
     //   float Offset;
       // GameObject Object;
        public FloatingBar(GameObject obj, ProgressOld progress)
        {
            Object = obj;
            Percentage = progress.Percentage;
            CurrentLife = Life;
        }

        public override void Update()
        {
            //Offset += Speed * GlobalVars.DeltaTime;
            Rectangle rect = Rooms.Ingame.Instance.Camera.GetScreenBounds(Object.Transform.Position.Global, Object.GetComponent<SpriteComponent>("Sprite").Sprite.GetBounds());
            Location = new Vector2(rect.X + rect.Width / 2 - Width / 2, rect.Y);// - (int)Offset);
            CurrentLife -= 1;//GlobalVars.DeltaTime;
            if (CurrentLife < 0)
            {
                OnFinished();
                Parent.Controls.Remove(this);
            }
            base.Update();
        }

        public override void Draw(SpriteBatch sb)
        {
            if (Object == null)
                return;

            //Color alpha = Color.Lerp(Color.Transparent, Color.White, (float)Math.Sin((CurrentLife / (float)Life) * Math.PI / 2f)); //(float)Math.Pow(CurrentLife / (float)Life, 0.5f));
            //Color alpha = Color.Lerp(Color.Transparent, Color.White, (float)Math.Pow(CurrentLife / (float)Life, 0.33f));
            Color alpha = Color.Lerp(Color.Transparent, Color.White, 10 * (float)Math.Sin(Math.PI * (CurrentLife / (float)Life)));
            sb.Draw(UIManager.DefaultProgressBar, new Rectangle((int)Location.X, (int)Location.Y, Width, UIManager.DefaultProgressBar.Height), null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.FlipVertically, 1);
            sb.Draw(UIManager.DefaultProgressBar, Location, new Rectangle(0, 0, (int)(Width * Percentage), UIManager.DefaultProgressBar.Height), alpha);
            //sb.Draw(UIManager.ProgressBarBorder, new Rectangle((int)ScreenLocation.X, (int)ScreenLocation.Y, Width, UIManager.ProgressBarFill.Height), Color.White);
            if (Width == DefaultWidth)
                sb.Draw(UIManager.ProgressBarBorder, Location, alpha);
            //if (TextSprite != null)
            //    sb.Draw(TextSprite, Location + new Vector2(Width / 2, UIManager.ProgressBarBorder.Height / 2), null, Color.White, 0, new Vector2(TextSprite.Width / 2, TextSprite.Height / 2), 1, SpriteEffects.None, 0);
        }

        internal void Refresh()
        {
            CurrentLife = Life;
        }
    }
}
