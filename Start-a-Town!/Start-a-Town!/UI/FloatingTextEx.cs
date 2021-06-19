using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class FloatingTextEx : GroupBox
    {
        public class Segment
        {
            public string Text;
            public Color Color;
            public Segment(string text, Color color)
            {
                this.Text = text;
                this.Color = color;
            }
        }


        static Random Random = new Random();
        const float Gravity = 0.05f;// 0.1f;

        public event EventHandler<EventArgs> Finished;
        void OnFinished()
        {
            if (Finished != null)
                Finished(this, EventArgs.Empty);
        }

        static public float Speed = 0.5f, Life = 3 * Engine.TicksPerSecond;// 300f;
        float ScaleTime;
        float ScaleLength;
        float CurrentLife;
        float Offset;
        //GameObject Object;
        Func<Vector3> GetGlobal;
        Vector2 Velocity;
        Vector2 Position;
        //List<Label> Segments = new List<Label>();
        public FloatingTextEx(GameObject parent, params Segment[] segments)
        {
            Layer = LayerTypes.Speechbubbles;
            foreach (var s in segments)
            {
                var label = new Label(s.Text) { Location = this.Controls.TopRight, Font = UIManager.FontBold, TextColorFunc = () => s.Color };
                //this.Segments.Add(label);
                this.Controls.Add(label); 
            }
            CurrentLife = Life;
            ScaleTime = ScaleLength = 20;// Life / 10f;
            //Object = obj;
            this.GetGlobal = () => parent.Global;
            float angle = (float)((Math.PI / 3f) * (1 + Random.NextDouble()));
            this.Velocity = new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
            this.Velocity *= 3;
            this.Position = Vector2.Zero;
        }
        public FloatingTextEx(Func<Vector3> global, params Segment[] segments)
        {
            Layer = LayerTypes.Speechbubbles;
            foreach (var s in segments)
            {
                var label = new Label(s.Text) { Location = this.Controls.TopRight, Font = UIManager.FontBold, TextColorFunc = () => s.Color };
                this.Controls.Add(label);
            }
            CurrentLife = Life;
            ScaleTime = ScaleLength = 20;// Life / 10f;
            //Object = obj;
            this.GetGlobal = global;
            float angle = (float)((Math.PI / 3f) * (1 + Random.NextDouble()));
            this.Velocity = new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
            this.Velocity *= 3;
            this.Position = Vector2.Zero;
        }
        //public FloatingText(GameObject obj, Vector3 global, string text)
        //    : base(Vector2.Zero, text)
        //{
        //    Layer = LayerTypes.Speechbubbles;
        //    Text = text;
        //    CurrentLife = Life;
        //    ScaleTime = ScaleLength = 20;// Life / 10f;
        //    Object = obj;
        //    float angle = (float)((Math.PI / 3f) * (1 + Random.NextDouble()));
        //    this.Velocity = new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
        //    this.Velocity *= 3;
        //    this.Position = Vector2.Zero;
        //}

        public override void Update()
        {
            Offset += Speed;
            Camera cam = ScreenManager.CurrentScreen.Camera;

            //var sprite = Object.GetSprite();
            //var bounds = sprite.GetBounds();
            //var global = this.GetGlobal();
            //Rectangle rect = cam.GetScreenBounds(global, bounds);

            var global = this.GetGlobal();
            Vector2 origin = cam.GetScreenPosition(global);
            Location = origin + Position;// - (int)Offset); + rect.Width / 2 - Width / 2

            //Location = cam.GetScreenBounds(Object.Global) + Position;
            Position += Velocity;
            Velocity.Y += Gravity;
            Opacity = (float)Math.Sin(Math.PI * (CurrentLife / (float)Life) / 2f); // * 10
            CurrentLife -= 1;
            if(ScaleTime>0)
                ScaleTime--;
            if (CurrentLife < 0)
            {
                OnFinished();
                Hide();
            }
            base.Update();
        }
        /// <summary>
        /// override draw call to omit hittesting
        /// </summary>
        /// <param name="sb"></param>
        //public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        //{
        //    Color alpha = Color.Lerp(Color.Transparent, Color.White, 10 * (float)Math.Sin(Math.PI * (CurrentLife / (float)Life)));
        // //   sb.Draw(TextSprite, Location / UIManager.Scale, null, alpha, 0, Origin, 1, SpriteEffects.None, Depth);
        //    foreach (Control control in Controls)
        //        control.Draw(sb);
        //}
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            //float scaleFactor = Math.Max(0, (ScaleTime / ScaleLength) * 5);
            float p = (ScaleTime / ScaleLength);
            float scaleFactor = (float)Math.Sin(p * Math.PI) * 5;// Math.Max(0, *5);
            float scale = 1 + scaleFactor;
            var color = this.Color * Opacity;
            //var origin = this.Texture.Bounds.Center.ToVector();
            var center = this.Size.Center.ToVector();

            //sb.Draw(this.Texture, this.Location, null, color, Rotation, origin, scale, SpriteEffects.None, 0);
            foreach (var s in this.Controls)
            {
                //var origin = s.Texture.Bounds.Center.ToVector();
                var origin = center - s.Location;
                sb.Draw(s.Texture, this.Location + s.Location + origin, null, color, Rotation, origin, scale, SpriteEffects.None, 0);
            }
        }

    }
}
