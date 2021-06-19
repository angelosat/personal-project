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
    class BlockWindow : Block
    {
        AtlasDepthNormals.Node.Token[] TopParts, BottomParts;
        AtlasDepthNormals.Node.Token[][] Parts;
        AtlasDepthNormals.Node.Token[][][] PartsSeparate;
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Glass;
        }
        public BlockWindow()
            : base(Types.Window, opaque: false)
        {
            //this.MaterialType = MaterialType.Glass;
            //this.Material = Material.Glass;

            this.PartsSeparate = 
                new AtlasDepthNormals.Node.Token[][][] { 
                    new AtlasDepthNormals.Node.Token[][]{
                        new AtlasDepthNormals.Node.Token[]{
                            Block.Atlas.Load("blocks/windows/windowframebottom", Map.BlockDepthMap, Block.NormalMap),
                            Block.Atlas.Load("blocks/windows/glassbottom", Map.BlockDepthMap, Block.NormalMap)},
                        new AtlasDepthNormals.Node.Token[]{
                            Block.Atlas.Load("blocks/windows/windowframebottom2", Map.BlockDepthMap, Block.NormalMap),
                            Block.Atlas.Load("blocks/windows/glassbottom2", Map.BlockDepthMap, Block.NormalMap)}},
                    new AtlasDepthNormals.Node.Token[][]{
                        new AtlasDepthNormals.Node.Token[]{
                            Block.Atlas.Load("blocks/windows/windowframetop", Map.BlockDepthMap, Block.NormalMap),
                            Block.Atlas.Load("blocks/windows/glasstop", Map.BlockDepthMap, Block.NormalMap)},
                        new AtlasDepthNormals.Node.Token[]{
                            Block.Atlas.Load("blocks/windows/windowframetop2", Map.BlockDepthMap, Block.NormalMap),
                            Block.Atlas.Load("blocks/windows/glasstop2", Map.BlockDepthMap, Block.NormalMap)}}
            };

            //this.Parts = new AtlasDepthNormals.Node.Token[2][];
            //this.Parts[0] = this.BottomParts;
            //this.Parts[1] = this.TopParts;

            this.Variations.Add(this.PartsSeparate.First().First().First());

            //this.BottomParts = new AtlasDepthNormals.Node.Token[] { 
            //    Block.Atlas.Load("blocks/house/windowbottom", Map.BlockDepthMap, Block.NormalMap),
            //    Block.Atlas.Load("blocks/house/windowbottom2", Map.BlockDepthMap, Block.NormalMap)
            //};
            //this.TopParts = new AtlasDepthNormals.Node.Token[] { 
            //    Block.Atlas.Load("blocks/house/windowtop", Map.BlockDepthMap, Block.NormalMap),
            //    Block.Atlas.Load("blocks/house/windowtop2", Map.BlockDepthMap, Block.NormalMap)
            //};

            //this.Parts = new AtlasDepthNormals.Node.Token[2][];
            //this.Parts[0] = this.BottomParts;
            //this.Parts[1] = this.TopParts;

            //this.Variations.Add(this.BottomParts.First());
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

        public override void Place(IMap map, Vector3 global, byte data, int variation, int orientation)
        {
            if (!IsPositionValid(map, global))
                return;
            base.Place(map, global, GetData(0), variation, orientation);
            base.Place(map, global + Vector3.UnitZ, GetData(1), variation, orientation);
        }
        public override void Remove(IMap map, Vector3 global)
        {
            var part = map.GetData(global);
            base.Remove(map, global);

            switch(part)
            {
                case 0:
                    base.Remove(map, global + Vector3.UnitZ);
                    break;

                case 1:
                    base.Remove(map, global - Vector3.UnitZ);
                    break;

                default:
                    throw new ArgumentException();
            }
        }

        bool IsPositionValid(IMap map, Vector3 global)
        {
            if (map.GetBlock(global) != Block.Air)
                return false;
            if (map.GetBlock(global + Vector3.UnitZ) != Block.Air)
                return false;
            return true;
        }
        public override MyVertex[] Draw(Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            var partindex = GetPartIndex(data);
            var orientationindex = (int)(orientation + camera.Rotation) % 2;
            var parts = this.PartsSeparate[partindex][orientationindex];
            var frame = parts[0];
            var glass = parts[1];
            //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, Color.White, sunlight, blocklight, depth);
            camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, frame, camera.Zoom, fog, Color.White, sunlight, blocklight, depth);
            return camera.TransparentBlocksSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, glass, camera.Zoom, fog, Color.White, sunlight, blocklight, depth, blockcoords);
        }
        //public override void Draw(Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        //{
        //    var partindex = GetPartIndex(data);
        //    var orientationindex = (int)(orientation + camera.Rotation) % 2;
        //    var token = this.Parts[partindex][orientationindex];
        //    //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, Color.White, sunlight, blocklight, depth);
        //    camera.TransparentBlocksSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, Color.White, sunlight, blocklight, depth);

        //}
        public override MyVertex[] Draw(MySpriteBatch sb, Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            var partindex = GetPartIndex(data);
            var orientationindex = (int)(orientation + camera.Rotation) % 2;
            var token = this.PartsSeparate[partindex][orientationindex][0];
            //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, Color.White, sunlight, blocklight, depth);
            return sb.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, Color.White, sunlight, blocklight, depth, blockcoords);

        }
        //public override void Draw(MySpriteBatch sb, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        //{
        //    var partindex = GetPartIndex(data);
        //    var orientationindex = (int)(orientation + camera.Rotation) % 2;
        //    var token = this.Parts[partindex][orientationindex];
        //    //camera.WaterSpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, Color.White, sunlight, blocklight, depth);
        //    sb.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, Color.White, sunlight, blocklight, depth);

        //}

        public override void DrawPreview(MySpriteBatch sb, IMap map, Vector3 global, Camera cam, byte data, int orientation = 0)
        {
            var orientationindex = (int)(orientation + cam.Rotation) % 2;
            var bottom = this.PartsSeparate[0][orientationindex][0];
            var top = this.PartsSeparate[1][orientationindex][0];
            sb.DrawBlock(Block.Atlas.Texture, map, global, bottom, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
            sb.DrawBlock(Block.Atlas.Texture, map, global + Vector3.UnitZ, top, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
        }

        public override MyVertex[] Draw(MySpriteBatch opaquemesh, MySpriteBatch nonopaquemesh, MySpriteBatch transparentMesh, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            var partindex = GetPartIndex(data);
            var orientationindex = (int)(orientation + camera.Rotation) % 2;
            var parts = this.PartsSeparate[partindex][orientationindex];
            var frame = parts[0];
            var glass = parts[1];
            opaquemesh.DrawBlock(Block.Atlas.Texture, screenBounds, frame, camera.Zoom, fog, Color.White, sunlight, blocklight, depth, blockCoordinates);
            return transparentMesh.DrawBlock(Block.Atlas.Texture, screenBounds, glass, camera.Zoom, fog, Color.White, sunlight, blocklight, depth, blockCoordinates);
        }

        //public override void DrawPreview(MySpriteBatch sb, IMap map, Vector3 global, Camera cam, byte data, int orientation = 0)
        //{
        //    var orientationindex = (int)(orientation + cam.Rotation) % 2;
        //    var bottom = this.Parts[0][orientationindex];
        //    var top = this.Parts[1][orientationindex];
        //    sb.DrawBlock(Block.Atlas.Texture, map, global, bottom, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
        //    sb.DrawBlock(Block.Atlas.Texture, map, global + Vector3.UnitZ, top, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
        //}
    }
}
