using Microsoft.Xna.Framework;
using System;
using System.Linq;
using System.Windows.Forms;

namespace Start_a_Town_
{
    class ToolSelectRectangleBlocks : ToolDigging
    {
        IntVec3 PrevEnd;
        readonly BlockRenderer Renderer = new();
        public ToolSelectRectangleBlocks()
        {
        }
        public ToolSelectRectangleBlocks(IntVec3 origin, Action<IntVec3, IntVec3, bool> callback) : base(callback)
        {
            this.Begin = origin;
            this.End = this.Begin;
            this.Width = this.Height = 1;
            this.Enabled = true;
        }
        public override Messages MouseLeftUp(HandledMouseEventArgs e)
        {
            base.MouseLeftUp(e);
            return Messages.Remove;
        }
        public override Messages MouseRightUp(HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }
        /// <summary>
        /// TODO optimize
        /// </summary>
        /// <param name="map"></param>
        /// <param name="camera"></param>
        void Validate(MapBase map, Camera camera)
        {
            var positions =
                this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air || map.IsUndiscovered(v)); // we want to include undiscovered air cells in selection
            this.Renderer.CreateMesh(camera, positions);
        }

        internal override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (!this.Enabled)
                return;

            if (this.End != this.PrevEnd)
            {
                this.Validate(map, camera);
                this.PrevEnd = this.End;
            }
            this.Renderer.DrawBlocks(map, camera);
        }
    }
}
