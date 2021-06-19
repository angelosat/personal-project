using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
namespace Start_a_Town_
{
    public class Canvas
    {
        public MySpriteBatch Opaque, NonOpaque, Transparent, Designations;
        public Canvas(GraphicsDevice gd)
        {
            this.Opaque = new MySpriteBatch(gd);
            this.NonOpaque = new MySpriteBatch(gd);
            this.Transparent = new MySpriteBatch(gd);
            this.Designations = new MySpriteBatch(gd);
        }
        public Canvas(GraphicsDevice gd, int size)
        {
            this.Opaque = new MySpriteBatch(gd, size);
            this.NonOpaque = new MySpriteBatch(gd, size);
            this.Transparent = new MySpriteBatch(gd, size);
            this.Designations = new MySpriteBatch(gd, size);
        }
    }
}
