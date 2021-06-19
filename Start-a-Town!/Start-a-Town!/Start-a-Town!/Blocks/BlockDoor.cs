using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class BlockDoor : Block
    {
        class State : IBlockState
        {
            public bool Open { get; set; }
            public int Part { get; set; }
            static public void Get(byte data, out bool open, out int part)
            {
                open = (data & 0x4) == 0x4;// != 0x4;
                part = data & 0x3;
            }
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
            public void FromMaterial(GameObject material) { }
            public Color GetTint(byte d)
            { return Color.White; }
            public string GetName(byte d)
            {
                return "Part:" + this.Part.ToString() + ":" + (this.Open ? "Closed" : "Open");
            }
        }
        State GetState(byte data)
        {
            return new State(data);
        }

        [Flags]
        enum States {Open = 0x0, Closed = 0x1};

      //  List<Vector3> Children = new List<Vector3>();
        List<Vector3> GetChildren(GameObject parent)
        {
            return new List<Vector3>() { parent.Global, 
                        parent.Global + new Vector3(0, 0, 1), parent.Global + new Vector3(0, 0, 2) };
        }

        void SetState(Map map, Vector3 bottom, bool closed)
        {
            throw new NotImplementedException();
            for (int i = 0; i < 3; i++)
            {
                Vector3 g = bottom + Vector3.UnitZ * i;
                Cell cell = g.GetCell(map);

            }
        }

        //public override void Placed(Net.IObjectProvider net, Vector3 global)
        //{
        //    for (int i = 0; i < 3; i++)
        //    {
        //        Vector3 g = global + new Vector3(0, 0, i);
        //        byte data = (byte)i;
        //        //g.TrySetCell(net, Block.Types.Door, data);
        //        net.Map.SetBlock(g, Block.Types.Door, data);
        //    }
        //}
        //public override void Removed(Net.IObjectProvider net, Vector3 global)
        //{
        //    foreach (var g in GetChildren(net, global))
        //        //g.TrySetCell(net, Block.Types.Air, 0);
        //        net.Map.SetBlock(g, Block.Types.Air, 0);
        //}
        public override void Place(IMap map, Vector3 global, byte data, int variation, int orientation)
        {
            for (int i = 0; i < 3; i++)
            {
                Vector3 g = global + new Vector3(0, 0, i);
                byte _data = (byte)i;
                map.SetBlock(g, Block.Types.Door, _data);
            }


            // DETECT HOUSE
            // find which side is enterior by checking heightmap
            var back = global - Vector3.UnitY;
            var front = global + Vector3.UnitY;
            var backHeightMap = map.GetHeightmapValue(back);
            var frontHeightMap = map.GetHeightmapValue(front);
            var backIsInside = back.Z < backHeightMap;
            var frontIsInside = front.Z < frontHeightMap;
            if(backIsInside)
            {
                //flood fill to find all enterior

            }
        }
        public override void Remove(IMap map, Vector3 global)
        {
            foreach (var g in GetChildren(map, global))
                map.SetBlock(g, Block.Types.Air, 0);
        }
        private static Vector3 GetBase(IMap map, Vector3 global)
        {
            //byte data = global.GetData(net.Map);
            byte data = map.GetData(global);

            //Vector3 doorBase = parent.Global - Vector3.UnitZ * data;
            byte masked = data &= 0x3;
            int baseZ = (int)(global.Z - masked);
            Vector3 baseLoc = new Vector3(global.X, global.Y, baseZ);
            return baseLoc;
        }
        public static List<Vector3> GetChildren(IMap map, Vector3 global)
        {
            List<Vector3> list = new List<Vector3>();
            Vector3 baseLoc = GetBase(map, global);
            for (int i = 0; i < 3; i++)
            {
                Vector3 g = baseLoc + new Vector3(0, 0, i);
                list.Add(g);
            }
            return list;
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
            int part;
            bool open;
            State.Get(cell.BlockData, out open, out part);

            return !open;
        }
        public override bool IsSolid(Cell cell, Vector3 withinBlock)
        {
            return this.IsSolid(cell);
        }

        public override Material GetMaterial(byte blockdata)
        {
            return Material.LightWood;
        }
        public BlockDoor()
            : base(Block.Types.Door, GameObject.Types.Door, 0, 1, false, true)
        {
            //this.Reagents.Add(GameObject.Types.WoodenPlank);
            this.Reagents.Add(new Reaction.Reagent("Base", Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks), Reaction.Reagent.IsOfMaterial(Material.LightWood)));
            //this.Material = Material.LightWood;
            //this.Variations.AddRange(Block.Atlas.Load("blocks/doors", "blocks/doorw", "blocks/doorn", "blocks/doore"));
            this.AssetNames = "doors/doors, doors/doore, doors/doorn, doors/doorw";

            this.Recipe = new BlockConstruction(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Wood), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockConstruction.Product(this)
                );
        }

        public override List<byte> GetVariations()
        {
            return new List<byte>() { 0 };
        }

        protected override void HandleMessage(Vector3 global, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Activate:
                    BlockDoor.State state = new BlockDoor.State(e.Network, global);
                    state.Open = !state.Open;
                    state.Apply(e.Network.Map, global);
                    return;

                default:
                    break;
            }
            return;
        }


        public override void GetTooltip(UI.Control tooltip, IMap map, Vector3 global)
        {
            base.GetTooltip(tooltip, map, global);
            //var state = new State(map.GetNetwork(), global);
            var cell =map.GetCell(global);
            var data = cell.BlockData;// map.GetData(global); //
            var open = (data & 0x4) == 0x4;

            Cell cell2 = map.GetCell(global);
            bool lastOpen = (cell.BlockData & 0x4) == 0x4;

            tooltip.Controls.Add(new Label(open ? "Open" : "Closed" ) { Location = tooltip.Controls.BottomLeft });
        }


        private static void Toggle(IMap map, Vector3 global)
        {
            var children = GetChildren(map, global);
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
            }
        }

        public override void GetPlayerActionsWorld(GameObject player, Vector3 global, Dictionary<PlayerInput, Interaction> list)
        {
            list.Add(new PlayerInput(PlayerActions.Activate), new InteractionToggleDoor());
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
            bool closed;
            int part;
            State.Get(cell.BlockData, out closed, out part);
            int ori = (cell.Orientation + (closed ? 1 : 0)) % 4; // FASTER???
            sb.DrawBlock(Block.Atlas.Texture, screenPos, this.Variations[ori].Rectangle, zoom, Color.White, sunlight, blocklight, depth);
        }
        public override void Draw(MySpriteBatch sb, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            bool open;
            int part;
            State.Get(cell.BlockData, out open, out part);
            int ori = (cell.Orientation + (open ? 1 : 0)) % 4; // FASTER???
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[ori], zoom, fog, Color.White, sunlight, blocklight, depth);
        }
        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float zoom, float depth, Cell cell)
        {
            bool open;
            int part;
            State.Get(cell.BlockData, out open, out part);
            int ori = (cell.Orientation + (open ? 1 : 0)) % 4; // FASTER???
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[ori], zoom, fog, Color.White, sunlight, blocklight, depth);
            //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[cell.Orientation], zoom, Color.White, sunlight, blocklight, depth);
        }
        public override void Draw(MySpriteBatch sb, Rectangle screenBounds, Color sunlight, Vector4 blocklight, float zoom, float depth, Cell cell)
        {
            bool closed;
            int part;
            State.Get(cell.BlockData, out closed, out part);
            //int ori = (cell.Orientation + (closed ? 1 : 0)) % 4; // FASTER???
            //sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[ori], zoom, Color.White, sunlight, blocklight, depth);
            sb.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[cell.Orientation], zoom, Color.White, sunlight, blocklight, depth);

        }

        public override MyVertex[] Draw(Vector3 blockcoords, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            bool open;
            int part;
            State.Get(data, out open, out part);
            int ori = (orientation + (open ? 1 : 0)) % 4; // FASTER???
            return camera.SpriteBatch.DrawBlock(Block.Atlas.Texture, screenBounds, this.Variations[ori], camera.Zoom, fog, Color.White, sunlight, blocklight, depth, blockcoords);
        }

        class InteractionToggleDoor : Interaction
        {
            public InteractionToggleDoor():base("Open/close")
            {
                this.Name = "Open/close";
                this.Seconds = 0;
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
                Toggle(a.Map, t.Global);
            }

            public override object Clone()
            {
                return new InteractionToggleDoor();
            }
        }
    }
}
