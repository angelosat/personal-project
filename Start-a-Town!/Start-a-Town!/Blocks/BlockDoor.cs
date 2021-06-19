using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Blocks;

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
            public State(IObjectProvider net, Vector3 global)
            {
                //Cell cell = global.GetCell(net.Map);
                Cell cell = net.Map.GetCell(global);

                this.Open = (cell.BlockData & 0x4) == 0x4;
                this.Part = cell.BlockData & 0x3;
            }
            public State(byte data)
            {
                this.Open = (data & 0x4) == 0x4;//!= 0x4;
                this.Part = data & 0x3;
            }
            public void Apply(IMap map, Vector3 global)
            {
                //Cell cell = global.GetCell(map);
                Cell cell = map.GetCell(global);

                if (cell.Block.Type != Types.Door)
                    throw new Exception("Block type mismatch");

                int baseZ = (int)global.Z - this.Part;
                //   int ori = cell.Orientation + (this.Closed ? 1 : -1);
                for (int i = 0; i < 3; i++)
                {
                    Vector3 g = new Vector3(global.X, global.Y, baseZ + i);// global + Vector3.UnitZ * i;
                    //cell = g.GetCell(map);
                    cell = map.GetCell(g);

                    //    cell.Orientation = ori;// += this.Closed ? 1 : -1;
                    cell.BlockData = (byte)i;

                    if (this.Open)
                        cell.BlockData |= 0x4;
                    else
                        //cell.BlockData &= ~(2 << 3);
                        //cell.BlockData = (byte)(cell.BlockData & ~0x4);
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
        static public BoundingBox GetBoundingBox(IMap map, Vector3 global)
        {
            var children = GetChildren(map, global);
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
            locked = IsLocked(data);// (data & 0x8) == 0x8;
            open = IsOpen(data);// (data & 0x4) == 0x4;// != 0x4;
            part = GetPart(data);// data & 0x3;
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

        public override Dictionary<Vector3, byte> GetParts(Vector3 global, int orientation)
        {
            var dic = new Dictionary<Vector3, byte>();
            for (int i = 0; i < 3; i++)
                dic.Add(global + new Vector3(0, 0, i), GetData(i));
            return dic;
        }

        [Flags]
        enum States { Open = 0x0, Closed = 0x1 };

        static AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];

        List<Vector3> GetChildren(GameObject parent)
        {
            return new List<Vector3>() { parent.Global,
                        parent.Global + new Vector3(0, 0, 1), parent.Global + new Vector3(0, 0, 2) };
        }

        void SetState(Map map, Vector3 bottom, bool closed)
        {
            throw new NotImplementedException();
            //for (int i = 0; i < 3; i++)
            //{
            //    Vector3 g = bottom + Vector3.UnitZ * i;
            //    Cell cell = g.GetCell(map);

            //}
        }
        public override bool IsOpaque(Cell cell)
        {
            return !IsOpen(cell.BlockData);
        }
        internal override bool IsPathable(Cell cell, Vector3 blockCoords)
        {
            return !IsLocked(cell.BlockData);
        }
        public override bool Multi => true;
        public override void Place(IMap map, Vector3 global, byte data, int variation, int orientation, bool notify = true)
        {
            var positions = new Vector3[2];
            for (int i = 0; i < 2; i++)
            {
                Vector3 g = global + new Vector3(0, 0, i);
                positions[i] = g;
                byte _data = (byte)i;
                map.SetBlock(g, Block.Types.Door, _data, variation, orientation, false);
            }
            if (notify)
                map.NotifyBlocksChanged(positions);

            // DETECT HOUSE
            // find which side is enterior by checking heightmap
            var back = global - Vector3.UnitY;
            var front = global + Vector3.UnitY;
            var backHeightMap = map.GetHeightmapValue(back);
            var frontHeightMap = map.GetHeightmapValue(front);
            var backIsInside = back.Z < backHeightMap;
            var frontIsInside = front.Z < frontHeightMap;
            if (backIsInside)
            {
                //flood fill to find all enterior

            }
        }
        public override void Remove(IMap map, Vector3 global, bool notify = true)
        {
            var positions = GetChildren(map, global);
            foreach (var g in positions)
                map.SetBlock(g, Block.Types.Air, 0, 0, 0, false);// raiseEvent: notify);
            if (notify)
                map.NotifyBlocksChanged(positions);
        }
        private static Vector3 GetBase(IMap map, Vector3 global)
        {
            //byte data = global.GetData(net.Map);
            byte data = map.GetData(global);

            //Vector3 doorBase = parent.Global - Vector3.UnitZ * data;
            byte masked = data &= 0x1;// 0x3;
            int baseZ = (int)(global.Z - masked);
            Vector3 baseLoc = new Vector3(global.X, global.Y, baseZ);
            return baseLoc;
        }
        public static IEnumerable<Vector3> GetChildren(IMap map, Vector3 global)
        {
            //List<Vector3> list = new List<Vector3>();
            Vector3 baseLoc = GetBase(map, global);
            for (int i = 0; i < 2; i++)
            {
                Vector3 g = baseLoc + new Vector3(0, 0, i);
                //list.Add(g);
                yield return g;
            }
            //return list;
        }

        public override bool IsSolid(IMap map, Vector3 global)
        {
            //byte data = global.GetData(map);
            //return (data & 0x4) != 0x4;


            //State.Get(global.GetCell(map).BlockData, out closed, out part);

            var cell = map.GetCell(global);
            return this.IsSolid(cell);
        }
        public override bool IsSolid(Cell cell)
        {
            //return false;
            bool locked;
            int part;
            bool open;
            Read(cell.BlockData, out locked, out open, out part);

            return !open;
        }
        public override bool IsSolid(Cell cell, Vector3 withinBlock)
        {
            return this.IsSolid(cell);
        }
        public override float GetDensity(byte data, Vector3 global)
        {
            //return 0;
            bool locked;
            int part;
            bool open;
            Read(data, out locked, out open, out part);
            return open ? 0 : 1;
        }
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.LightWood;
        }

        public BlockDoor()
            : base(Block.Types.Door, GameObject.Types.Door, 0, 1, false, true)
        {
            this.Reagents.Add(new Reaction.Reagent("Base", Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks), Reaction.Reagent.IsOfMaterial(MaterialDefOf.LightWood)));
            //this.AssetNames = "doors/doors, doors/doore, doors/doorn, doors/doorw";
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();

            var ndepth = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/doors/doorndepth");
            var wdepth = Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/doors/doorwdepth");

            //Orientations[0] = Block.Atlas.Load("blocks/doors/doors", Map.BlockDepthMap, Block.NormalMap);
            //Orientations[1] = Block.Atlas.Load("blocks/doors/doore", Map.BlockDepthMap, Block.NormalMap);
            //Orientations[2] = Block.Atlas.Load("blocks/doors/doorn", ndepth, Block.NormalMap);
            //Orientations[3] = Block.Atlas.Load("blocks/doors/doorw", wdepth, Block.NormalMap);
            Orientations[0] = Block.Atlas.Load("blocks/doors/doors", Map.BlockDepthMap, Block.NormalMap);
            Orientations[1] = Block.Atlas.Load("blocks/doors/doorw", wdepth, Block.NormalMap);
            Orientations[2] = Block.Atlas.Load("blocks/doors/doorn", ndepth, Block.NormalMap);
            Orientations[3] = Block.Atlas.Load("blocks/doors/doore", Map.BlockDepthMap, Block.NormalMap);

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Wood), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockRecipe.Product(this)
                );
            Towns.Constructions.ConstructionsManager.Doors.Add(this.Recipe);
        }
        public override Icon GetIcon()
        {
            return new Icon(this.GetDefault());
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            yield return 0;
            //return new List<byte>() { 0 };
        }

        //protected override void HandleMessage(Vector3 global, ObjectEventArgs e)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.Activate:
        //            BlockDoor.State state = new BlockDoor.State(e.Network, global);
        //            state.Open = !state.Open;
        //            state.Apply(e.Network.Map, global);
        //            return;

        //        default:
        //            break;
        //    }
        //    return;
        //}

        public override void GetTooltip(UI.Control tooltip, IMap map, Vector3 global)
        {
            base.GetTooltip(tooltip, map, global);
            //var state = new State(map.GetNetwork(), global);
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

        public static void Toggle(IMap map, Vector3 global)
        {
            var children = GetChildren(map, global);
            var chunk = map.GetChunk(global);
            foreach (var g in children)
            {
                //Cell cell = g.GetCell(net.Map);
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
            //map.GetChunk(global).Valid = false;
        }
        private static void ToggleLock(IMap map, Vector3 global)
        {
            var children = GetChildren(map, global);
            foreach (var g in children)
            {
                Cell cell = map.GetCell(g);
                if (map.GetBlock(global).Type != Types.Door)
                    throw new Exception();
                bool locked;
                int part;
                bool open;
                Read(cell.BlockData, out locked, out open, out part);
                if (open)
                    return;
                cell.BlockData = WriteLocked(cell.BlockData, !locked);
            }
            map.EventOccured(Message.Types.DoorLockToggled, global);
        }
        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            if (!IsLocked(player.Map.GetData(global)))
                list.Add(PlayerInput.RButton, new InteractionToggleDoor());
            if (!IsOpen(player.Map.GetData(global)))
                list.Add(PlayerInput.Activate, new InteractionToggleDoorLock());
        }
        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>() { new InteractionToggleDoor() };
        }
        void GetState(GameObject parent, out bool open)
        {
            States data = (States)parent.Global.GetData(parent.Map);
            open = (data & States.Closed) == States.Closed ? false : true;
        }

        public override void Draw(MySpriteBatch sb, Vector2 screenPos, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            //base.Draw(sb, screenPos, light, zoom, depth, cell);
            //int ori = (cell.Orientation + (new State(cell.BlockData).Closed ? 1 : 0) )% 4; // SLOW???
            bool locked;
            int part;
            bool open;
            Read(cell.BlockData, out locked, out open, out part);
            int ori = (cell.Orientation + (open ? 1 : 0)) % 4; // FASTER???
            sb.DrawBlock(Block.Atlas.Texture, screenPos, this.Variations[ori].Rectangle, zoom, Color.White, sunlight, blocklight, depth);
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
            //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[cell.Orientation], zoom, Color.White, sunlight, blocklight, depth);
        }
        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            bool locked;
            int part;
            bool open;
            Read(cell.BlockData, out locked, out open, out part);
            //int ori = (cell.Orientation + (closed ? 1 : 0)) % 4; // FASTER???
            //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[ori], zoom, Color.White, sunlight, blocklight, depth);
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[cell.Orientation], zoom, Color.White, sunlight, blocklight, depth);

        }
        public override MyVertex[] Draw(Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            throw new NotImplementedException();
            bool locked;
            int part;
            bool open;
            Read(data, out locked, out open, out part);
            int ori = (orientation + (open ? 1 : 0)) % 4; // FASTER???
            return camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[ori], camera.Zoom, fog, Color.White, sunlight, blocklight, depth, this, blockcoords);
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            bool locked;
            int part;
            bool open;
            Read(data, out locked, out open, out part);
            //int ori = (orientation + (open ? 1 : 0)) % 4; // FASTER???
            int ori = (orientation - (int)camera.Rotation + (open ? 1 : 0)); // FASTER???
            if (ori < 0)
                ori += 4;
            else
                ori %= 4;
            return canvas.Opaque.DrawBlock(Block.Atlas.Texture, screenBounds, Orientations[ori], camera.Zoom, fog, Color.White, sunlight, blocklight, depth, this, blockCoordinates);
        }
        public override void DrawPreview(MySpriteBatch sb, IMap map, Vector3 global, Camera cam, Color tint, byte data, int variation = 0, int orientation = 0)
        {
            var token = Orientations[orientation];
            sb.DrawBlock(Block.Atlas.Texture, map, global, token, cam, Color.Transparent, tint, Color.White, Vector4.One);
            sb.DrawBlock(Block.Atlas.Texture, map, global + Vector3.UnitZ, token, cam, Color.Transparent, tint, Color.White, Vector4.One);
            sb.DrawBlock(Block.Atlas.Texture, map, global + Vector3.UnitZ, token, cam, Color.Transparent, tint, Color.White, Vector4.One);
        }

        public class InteractionToggleDoorLock : Interaction
        {
            public InteractionToggleDoorLock()
                : base("Lock/Unlock", .4f)
            {
                //this.Name = "Open/close";
                //this.Seconds = 0;
            }
            static readonly TaskConditions conds = new TaskConditions(new AllCheck(new RangeCheck()));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }

            public override void Perform(GameObject a, TargetArgs t)
            {
                base.Perform(a, t);
                //var state = new BlockDoor.State(a.Net, t.FinalGlobal);
                ToggleLock(a.Map, t.Global);
            }

            public override object Clone()
            {
                return new InteractionToggleDoorLock();
            }
        }
    }
}
