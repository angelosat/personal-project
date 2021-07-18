using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    public class BlockDoor : Block
    {
        public class State : IBlockState
        {
            public bool Open { get; set; }
            public int Part { get; set; }

            public State(bool open, int part)
            {
                this.Open = open;
                this.Part = part;
            }
            public State(INetwork net, Vector3 global)
            {
                Cell cell = net.Map.GetCell(global);

                this.Open = (cell.BlockData & 0x4) == 0x4;
                this.Part = cell.BlockData & 0x3;
            }
            public State(byte data)
            {
                this.Open = (data & 0x4) == 0x4;
                this.Part = data & 0x3;
            }
            public void Apply(MapBase map, Vector3 global)
            {
                Cell cell = map.GetCell(global);

                if (cell.Block.Type != Types.Door)
                    throw new Exception("Block type mismatch");

                int baseZ = (int)global.Z - this.Part;
                for (int i = 0; i < 3; i++)
                {
                    var g = new Vector3(global.X, global.Y, baseZ + i);
                    cell = map.GetCell(g);

                    cell.BlockData = (byte)i;

                    if (this.Open)
                        cell.BlockData |= 0x4;
                    else
                        cell.BlockData = (byte)(cell.BlockData ^= 0x4);
                }
            }
            public void Apply(ref byte data)
            {
                data = (byte)this.Part;

                if (this.Open)
                    data |= 0x4;
                else
                    data = (byte)(data & ~0x4);
            }
            public void Apply(Block.Data data)
            {
                data.Value = (byte)this.Part;

                if (this.Open)
                    data.Value |= 0x4;
                else
                    data.Value = (byte)(data.Value & ~0x4);
            }
            public void FromCraftingReagent(GameObject material) { }
            public Color GetTint(byte d)
            { return Color.White; }
            public string GetName(byte d)
            {
                return "Part:" + this.Part.ToString() + ":" + (this.Open ? "Closed" : "Open");
            }
        }
        static public State GetState(byte data)
        {
            return new State(data);
        }
        static public BoundingBox GetBoundingBox(MapBase map, Vector3 global)
        {
            var children = BlockDefOf.Door.GetParts(map, global);
            var minx = children.Min(c => c.X);
            var miny = children.Min(c => c.Y);
            var minz = children.Min(c => c.Z);
            var maxx = children.Max(c => c.X);
            var maxy = children.Max(c => c.Y);
            var maxz = children.Max(c => c.Z);
            var min = new Vector3(minx, miny, minz);
            var max = new Vector3(maxx, maxy, maxz);
            var box = new BoundingBox(min - new Vector3(.5f, .5f, 0), max + new Vector3(.5f, .5f, 0));
            return box;
        }
        static public void Read(byte data, out bool locked, out bool open, out int part)
        {
            locked = IsLocked(data);
            open = IsOpen(data);
            part = GetPart(data);
        }
        static public byte WriteOpen(byte data, bool open)
        {
            if (open)
                data |= 0x4;
            else
                data ^= 0x4;
            return data;
        }
        static public byte WritePart(byte data, int part)
        {
            data &= (byte)part;
            return data;
        }
        static public byte WriteLocked(byte data, bool locked)
        {
            if (locked)
                data |= 0x8;
            else
                data ^= 0x8;
            return data;
        }
        static public bool IsLocked(byte data)
        {
            return (data & 0x8) == 0x8;
        }
        static public bool IsOpen(byte data)
        {
            return (data & 0x4) == 0x4;
        }
        /// <summary>
        /// returns z-distance from bottom block
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        static public int GetPart(byte data)
        {
            return data & 0x3;
        }
        static public byte GetData(int verticalPos)
        {
            return (byte)verticalPos;
        }
        public override bool IsRoomBorder => true;

        public override Dictionary<IntVec3, byte> GetParts(IntVec3 global, int orientation)
        {
            var dic = new Dictionary<IntVec3, byte>();
            for (int i = 0; i < 3; i++)
                dic.Add(global + new IntVec3(0, 0, i), GetData(i));
            return dic;
        }

        [Flags]
        enum States { Open = 0x0, Closed = 0x1 };

        static AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];

        public override bool IsOpaque(Cell cell)
        {
            return !IsOpen(cell.BlockData);
        }
        internal override bool IsPathable(Cell cell, IntVec3 blockCoords)
        {
            return !IsLocked(cell.BlockData);
        }
        public override bool Multi => true;
        public override void Place(MapBase map, IntVec3 global, byte data, int variation, int orientation, bool notify = true)
        {
            var positions = new IntVec3[2];
            for (int i = 0; i < 2; i++)
            {
                var g = global + new IntVec3(0, 0, i);
                positions[i] = g;
                byte _data = (byte)i;
                map.SetBlock(g, Block.Types.Door, _data, variation, orientation, false);
            }
            if (notify)
                map.NotifyBlocksChanged(positions);

            // DETECT HOUSE
            // find which side is enterior by checking heightmap
            var back = global - IntVec3.UnitY;
            var front = global + IntVec3.UnitY;
            var backHeightMap = map.GetHeightmapValue(back);
            var frontHeightMap = map.GetHeightmapValue(front);
            var backIsInside = back.Z < backHeightMap;
            var frontIsInside = front.Z < frontHeightMap;
            if (backIsInside)
            {
                //flood fill to find all enterior
            }
        }
        
        private static IntVec3 GetCenter(byte data)
        {
            byte masked = data &= 0x1;
            int baseZ = -masked;
            var baseLoc = new IntVec3(0, 0, baseZ);
            return baseLoc;
        }

        public override IEnumerable<IntVec3> GetParts(byte data)
        {
            var center = GetCenter(data);
            for (int i = 0; i < 2; i++)
                yield return center + new IntVec3(0, 0, i);
        }
        public override bool IsSolid(Cell cell)
        {
            Read(cell.BlockData, out var locked, out var open, out var part);
            return !open;
        }
        public override bool IsSolid(Cell cell, Vector3 withinBlock)
        {
            return this.IsSolid(cell);
        }
        public override float GetDensity(byte data, Vector3 global)
        {
            Read(data, out var locked, out var open, out var part);
            return open ? 0 : 1;
        }
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.LightWood;
        }

        public BlockDoor()
            : base(Block.Types.Door, 0, 1, false, true)
        {
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();

            var ndepth = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/doors/doorndepth");
            var wdepth = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/doors/doorwdepth");

            Orientations[0] = Block.Atlas.Load("blocks/doors/doors", MapBase.BlockDepthMap, Block.NormalMap);
            Orientations[1] = Block.Atlas.Load("blocks/doors/doorw", wdepth, Block.NormalMap);
            Orientations[2] = Block.Atlas.Load("blocks/doors/doorn", ndepth, Block.NormalMap);
            Orientations[3] = Block.Atlas.Load("blocks/doors/doore", MapBase.BlockDepthMap, Block.NormalMap);

            this.ToggleConstructionCategory(ConstructionsManager.Doors, true);
        }
        public override Icon GetIcon()
        {
            return new Icon(this.GetDefault());
        }
        public override IEnumerable<byte> GetMaterialVariations()
        {
            yield return 0;
        }

        public override void GetTooltip(UI.Control tooltip, MapBase map, Vector3 global)
        {
            base.GetTooltip(tooltip, map, global);
            var cell = map.GetCell(global);
            var data = cell.BlockData;// map.GetData(global); //
            var open = (data & 0x4) == 0x4;

            Cell cell2 = map.GetCell(global);
            bool lastOpen = (cell.BlockData & 0x4) == 0x4;
            var locked = IsLocked(cell.BlockData);
            tooltip.Controls.Add(new Label(open ? "Open" : "Closed") { Location = tooltip.Controls.BottomLeft });
            tooltip.Controls.Add(new Label(locked ? "Locked" : "Unlocked") { Location = tooltip.Controls.BottomLeft });
        }

        public override AtlasDepthNormals.Node.Token GetDefault()
        {
            return Orientations[0];
        }
        public override AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return Orientations[orientation];
        }

        public static void Toggle(MapBase map, Vector3 global)
        {
            var children = BlockDefOf.Door.GetParts(map, global);
            var chunk = map.GetChunk(global);
            foreach (var g in children)
            {
                Cell cell = map.GetCell(g);
                if (map.GetBlock(global).Type != Types.Door)
                    throw new Exception();
                bool lastOpen = (cell.BlockData & 0x4) == 0x4;
                var open = !lastOpen;
                if (open)
                    cell.BlockData |= 0x4;
                else
                    cell.BlockData ^= 0x4;
                chunk.InvalidateSlice(g.Z);
            }
        }
        
        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            bool locked;
            int part;
            bool open;
            Read(cell.BlockData, out locked, out open, out part);
            int ori = (cell.Orientation + (open ? 1 : 0)) % 4; // FASTER???
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[ori], zoom, fog, Color.White, sunlight, blocklight, depth, this);
        }
        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            bool locked;
            int part;
            bool open;
            Read(cell.BlockData, out locked, out open, out part);
            int ori = (cell.Orientation + (open ? 1 : 0)) % 4; // FASTER???
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[ori], zoom, fog, Color.White, sunlight, blocklight, depth);
        }
        
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            bool locked;
            int part;
            bool open;
            Read(data, out locked, out open, out part);
            int ori = (orientation - (int)camera.Rotation + (open ? 1 : 0)); // FASTER???
            if (ori < 0)
                ori += 4;
            else
                ori %= 4;
            return canvas.Opaque.DrawBlock(Block.Atlas.Texture, screenBounds, Orientations[ori], camera.Zoom, fog, Color.White, sunlight, blocklight, depth, this, blockCoordinates);
        }
        public override void DrawPreview(MySpriteBatch sb, MapBase map, Vector3 global, Camera cam, Color tint, byte data, int variation = 0, int orientation = 0)
        {
            var token = Orientations[orientation];
            sb.DrawBlock(Block.Atlas.Texture, map, global, token, cam, Color.Transparent, tint, Color.White, Vector4.One);
            sb.DrawBlock(Block.Atlas.Texture, map, global + Vector3.UnitZ, token, cam, Color.Transparent, tint, Color.White, Vector4.One);
            sb.DrawBlock(Block.Atlas.Texture, map, global + Vector3.UnitZ, token, cam, Color.Transparent, tint, Color.White, Vector4.One);
        }
    }
}
