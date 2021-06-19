using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Graphics;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Blocks
{
    class BlockWater : Block
    {
        static readonly float AnimationSpeed = Engine.TargetFps / 2f;//10f;
        static float AnimationT = AnimationSpeed;
        //static readonly float FlowSpeed = Engine.TargetFps / 2f;//10f;
        //static float FlowT = FlowSpeed;

        //static List<LiduidFlowProcess> FlowProcesses = new List<LiduidFlowProcess>();

        AtlasDepthNormals.Node.Token[][] Assets;
        enum Fullness { Half, Full };
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Water;

        }
        public BlockWater()
            : base(Block.Types.Water, opaque: false, density: 0.2f, solid: false) //transparency: 0.8f,
        {
            //this.Material = Material.Water;
            //this.MaterialType = MaterialType.Liquid;
            //this.AssetNames = "water/water1half, water/water2half, water/water3half, water/water4half";
            this.AssetNames = "water/water1, water/water2, water/water3, water/water4";
            this.Assets = new AtlasDepthNormals.Node.Token[2][];
            this.Assets[(int)Fullness.Half] = new AtlasDepthNormals.Node.Token[1]{
                Block.Atlas.Load("blocks/water/water1half", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap)
                //,
                //Block.Atlas.Load("blocks/water/water2half", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap),
                //Block.Atlas.Load("blocks/water/water3half", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap),
                //Block.Atlas.Load("blocks/water/water4half", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap)
            };
            this.Assets[(int)Fullness.Full] = new AtlasDepthNormals.Node.Token[1]{
                Block.Atlas.Load("blocks/water/water1")
                //,
                //Block.Atlas.Load("blocks/water/water2"),
                //Block.Atlas.Load("blocks/water/water3"),
                //Block.Atlas.Load("blocks/water/water4")
            };
        }

        public override void Place(IMap map, Vector3 global, byte data, int variation, int orientation)
        {
            base.Place(map, global, data, variation, orientation);
            var flow = new LiduidFlowProcess(map, global, global);
            LiduidFlowProcess.Add(flow);
        }

        public override void NeighborChanged(Net.IObjectProvider net, Vector3 global)
        {
            var above = net.Map.GetBlock(global + Vector3.UnitZ);
            if (above == Block.Water)
            {
                net.Map.GetCell(global).BlockData = 1;
                return;
            }
            var below = net.Map.GetBlock(global - Vector3.UnitZ);
            //if (below == Block.Water)
            //    return;
            //else 
            /// even if the block below is water, we must take into account the neighbor horizontal blocks, they can still flow inside the current block
            /// so, don't return
            if (below == Block.Air)
            {
                LiduidFlowProcess.Add(new LiduidFlowProcess(net.Map, global, global));
                return;
            }
            var neighbors = global.GetNeighbors().Where(f => f.Z == global.Z);
            foreach (var n in neighbors)
                if (net.Map.GetBlock(n) == Block.Air)
                    LiduidFlowProcess.Add(new LiduidFlowProcess(net.Map, global, global));
        }

        public override bool IsTargetable(Vector3 global)
        {
            return false;
        }
        public override float GetHeight(byte data, float x, float y)
        {
            return data == 1 ? 1 : .5f; // if full (1) return 1 height, else return .5f height for half fullness (0)
        }
        //public override float GetDensity(IMap map, Vector3 global)
        //{
        //    var data = map.GetData(global);
        //    return data == 1 ? this.Density : 0;
        //}
        public override float GetDensity(byte data, Vector3 global)
        {
            return data == 1 ? this.Density : 0;
        }
        /// <summary>
        /// 0 is halfblock, 1 is full
        /// </summary>
        /// <param name="depth"></param>
        /// <returns></returns>
        static public byte GetData(int depth)
        {
            return (byte)depth;
        }

        public override void Update()
        {
            AnimationT--;
            if (AnimationT <= 0)
            {
                AnimationT = AnimationSpeed;
                AtlasDepthNormals.Node.Token[] newVariations = new AtlasDepthNormals.Node.Token[4];
                newVariations[0] = this.Variations[1];
                newVariations[1] = this.Variations[2];
                newVariations[2] = this.Variations[3];
                newVariations[3] = this.Variations[0];
                //var r = Rand.Next(4);
                //newVariations[0] = this.Variations[r];
                //this.Variations.RemoveAt(r);
                //r = Rand.Next(3);
                //newVariations[1] = this.Variations[r];
                //this.Variations.RemoveAt(r);
                //r = Rand.Next(2); 
                //newVariations[2] = this.Variations[r];
                //this.Variations.RemoveAt(r);
                //newVariations[3] = this.Variations[0];
                this.Variations = newVariations.ToList();
            }
            LiduidFlowProcess.UpdateProcesses();
            //FlowT--;
            //if (FlowT <= 0)
            //{
            //    FlowT = FlowSpeed;
            //    foreach (var proc in FlowProcesses.ToList())
            //        if (proc.Update())
            //            FlowProcesses.Remove(proc);
            //}
        }
        //public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        //{
        //    //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[Math.Min(cell.Variation, this.Variations.Count - 1)], zoom, fog, tint, sunlight, blocklight, Color.Red.ToVector4(), depth);
        //    sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[cell.BlockData][cell.Variation], zoom, fog, tint, sunlight, blocklight, Color.Red.ToVector4(), depth);
        //}
        public override void Draw(Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, Cell cell)
        {
            //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[cell.BlockData][cell.Variation], camera.Zoom, fog, tint, sunlight, blocklight, Color.Red.ToVector4(), depth);
            camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[cell.BlockData][cell.Variation], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth, blockcoords);

        }
        public override MyVertex[] Draw(Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[cell.BlockData][cell.Variation], camera.Zoom, fog, tint, sunlight, blocklight, Color.Red.ToVector4(), depth);
            //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][variation], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth);
            return camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][0], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth, blockcoords);

        }
        public override MyVertex[] Draw(MySpriteBatch sb, Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[cell.BlockData][cell.Variation], camera.Zoom, fog, tint, sunlight, blocklight, Color.Red.ToVector4(), depth);
            //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][variation], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth);
            return camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][0], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth, blockcoords);

        }
        public override MyVertex[] Draw(Chunk chunk, Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            return chunk.TransparentBlocksVertexBuffer.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][0], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth, blockcoords);
        }
        public override MyVertex[] Draw(MySpriteBatch mesh, MySpriteBatch nonopaquemesh, MySpriteBatch transparentMesh, Chunk chunk, Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            return transparentMesh.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][0], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth, blockcoords);
        }
        //public override void Draw(MySpriteBatch sb, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        //{
        //    //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[cell.BlockData][cell.Variation], camera.Zoom, fog, tint, sunlight, blocklight, Color.Red.ToVector4(), depth);
        //    //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][variation], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth);
        //    camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][0], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth);

        //}
    }

    class LiduidFlowProcess
    {
        static List<LiduidFlowProcess> FlowProcesses = new List<LiduidFlowProcess>();
        static readonly float FlowSpeed = Engine.TargetFps / 2f;//10f;
        static float FlowT = FlowSpeed;

        static Random Rand = new Random(); 
        private Vector3 Source, Current;
        HashSet<Vector3> Handled = new HashSet<Vector3>();
        Queue<Vector3> ToHandle = new Queue<Vector3>();
        IMap Map;


        static public void Add(LiduidFlowProcess proc)
        {
            FlowProcesses.Add(proc);
        }

        public LiduidFlowProcess(IMap map, Vector3 source, Vector3 current)
        {
            this.Map = map;
            // TODO: Complete member initialization
            this.Current = current;
            this.Source = source;
            this.ToHandle.Enqueue(current);
        }

        static public void UpdateProcesses()
        {
            FlowT--;
            if (FlowT > 0)
                return;
            FlowT = FlowSpeed;
            foreach (var proc in FlowProcesses.ToList())
                if (proc.Update())
                    FlowProcesses.Remove(proc);
        }
        internal bool Update()
        {
            if (this.ToHandle.Count == 0)
                return true;
            var current = this.ToHandle.Dequeue();
            this.Handled.Add(current);

            //if (Vector3.Distance(current, this.Source) > 5)
            //    return false;

            var below = current - Vector3.UnitZ;
            var belowBlock = this.Map.GetBlock(below);
            if (belowBlock == Block.Air)
            {
                this.Map.SetBlock(below, Block.Types.Water, 1, Rand.Next(4));
                if (!this.Handled.Contains(below))
                    if (!this.ToHandle.Contains(below))
                        this.ToHandle.Enqueue(below);
                return false;
            }
            /// even if the block below is water, we must take into account the neighbor horizontal blocks, they can still flow inside the current block
            /// so, don't return
            //else if (belowBlock == Block.Water)
            //    return true;


            var east = current + Vector3.UnitX;
            //var eastBlock = map.GetBlock(east);
            var south = current + Vector3.UnitY;
            //var southBlock = map.GetBlock(south);
            var west = current - Vector3.UnitX;
            //var westBlock = map.GetBlock(west);
            var north = current - Vector3.UnitY;
            //var northBlock = map.GetBlock(north);
            foreach (var n in new List<Vector3>() { east, south, west, north })
            {
                var nblock = this.Map.GetBlock(n);
                if (nblock != Block.Air)
                    continue;
                this.Map.SetBlock(n, Block.Types.Water, 0, Rand.Next(4));
                FlowProcesses.Add(new LiduidFlowProcess(this.Map, this.Source, n));
                //if (!this.Handled.Contains(n))
                //    if (!this.ToHandle.Contains(n))
                //        this.ToHandle.Enqueue(n);
            }
            return false;
        }

        //internal bool Update()
        //{
        //    if (this.ToHandle.Count == 0)
        //        return true;
        //    var current = this.ToHandle.Dequeue();
        //    this.Handled.Add(current);

        //    if (Vector3.Distance(current, this.Source) > 5)
        //        return false;

        //    var below = current - Vector3.UnitZ;
        //    var belowBlock = this.Map.GetBlock(below);
        //    if (belowBlock == Block.Air)
        //    {
        //        this.Map.SetBlock(below, Block.Types.Water, 0, Rand.Next(4));
        //        if (!this.Handled.Contains(below))
        //            if (!this.ToHandle.Contains(below))
        //                this.ToHandle.Enqueue(below);
        //        return false;
        //    }
        //    var east = current + Vector3.UnitX;
        //    //var eastBlock = map.GetBlock(east);
        //    var south = current + Vector3.UnitY;
        //    //var southBlock = map.GetBlock(south);
        //    var west = current - Vector3.UnitX;
        //    //var westBlock = map.GetBlock(west);
        //    var north = current - Vector3.UnitY;
        //    //var northBlock = map.GetBlock(north);
        //    foreach (var n in new List<Vector3>() { east, south, west, north })
        //    {
        //        var nblock = this.Map.GetBlock(n);
        //        if (nblock != Block.Air)
        //            continue;
        //        this.Map.SetBlock(n, Block.Types.Water, 0, Rand.Next(4));
        //        if (!this.Handled.Contains(n))
        //            if (!this.ToHandle.Contains(n))
        //                this.ToHandle.Enqueue(n);
        //    }
        //    return false;
        //}
    }
}
