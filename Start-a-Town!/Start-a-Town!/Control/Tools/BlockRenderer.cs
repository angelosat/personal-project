using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        public BlockRendererNew()
        {

        }
        public BlockRendererNew(AtlasDepthNormals.Node.Token blockTexture)
        {
            this.BlockToken = blockTexture;
        }
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
                        this.BlockToken,
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
            this.Slices.Clear();
        }
    }
    public class BlockRendererObservable
    {
        readonly ObservableCollection<IntVec3> Cells;// = new();
        readonly Dictionary<int, MySpriteBatch> Slices = new();
        readonly HashSet<int> InvalidatedSlices = new();
        readonly AtlasDepthNormals.Node.Token BlockToken;

        public BlockRendererObservable(ObservableCollection<IntVec3> cells)
            : this(Block.BlockBlueprint, cells)
        {
        }
        public BlockRendererObservable(AtlasDepthNormals.Node.Token texToken, ObservableCollection<IntVec3> cells)
        {
            this.BlockToken = texToken;
            this.Cells = cells;
            this.Cells.CollectionChanged += this.Cells_CollectionChanged;
        }
        private void Cells_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems is not null)
                foreach (var z in e.OldItems.Cast<IntVec3>().Select(cell => cell.Z))
                    this.InvalidatedSlices.Add(z);
            if (e.NewItems is not null)
                foreach (var z in e.NewItems.Cast<IntVec3>().Select(cell => cell.Z))
                    this.InvalidatedSlices.Add(z);
        }
        void Validate(Camera camera)
        {
            if (!this.InvalidatedSlices.Any())
                return;
            var bySlice = this.Cells.ToLookup(c => c.Z);
            foreach (var z in this.InvalidatedSlices)
            {
                if (!bySlice.Contains(z))
                    this.Slices.Remove(z);
                else
                {
                    var cells = bySlice[z];
                    var slice = this.Slices.GetOrAdd(z, sliceCtor);
                    slice.Clear();
                    foreach (var cell in cells)
                        camera.DrawBlockSelectionGlobal(
                            slice,
                            this.BlockToken,
                            cell);
                }
            }
            this.InvalidatedSlices.Clear();

            static MySpriteBatch sliceCtor()
            {
                return new(Game1.Instance.GraphicsDevice);
            }
        }
       
        public void DrawBlocks(MapBase map, Camera camera, IEnumerable<IntVec3> positions)
        {
            this.Validate(camera);
            camera.PrepareShader(map);
            Coords.Rotate(camera, 0, 0, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(0, 0, (rotx + roty) * Chunk.Size));
            camera.Effect.Parameters["World"].SetValue(world);
            camera.Effect.CurrentTechnique.Passes["Pass1"].Apply();
            foreach (var slice in this.Slices)
                if (slice.Key <= camera.DrawLevel)
                    slice.Value.Draw();
        }
    }

}
