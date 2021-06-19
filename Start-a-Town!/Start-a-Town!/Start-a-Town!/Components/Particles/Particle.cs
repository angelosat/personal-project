using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Components.Particles
{
    public class Particle
    {
        public Texture2D Texture = Block.ParticlePixel.Atlas.Texture;// UI.UIManager.Highlight;
        public Rectangle SourceRectangle = Block.ParticlePixel.Rectangle;//new Rectangle(0, 0, 1, 1);

        public Vector3 Offset, Velocity;
        public float Lifetime, LifetimeMax;
        public Func<Particle, Color> ColorFunc = p => Color.White;
        public Func<Particle, float> ScaleFunc = p => 2;
        public Func<Particle, float> AlphaFunc = p => p.LifePercentage;

        public float LifePercentage { get { return this.Lifetime / this.LifetimeMax; } }

        public Particle(Vector3 startOffset, Vector3 startVelocity, float life)
        {
            this.Offset = startOffset;
            this.Velocity = startVelocity;
            this.Lifetime = this.LifetimeMax = life;
        }

        public void Update()//Vector3 adjustment)
        {
            //this.Velocity += adjustment;
            //this.Offset += this.Velocity;
            this.Lifetime--;
        }


        public void Draw(Camera cam, IMap map, Vector3 global)
        {
            var transformedGlobal = this.Offset + global;
            if (transformedGlobal.Z > cam.MaxDrawZ)
                return;
            byte skylight, blocklight;
            map.GetLight(transformedGlobal.Round(), out skylight, out blocklight);
            var skyColor = map.GetAmbientColor() * ((skylight + 1) / 16f); //((skylight) / 15f);
            skyColor.A = 255;
            //var blockColor = Color.Lerp(Color.Black, Color.White, (blocklight) / 15f);
            var blockColorVector = Vector4.Lerp(new Vector4(0, 0, 0, 1), Vector4.One, (blocklight) / 15f);

            var screenpos = cam.GetScreenPositionFloat(transformedGlobal);
            //screenpos -= new Vector2(this.SourceRectangle.Width, this.SourceRectangle.Height) * 0.5f;
            var alpha = this.AlphaFunc(this);// this.LifePercentage;
            var scale = this.ScaleFunc(this);
            var color = this.ColorFunc(this);
            //var origin = Vector2.One * .5f;
            var depth = transformedGlobal.GetDrawDepth(map, cam);
            var finalscale = new Vector2(cam.Zoom) * scale;
            var finalcolor = color * alpha;
            var origin = new Vector2(this.SourceRectangle.Width, this.SourceRectangle.Height) / 2f;
            cam.ParticlesSpriteBatch.Draw(
                //UI.UIManager.Highlight, screenpos, new Rectangle(0, 0, 1, 1), 
                this.Texture, screenpos, this.SourceRectangle,
                0, origin, finalscale,
                //Color.White, Color.White, 
                skyColor, blockColorVector,// blockColor,
                finalcolor, Color.Transparent,
                Microsoft.Xna.Framework.Graphics.SpriteEffects.None, depth);
        }
    }
}
