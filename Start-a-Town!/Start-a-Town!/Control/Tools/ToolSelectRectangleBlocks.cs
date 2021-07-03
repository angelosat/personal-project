using System;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class ToolSelectRectangleBlocks : ToolDigging
    {
        Vector3 PrevEnd;
        MySpriteBatch Batch;
        public ToolSelectRectangleBlocks()
        {

        }
        public ToolSelectRectangleBlocks(Vector3 origin, Action<Vector3, Vector3, bool> callback) : base(callback)
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
        void Validate(IMap map, Camera camera)
        {
            this.Batch = new MySpriteBatch(Game1.Instance.GraphicsDevice);
            var positions = this.Begin.GetBox(this.End)
                .Where(v => map.GetBlock(v) != BlockDefOf.Air).ToList();
            var texture = Block.BlockBlueprint;
            texture.Atlas.Begin(this.Batch);
            foreach (var pos in positions)
                camera.DrawBlockGlobal(this.Batch, map, pos);
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            if (!this.Enabled)
                return;
            
            if (this.End.Round() != this.PrevEnd)
            {
                this.Validate(map, camera);
                this.PrevEnd = this.End.Round();
            }
            camera.PrepareShader(map);
            float x, y;
            Coords.Iso(camera, 0 * Chunk.Size, 0 * Chunk.Size, 0, out x, out y);
            Coords.Rotate(camera, 0, 0, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(x, y, ((rotx + roty) * Chunk.Size)));
            camera.Effect.Parameters["World"].SetValue(world);
            camera.Effect.CurrentTechnique.Passes["Pass1"].Apply();
            this.Batch.Draw();
        }
    }
}
