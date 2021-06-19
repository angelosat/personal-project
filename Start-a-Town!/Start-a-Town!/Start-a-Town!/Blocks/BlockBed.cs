using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Blocks
{
    class BlockBed : Block
    {
        public enum Part { Bottom = 0x0, Top = 0x1 }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.LightWood;
        }
        AtlasDepthNormals.Node.Token[] TopParts, BottomParts;
        AtlasDepthNormals.Node.Token[][] Parts;
        public BlockBed():base(Block.Types.Bed, 0f, 1f, false, true)
        {
            //this.AssetNames = "bed/bedslimbottom, bed/bedslimtop";
            //this.Material = Material.LightWood;
            //this.MaterialType = MaterialType.Wood;
            this.Recipe = new BlockConstruction(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfSubType(ItemSubType.Planks),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockConstruction.Product(this),
                    Components.Skills.Skill.Building);

            this.BottomParts = new AtlasDepthNormals.Node.Token[] { 
                Block.Atlas.Load("blocks/bed/bedslimbottom", "blocks/bed/bedslimbottomdepth", "blocks/bed/bedslimbottomnormal"),
                Block.Atlas.Load("blocks/bed/bedslimbottom2", "blocks/bed/bedslimbottom2depth", "blocks/bed/bedslimbottom2normal"),
                Block.Atlas.Load("blocks/bed/bedslimtop", "blocks/bed/bedslimtopdepth", "blocks/bed/bedslimtopnormal"),
                Block.Atlas.Load("blocks/bed/bedslimtop2", "blocks/bed/bedslimtop2depth", "blocks/bed/bedslimtop2normal")
            };
            this.TopParts = new AtlasDepthNormals.Node.Token[] { 
                Block.Atlas.Load("blocks/bed/bedslimtop", "blocks/bed/bedslimtopdepth", "blocks/bed/bedslimtopnormal"),
                Block.Atlas.Load("blocks/bed/bedslimtop2", "blocks/bed/bedslimtop2depth", "blocks/bed/bedslimtop2normal"),
                Block.Atlas.Load("blocks/bed/bedslimbottom", "blocks/bed/bedslimbottomdepth", "blocks/bed/bedslimbottomnormal"),
                Block.Atlas.Load("blocks/bed/bedslimbottom2", "blocks/bed/bedslimbottom2depth", "blocks/bed/bedslimbottom2normal")
            };
            this.Variations.Add(this.BottomParts.First());
            //this.BottomParts = new AtlasDepthNormals.Node.Token[] { 
            //    Block.Atlas.Load("blocks/bed/bedslimbottom", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap),
            //    Block.Atlas.Load("blocks/bed/bedslimbottom2", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap),
            //    Block.Atlas.Load("blocks/bed/bedslimtop", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap),
            //    Block.Atlas.Load("blocks/bed/bedslimtop2", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap)
            //};
            //this.TopParts = new AtlasDepthNormals.Node.Token[] { 
            //    Block.Atlas.Load("blocks/bed/bedslimtop", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap),
            //    Block.Atlas.Load("blocks/bed/bedslimtop2", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap),
            //    Block.Atlas.Load("blocks/bed/bedslimbottom", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap),
            //    Block.Atlas.Load("blocks/bed/bedslimbottom2", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap)
            //};

            this.Parts = new AtlasDepthNormals.Node.Token[2][];
            this.Parts[0] = this.BottomParts;
            this.Parts[1] = this.TopParts;
        }

        public override float GetHeight(byte data, float x, float y)
        {
            return 0.5f;
        }

        static public void GetState(byte data, out Part part, out int orientation)
        {
            part = (Part)(data & 0x1);
            orientation = (int)(data & 0x6) >> 1;
        }
        static public void GetState(Cell cell, out Part part, out int orientation)
        {
            GetState(cell.BlockData, out part, out orientation);
        }
        static public void GetState(IMap map, Vector3 global, out Part part, out int orientation)
        {
            GetState(map.GetCell(global), out part, out orientation);
        }
        static public byte GetData(Part part, int orientation)
        {
            byte data = 0;
            data = (byte)(data | (byte)part);
            data = (byte)(data | ((byte)orientation << 1));
            return data;
        }

        public override void Place(IMap map, Vector3 global, byte data, int variation, int orientation)
        {
            if (!IsValidPosition(map, global, orientation))
                return;
            switch (orientation)
            {
                case 0:
                    map.SetBlock(global, Block.Types.Bed, GetData(Part.Bottom, orientation), 0);
                    map.SetBlock(global - Vector3.UnitX, Block.Types.Bed, GetData(Part.Top, orientation), 0);
                    break;
                case 1:
                    map.SetBlock(global, Block.Types.Bed, GetData(Part.Bottom, orientation), 0);
                    map.SetBlock(global - Vector3.UnitY, Block.Types.Bed, GetData(Part.Top, orientation), 0);
                    break;
                case 2:
                    map.SetBlock(global, Block.Types.Bed, GetData(Part.Bottom, orientation), 0);
                    map.SetBlock(global + Vector3.UnitX, Block.Types.Bed, GetData(Part.Top, orientation), 0);
                    break;
                case 3:
                    map.SetBlock(global, Block.Types.Bed, GetData(Part.Bottom, orientation), 0);
                    map.SetBlock(global + Vector3.UnitY, Block.Types.Bed, GetData(Part.Top, orientation), 0);
                    break;
                default: break;
            }
            //map.SetBlock(global, Block.Types.Bed, GetData(Part.Bottom), 0);
            //map.SetBlock(global - Vector3.UnitX, Block.Types.Bed, GetData(Part.Top), 0);
        }
        public override void Remove(IMap map, Vector3 global)
        {
            Part part;
            int ori;
            GetState(map, global, out part, out ori);
            map.SetBlock(global, Types.Air);
            var i = part == 0 ? 1 : -1;
            switch (ori)
            {
                case 0:
                    map.SetBlock(global - i * Vector3.UnitX, Block.Types.Air, 0, 0);
                    break;
                case 1:
                    map.SetBlock(global - i * Vector3.UnitY, Block.Types.Air, 0, 0);
                    break;
                case 2:
                    map.SetBlock(global + i * Vector3.UnitX, Block.Types.Air, 0, 0);
                    break;
                case 3:
                    map.SetBlock(global + i * Vector3.UnitY, Block.Types.Air, 0, 0);
                    break;
                default: break;
            }
        }
        static bool IsValidPosition(IMap map, Vector3 global, int orientation)
        {
            var positions = new List<Vector3>();
            positions.Add(global);
            switch(orientation)
            {
                case 0:
                    positions.Add(global - Vector3.UnitX);
                    break;
                case 1:
                    positions.Add(global - Vector3.UnitY);
                    break;
                case 2:
                    positions.Add(global + Vector3.UnitX);
                    break;
                case 3:
                    positions.Add(global + Vector3.UnitX);
                    break;
                default: break;
            }
            foreach(var pos in positions)
                if(map.GetBlock(pos).Type != Types.Air)
                return false;
            return true;
        }

        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            Part part;
            int ori;
            GetState(cell, out part, out ori);
            var token = this.Parts[(int)part][ori];
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, token, zoom, fog, Color.White, sunlight, blocklight, depth);
        }
        public override void Draw(Vector3 blockcoords, Camera cam, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, Cell cell)
        {
            Part part;
            int ori;
            GetState(cell, out part, out ori);
            var token = this.Parts[(int)part][ori];
            cam.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, token, cam.Zoom, fog, Color.White, sunlight, blocklight, depth, blockcoords);
        }
        public override MyVertex[] Draw(Chunk chunk, Vector3 blockcoords, Camera cam, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, Cell cell)
        {
            Part part;
            int ori;
            GetState(cell, out part, out ori);
            var token = this.Parts[(int)part][ori];
            return chunk.VertexBuffer.DrawBlock(Block.Atlas.Texture, screenBounds, token, cam.Zoom, fog, Color.White, sunlight, blocklight, depth, blockcoords);
        }
        public override MyVertex[] Draw(Chunk chunk, Vector3 blockcoords, Camera cam, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            Part part;
            int ori;
            GetState(data, out part, out ori);
            var token = this.Parts[(int)part][(ori + (int)cam.Rotation) % 4];
            return chunk.VertexBuffer.DrawBlock(Block.Atlas.Texture, screenBounds, token, cam.Zoom, fog, Color.White, sunlight, blocklight, depth, blockcoords);
        }
        public override MyVertex[] Draw(MySpriteBatch mesh, MySpriteBatch nonopaquemesh, MySpriteBatch transparentMesh, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            Part part;
            int ori;
            GetState(data, out part, out ori);
            var token = this.Parts[(int)part][(ori + (int)camera.Rotation) % 4];
            return nonopaquemesh.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, Color.White, sunlight, blocklight, depth, blockCoordinates);
        }
        public override void DrawPreview(MySpriteBatch sb, IMap map, Vector3 global, Camera cam, byte data, int orientation = 0)
        {
            var bottom = global;
            var top = global - Vector3.UnitX;
            var bottomSecIndex = (int)cam.Rotation;
            var topSrcIndex = (int)cam.Rotation;

            //var bottomSrc = this.BottomParts[0];
            //var topSrc = this.TopParts[0];

            switch (orientation)
            {
                case 1:
                    top = global - Vector3.UnitY;
                    bottomSecIndex += 1; //= this.BottomParts[1];
                    topSrcIndex += 1; //= this.TopParts[1];
                    break;

                case 2:
                    top = global + Vector3.UnitX;
                    bottomSecIndex += 2; //= this.BottomParts[2];
                    topSrcIndex += 2; //= this.TopParts[2];
                    break;

                case 3:
                    top = global + Vector3.UnitY;
                    bottomSecIndex += 3; //= this.BottomParts[3];
                    topSrcIndex += 3; //= this.TopParts[3];
                    break;

                default: break;
            }
            bottomSecIndex = bottomSecIndex % 4;
            topSrcIndex = topSrcIndex % 4;
            //var bottomSrc = this.BottomParts[bottomSecIndex];
            //var topSrc = this.TopParts[topSrcIndex];
            var bottomSrc = this.Parts[0][bottomSecIndex];
            var topSrc = this.Parts[1][topSrcIndex];

            //sb.DrawBlock(Block.Atlas.Texture, bottom, this.Variations[0], cam.Zoom, Color.Transparent, Color.White, Color.White, Vector4.One);
            //sb.DrawBlock(Block.Atlas.Texture, top, this.Variations[1], cam.Zoom, Color.Transparent, Color.White, Color.White, Vector4.One);

            var topd = top.GetDrawDepth(map, cam);
            var bottomd = bottom.GetDrawDepth(map, cam);
            if (topd > bottomd)
            {
                sb.DrawBlock(Block.Atlas.Texture, map, top, topSrc, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
                sb.DrawBlock(Block.Atlas.Texture, map, bottom, bottomSrc, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
            }
            else
            {
                sb.DrawBlock(Block.Atlas.Texture, map, bottom, bottomSrc, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
                sb.DrawBlock(Block.Atlas.Texture, map, top, topSrc, cam, Color.Transparent, Color.White * 0.5f, Color.White, Vector4.One);
            }
        }

        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            list.Add(PlayerInput.RButton, new InteractionSleep());
        }

        class InteractionSleep : Interaction
        {
            public InteractionSleep():base("Sleep", 1)
            {

            }

            static readonly TaskConditions conds = new TaskConditions(new AllCheck(new RangeCheck()));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }

            public override object Clone()
            {
                return new InteractionSleep();
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                a.Map.Time = new TimeSpan(a.Map.Time.Days, 8, a.Map.Time.Minutes, a.Map.Time.Seconds);
                foreach (var ch in a.Map.GetActiveChunks())
                    ch.Value.LightCache.Clear();
            }
        }
    }
}
