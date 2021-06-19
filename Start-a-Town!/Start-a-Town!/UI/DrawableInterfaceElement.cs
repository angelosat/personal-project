using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
	public abstract class DrawableInterfaceElement
	{
        public virtual void Update() { }
        public virtual void Draw(SpriteBatch sb) { }

        public virtual void Destroy() { }

        public int Width, Height;
        protected Vector2 CenterScreen
        { get { return new Vector2((UIManager.Width - Width) / 2, (UIManager.Height - Height) / 2); } }
	}
}
