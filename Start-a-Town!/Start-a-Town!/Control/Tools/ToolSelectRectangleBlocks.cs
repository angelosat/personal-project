using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            //camera.DrawGridBlocks(this.Batch, Block.BlockBlueprint, positions, Color.White);
            var texture = Block.BlockBlueprint;
            texture.Atlas.Begin(this.Batch);
            foreach (var pos in positions)
                //camera.DrawGridBlockNoFlush(this.Batch, texture, Color.White, pos);
                camera.DrawBlockGlobal(this.Batch, map, pos);
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            if (!this.Enabled)
                return;
            //base.DrawBeforeWorld(sb, map, camera);
            //return;
            
            if (this.End.Round() != this.PrevEnd)
            {
                this.Validate(map, camera);
                this.PrevEnd = this.End.Round();
            }
            camera.PrepareShader(map);
            float x, y;
            //Coords.Iso(cam, this.MapCoords.X * Chunk.Size, this.MapCoords.Y * Chunk.Size, 0, out x, out y);
            Coords.Iso(camera, 0 * Chunk.Size, 0 * Chunk.Size, 0, out x, out y);
            int rotx, roty;
            Coords.Rotate(camera, 0, 0, out rotx, out roty);
            var world = Matrix.CreateTranslation(new Vector3(x, y, ((rotx + roty) * Chunk.Size)));

            //var world = Matrix.CreateTranslation(new Vector3(x, y, ((this.MapCoords.X + this.MapCoords.Y) * Chunk.Size)));
            camera.Effect.Parameters["World"].SetValue(world);

            // var view =// Matrix.Identity;
            //new Matrix(
            //   1.0f, 0.0f, 0.0f, 0.0f,
            //   0.0f, -1.0f, 0.0f, 0.0f,
            //   0.0f, 0.0f, 1.0f, 0.0f,
            //   0.0f, 0.0f, 0.0f, 1.0f);
            // float camerax = camera.Coordinates.X;
            // float cameray = camera.Coordinates.Y;
            // view = view * Matrix.CreateTranslation(new Vector3(-camerax, cameray, 0)) * Matrix.CreateScale(camera.Zoom) * Matrix.CreateTranslation(new Vector3(this.Width / 2, -this.Height / 2, 0));
            // camera.Effect.Parameters["View"].SetValue(view);

            camera.Effect.CurrentTechnique.Passes["Pass1"].Apply();

            this.Batch.Draw();
        }
    }
}
