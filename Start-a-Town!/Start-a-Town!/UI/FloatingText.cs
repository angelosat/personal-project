using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class FloatingText : Label
    {
        public class Manager
        {
            //static public FloatingText Create(GameObject entity, Vector3 global, string text, Action<FloatingText> initializer)
            //{
            //    var ft = new FloatingText(entity, global, text);
            [Obsolete]
            static public FloatingText Create(GameObject entity, string text, Action<FloatingText> initializer = null)
            {
                return Create(() => entity.Global + entity.Physics.Height * Vector3.UnitZ, text, initializer);
                //var ft = new FloatingText(entity, text);
                //initializer(ft);
                //ft.Show();
                //return ft;
            }
            [Obsolete]
            static public FloatingText Create(Func<Vector3> global, string text, Action<FloatingText> initializer = null)
            {
                //var ft = new FloatingText(entity, text);
                var ft = new FloatingText(global, text);
                initializer?.Invoke(ft);
                ft.Font = UIManager.FontBold;
                ft.Show();
                return ft;
            }
        }
        static public void Create(GameObject entity, string text, Action<FloatingText> initializer = null)
        {
            Create(entity.Map, () => entity.Global + entity.Physics.Height * Vector3.UnitZ, text, initializer);
            //if (Rooms.Ingame.Net != entity.Net)
            //    return;
            //var ft = new FloatingText(()=>entity.Global, text);
            //initializer(ft);
            //ft.Show();
        }
        static public void Create(IMap map, Func<Vector3> global, string text, Action<FloatingText> initializer = null)
        {
            if (Rooms.Ingame.Net != map.Net)
                return;
            var ft = new FloatingText(global, text);
            ft.Font = UIManager.FontBold;
            initializer?.Invoke(ft);
            ft.Show();
        }
        static public void Create(IMap map, Vector3 global, string text, Action<FloatingText> initializer) => Create(map, () => global, text, initializer);

        static readonly Random Random = new Random();
        const float Gravity = 0.05f;// 0.1f;

        public event EventHandler<EventArgs> Finished;
        void OnFinished()
        {
            Finished?.Invoke(this, EventArgs.Empty);
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

        public FloatingText(GameObject obj, string text)
            : base(Vector2.Zero, text)
        {
            Layer = LayerTypes.Speechbubbles;
            Text = text;
            CurrentLife = Life;
            ScaleTime = ScaleLength = 20;// Life / 10f;
            //Object = obj;
            this.GetGlobal = () => obj.Global;
            float angle = (float)((Math.PI / 3f) * (1 + Random.NextDouble()));
            this.Velocity = new Vector2((float)Math.Cos(angle), -(float)Math.Sin(angle));
            this.Velocity *= 3;
            this.Position = Vector2.Zero;
        }
        public FloatingText(Func<Vector3> global, string text)
            : base(Vector2.Zero, text)
        {
            Layer = LayerTypes.Speechbubbles;
            Text = text;
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
            sb.Draw(this.Texture, this.Location, null, this.Color * Opacity, Rotation, this.Texture.Bounds.Center.ToVector(), scale, SpriteEffects.None, 0);
            //foreach (var c in this.Controls)
            //    c.Draw(sb, viewport);
        }

        //static public FloatingText Create(GameObject source, Vector3 global, string text)
        //{
        //    FloatingText fl = new FloatingText(source, global, text);
        static public FloatingText Create(GameObject source, string text)
        {
            FloatingText fl = new FloatingText(source, text);
            fl.Show();
            return fl;
        }
    }
}
