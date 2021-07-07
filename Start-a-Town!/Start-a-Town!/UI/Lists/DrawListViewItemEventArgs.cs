using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class DrawListViewItemEventArgs : EventArgs
    {
        public SpriteBatch SpriteBatch;
        public Rectangle Bounds;
        public ListViewItem Item;

        public DrawListViewItemEventArgs(SpriteBatch sb, Rectangle bounds, 
            ListViewItem item)
        {
            SpriteBatch = sb;
            Bounds = bounds;
            Item = item;
        }
    }
}
