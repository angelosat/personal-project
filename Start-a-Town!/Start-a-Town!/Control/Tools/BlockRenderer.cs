using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    public class BlockRenderer
    {
        readonly MySpriteBatch Batch = new(Game1.Instance.GraphicsDevice);
        public void CreateMesh(Camera camera, IEnumerable<IntVec3> positions)
        {
            this.Batch.Clear();
            foreach (var pos in positions)
                camera.DrawBlockSelectionGlobal(this.Batch, pos);
        }
        public void DrawBlocks(MapBase map, Camera camera)
        {
            camera.PrepareShader(map);
            Coords.Iso(camera, 0 * Chunk.Size, 0 * Chunk.Size, 0, out float x, out float y);
            Coords.Rotate(camera, 0, 0, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(x, y, (rotx + roty) * Chunk.Size));
            camera.Effect.Parameters["World"].SetValue(world);
            camera.Effect.CurrentTechnique.Passes["Pass1"].Apply();
            this.Batch.Draw();
        }
    }
    public class BlockRendererNew
    {
        readonly Dictionary<int, MySpriteBatch> Slices = new();
        readonly AtlasDepthNormals.Node.Token BlockToken = Block.BlockBlueprint;
        bool Validated;
        public void CreateMesh(Camera camera, IEnumerable<IntVec3> positions)
        {
            if (this.Validated)
                return;
            this.Validated = true;
            this.Slices.Clear();
            foreach (var cells in positions.GroupBy(g => g.Z))
            {
                foreach (var cell in cells)
                    camera.DrawBlockSelectionGlobal(
                        this.Slices.GetOrAdd(cells.Key, sliceCtor),
                        cell);
            }

            static MySpriteBatch sliceCtor()
            {
                return new(Game1.Instance.GraphicsDevice);
            }
        }
        public void DrawBlocks(MapBase map, Camera camera, IEnumerable<IntVec3> positions)
        {
            this.CreateMesh(camera, positions);
            camera.PrepareShader(map);
            Coords.Rotate(camera, 0, 0, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(0, 0, (rotx + roty) * Chunk.Size));
            camera.Effect.Parameters["World"].SetValue(world);
            camera.Effect.CurrentTechnique.Passes["Pass1"].Apply();
            foreach (var slice in this.Slices)
                if (slice.Key <= camera.DrawLevel)
                    slice.Value.Draw();
        }

        internal void Invalidate()
        {
            this.Validated = false;
        }
    }
    public class BlockRendererNewNew
    {
        readonly HashSet<IntVec3> Cells = new();
        readonly Dictionary<int, MySpriteBatch> Slices = new();
        bool Validated;
        public void CreateMesh(Camera camera, IEnumerable<IntVec3> positions)
        {
            if (this.Validated)
                return;
            this.Validated = true;
            this.Slices.Clear();
            foreach (var cells in positions.GroupBy(g => g.Z))
            {
                foreach (var cell in cells)
                    camera.DrawBlockSelectionGlobal(
                        this.Slices.GetOrAdd(cells.Key, sliceCtor),
                        cell);
            }

            static MySpriteBatch sliceCtor()
            {
                return new(Game1.Instance.GraphicsDevice);
            }
        }
        public void DrawBlocks(MapBase map, Camera camera, IEnumerable<IntVec3> positions)
        {
            this.CreateMesh(camera, positions);
            camera.PrepareShader(map);
            Coords.Rotate(camera, 0, 0, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(0, 0, (rotx + roty) * Chunk.Size));
            camera.Effect.Parameters["World"].SetValue(world);
            camera.Effect.CurrentTechnique.Passes["Pass1"].Apply();
            foreach (var slice in this.Slices)
                if (slice.Key <= camera.DrawLevel)
                    slice.Value.Draw();
        }

        internal void Invalidate()
        {
            this.Validated = false;
        }
    }

}
