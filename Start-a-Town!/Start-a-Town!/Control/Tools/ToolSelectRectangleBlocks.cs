using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class ToolSelectRectangleBlocks : ToolDigging
    {
        IntVec3 PrevEnd;
        MySpriteBatch Batch = new(Game1.Instance.GraphicsDevice);
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
        public override void Update()
        {
            base.Update();
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
            this.Batch.Clear();
            foreach (var pos in positions)
                camera.DrawBlockSelectionGlobal(this.Batch, pos);
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
            camera.PrepareShader(map);
            Coords.Iso(camera, 0 * Chunk.Size, 0 * Chunk.Size, 0, out float x, out float y);
            Coords.Rotate(camera, 0, 0, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(x, y, (rotx + roty) * Chunk.Size));
            camera.Effect.Parameters["World"].SetValue(world);
            camera.Effect.CurrentTechnique.Passes["Pass1"].Apply();
            this.Batch.Draw();
        }
    }
}
