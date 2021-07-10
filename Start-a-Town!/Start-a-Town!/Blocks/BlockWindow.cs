using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Blocks
{
    class BlockWindow : Block
    {
        AtlasDepthNormals.Node.Token[][][] PartsSeparate;
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Glass;
        }
        public BlockWindow()
            : base(Types.Window, opaque: false)
        {
            this.PartsSeparate = 
                new AtlasDepthNormals.Node.Token[][][] { 
                    new AtlasDepthNormals.Node.Token[][]{
                        new AtlasDepthNormals.Node.Token[]{
                            Block.Atlas.Load("blocks/windows/windowframebottom", MapBase.BlockDepthMap, Block.NormalMap),
                            Block.Atlas.Load("blocks/windows/glassbottom", MapBase.BlockDepthMap, Block.NormalMap)},
                        new AtlasDepthNormals.Node.Token[]{
                            Block.Atlas.Load("blocks/windows/windowframebottom2", MapBase.BlockDepthMap, Block.NormalMap),
                            Block.Atlas.Load("blocks/windows/glassbottom2", MapBase.BlockDepthMap, Block.NormalMap)}},
                    new AtlasDepthNormals.Node.Token[][]{
                        new AtlasDepthNormals.Node.Token[]{
                            Block.Atlas.Load("blocks/windows/windowframetop", MapBase.BlockDepthMap, Block.NormalMap),
                            Block.Atlas.Load("blocks/windows/glasstop", MapBase.BlockDepthMap, Block.NormalMap)},
                        new AtlasDepthNormals.Node.Token[]{
                            Block.Atlas.Load("blocks/windows/windowframetop2", MapBase.BlockDepthMap, Block.NormalMap),
                            Block.Atlas.Load("blocks/windows/glasstop2", MapBase.BlockDepthMap, Block.NormalMap)}}

            
            };

            this.Variations.Add(this.PartsSeparate.First().First().First());


            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent()),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Doors.Add(this.Recipe);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="part">0: bottom part, 1: top part</param>
        /// <returns></returns>
        static byte GetData(int part)
        {
            if (part != 0 && part != 1)
                throw new ArgumentException();
            return (byte)part;
        }

        static int GetPartIndex(byte data)
        {
            return (int)data;
        }

        public override void Place(MapBase map, Vector3 global, byte data, int variation, int orientation, bool notify = true)
        {
            if (!IsPositionValid(map, global))
                return;
            base.Place(map, global, GetData(0), variation, orientation);
            base.Place(map, global + Vector3.UnitZ, GetData(1), variation, orientation, notify);
        }
        public override void Remove(MapBase map, Vector3 global, bool notify = true)
        {
            var part = map.GetBlockData(global);
            base.Remove(map, global);

            switch(part)
            {
                case 0:
                    base.Remove(map, global + Vector3.UnitZ, notify);
                    break;

                case 1:
                    base.Remove(map, global - Vector3.UnitZ, notify);
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        bool IsPositionValid(MapBase map, Vector3 global)
        {
            if (map.GetBlock(global) != BlockDefOf.Air)
                return false;
            if (map.GetBlock(global + Vector3.UnitZ) != BlockDefOf.Air)
                return false;
            return true;
        }

        public override void DrawPreview(MySpriteBatch sb, MapBase map, Vector3 global, Camera cam, byte data, int variation = 0, int orientation = 0)
        {
            var orientationindex = (int)(orientation + cam.Rotation) % 2;
            var bottom = this.PartsSeparate[0][orientationindex][0];
            var top = this.PartsSeparate[1][orientationindex][0];
            sb.DrawBlock(Block.Atlas.Texture, map, global, bottom, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
            sb.DrawBlock(Block.Atlas.Texture, map, global + Vector3.UnitZ, top, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
        }

        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            //15 18 65
            //sunlight 255 16 239(159?) 255
            //blocklight .0625 1

            //15 20 65
            //sunlight 255 16 239(191?) 255
            //blocklight .0625 1

            //15 18 66
            //sunlight 255 16 16 255
            //blocklight .0625 1

            //15 20 66
            //sunlight255 16 16 255
            //blocklight .0625 1

            var partindex = GetPartIndex(data);
            var orientationindex = (int)(orientation + camera.Rotation) % 2;
            var parts = this.PartsSeparate[partindex][orientationindex];
            var frame = parts[0];
            var glass = parts[1];
            canvas.Opaque.DrawBlock(Block.Atlas.Texture, screenBounds, frame, camera.Zoom, fog, Color.White, sunlight, blocklight, depth, this, blockCoordinates);
            return canvas.Transparent.DrawBlock(Block.Atlas.Texture, screenBounds, glass, camera.Zoom, fog, Color.White, sunlight, blocklight, depth, this, blockCoordinates);
        }
    }
}
