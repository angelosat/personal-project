using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    class Blur
    {
        static int Passes = 2;
        public static Texture2D BlurTexture(Texture2D texture)
        {
            var gfx = Game1.Instance.GraphicsDevice;

            Effect blur = Game1.Instance.Content.Load<Effect>("blur");
            float dim = texture.Width + 2 * Passes;
            blur.Parameters["Viewport"].SetValue(new Vector2(texture.Width + 2 * Passes, texture.Height + 2 * Passes));
            blur.CurrentTechnique = blur.Techniques["Technique1"];
            blur.CurrentTechnique.Passes["Pass1"].Apply();
            SpriteBatch sb = new SpriteBatch(gfx);

            //RenderTarget2D blurredTexture = new RenderTarget2D(gfx, texture.Width + 2 * Passes, texture.Height + 2 * Passes);
            //gfx.SetRenderTarget(blurredTexture);
            //gfx.Clear(Color.Transparent);
            //sb.Begin(0, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null, blur);
            //sb.Draw(texture, Vector2.Zero, Color.White);
            //sb.End();

            //RenderTarget2D final = new RenderTarget2D(gfx, texture.Width + 2 * Passes, texture.Height + 2 * Passes);
            //gfx.SetRenderTarget(final);
            //gfx.Clear(Color.Transparent);
            //sb.Begin(0, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null, blur);
            //sb.Draw(blurredTexture, Vector2.Zero, Color.White);
            //sb.End();

            RenderTarget2D next = new RenderTarget2D(gfx, texture.Width + 2 * Passes, texture.Height + 2 * Passes);
            gfx.SetRenderTarget(next);
            gfx.Clear(Color.Transparent);
            sb.Begin();
            sb.Draw(texture, Vector2.One * Passes, Color.White);
            sb.End();

            RenderTarget2D prev = next;
            for (int i = 0; i < Passes; i++)
            {
                next = new RenderTarget2D(gfx, texture.Width + 2 * Passes, texture.Height + 2 * Passes);
                gfx.SetRenderTarget(next);
                gfx.Clear(Color.Transparent);
                sb.Begin(0, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, null, null, blur);
                sb.Draw(prev, Vector2.Zero, Color.Red);
                sb.End();
                prev = next;
            }

            gfx.SetRenderTarget(prev);
            //gfx.Clear(Color.Transparent);
            sb.Begin(0, BlendState.AlphaBlend);
            sb.Draw(texture, Vector2.One * Passes, Color.White);
            sb.End();

            gfx.SetRenderTarget(null);
            return prev;
            //for (int i = 0; i < this.Passes; i++)
            //{
                
            //}
        }
    }
}
