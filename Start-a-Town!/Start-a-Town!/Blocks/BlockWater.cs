using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Blocks
{
    class BlockWater : Block
    {
        static readonly float AnimationSpeed = Engine.TicksPerSecond / 2f;
        static float AnimationT = AnimationSpeed;
        
        AtlasDepthNormals.Node.Token[][] Assets;
        enum Fullness { Half, Full };
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Water;

        }
        public BlockWater()
            : base(Block.Types.Water, opaque: false, density: 0.2f, solid: false)
        {
            this.AssetNames = "water/water1, water/water2, water/water3, water/water4";
            this.Assets = new AtlasDepthNormals.Node.Token[2][];
            this.Assets[(int)Fullness.Half] = new AtlasDepthNormals.Node.Token[1]{
                Block.Atlas.Load("blocks/water/water1half", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap)
            };
            this.Assets[(int)Fullness.Full] = new AtlasDepthNormals.Node.Token[1]{
                Block.Atlas.Load("blocks/water/water1")
            };
        }

        public override void Place(MapBase map, IntVec3 global, byte data, int variation, int orientation, bool notify = true)
        {
            base.Place(map, global, data, variation, orientation, notify);
            var flow = new LiduidFlowProcess(map, global, global);
            LiduidFlowProcess.Add(flow);
        }

        public override void NeighborChanged(IObjectProvider net, IntVec3 global)
        {
            var above = net.Map.GetBlock(global + IntVec3.UnitZ);
            if (above == BlockDefOf.Water)
            {
                net.Map.GetCell(global).BlockData = 1;
                return;
            }
            var below = net.Map.GetBlock(global - IntVec3.UnitZ);
            if (below == BlockDefOf.Air)
            {
                LiduidFlowProcess.Add(new LiduidFlowProcess(net.Map, global, global));
                return;
            }
            var neighbors = global.GetNeighbors().Where(f => f.Z == global.Z);
            foreach (var n in neighbors)
                if (net.Map.GetBlock(n) == BlockDefOf.Air)
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
                
                this.Variations = newVariations.ToList();
            }
            LiduidFlowProcess.UpdateProcesses();
            
        }
        
        public override MyVertex[] Draw(MySpriteBatch sb, Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            return camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][0], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth, this, blockcoords);
        }
        public override MyVertex[] Draw(Chunk chunk, Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            return chunk.Canvas.Transparent.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][0], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth, this, blockcoords);
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            return canvas.Transparent.DrawBlock(Block.Atlas.Texture, screenBounds, this.Assets[data][0], camera.Zoom, fog, tint, Color.White, sunlight, blocklight, Color.Red.ToVector4(), depth, this, blockcoords);
        }
    }
}
