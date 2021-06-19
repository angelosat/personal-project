using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Graphics
{
    public abstract class AtlasBase
    {
        public Texture2D Texture;
        public Texture2D DepthTexture;
        internal void Begin(MySpriteBatch sb)
        {
            if (Game1.Instance.GraphicsDevice.Textures[0] == this.Texture)
                sb.Flush();
            Game1.Instance.GraphicsDevice.Textures[0] = this.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = this.DepthTexture;
        }
    }
}
