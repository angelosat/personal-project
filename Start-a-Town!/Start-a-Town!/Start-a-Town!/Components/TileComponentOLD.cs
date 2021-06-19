using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace Start_a_Town_.Components
{
    /// <summary>
    /// A component that stores and can display a stack of tiles (base tile + tile overhangs)
    /// </summary>
    class TileComponent : Component
    {
        static Texture2D SpriteSheet;
        TileBase _Base;
        SortedDictionary<TileOverhang.Types, TileOverhang> Overhangs;

        public TileComponent(Texture2D spriteSheet)
        {
            SpriteSheet = spriteSheet;
            Overhangs = new SortedDictionary<TileOverhang.Types, TileOverhang>();
        }

        public void Attach(TileOverhang overhang)
        {
            Overhangs[overhang.Type] = overhang;
        }

        public TileBase Base
        {
            get { return _Base; }
            set { _Base = value; }
        }

        //public virtual void Draw(Camera camera, SelectionArgs tileSelection, Chunk chunk, Cell cell)//, Rectangle bounds)//Vector2 pos, Rectangle bounds, Color color)
        //{
        //    //Base.Draw(camera, tileSelection, chunk, cell);
        //    foreach (KeyValuePair<TileOverhang.Types, TileOverhang> pair in Overhangs)
        //        pair.Value.Draw(camera, camera.Transform(cell.GetGlobalCoords(chunk)), Color.White);
        //}
    }
}
