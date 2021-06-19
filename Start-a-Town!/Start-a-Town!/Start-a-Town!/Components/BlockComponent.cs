using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Components
{
    /// <summary>
    /// Gives the object the behaviour of a tile block.
    /// </summary>
    public class BlockComponent : Component// PhysicsComponent
    {
        public override string ComponentName
        {
            get
            {
                return "Block";
            }
        }

        public float Transparency { get { return (float)this["Transparency"]; } set { this["Transparency"] = value; } }
        public float Density { get { return (float)this["Density"]; } set { this["Density"] = value; } }
        public bool HasData { get { return (bool)this["HasData"]; } set { this["HasData"] = value; } }
        public bool Opaque { get { return (bool)this["Opaque"]; } set { this["Opaque"] = value; } }
        public Block.Types Type { get { return (Block.Types)this["Type"]; } set { this["Type"] = value; } }
        public GameObject Entity { get { return (GameObject)this["Entity"]; } set { this["Entity"] = value; } }
        public byte Data { get { return (byte)this["Data"]; }
            set { this["Data"] = value; } }
        public bool Solid { get { return (bool)this["Solid"]; } set { this["Solid"] = value; } }
        public Block Block { get { return (Block)this["Block"]; } set { this["Block"] = value; } }
        public BlockEntity BlockEntity { get { return (BlockEntity)this["BlockEntity"]; } set { this["BlockEntity"] = value; } }
        //public IBlockState State { get { return (IBlockState)this["State"]; } set { this["State"] = value; } }

        static Dictionary<Block.Types, BlockComponent> _Blocks;
        public static Dictionary<Block.Types, BlockComponent> Blocks
        {
            get
            {
                if (_Blocks == null)
                    _Blocks = new Dictionary<Block.Types, BlockComponent>();
                return _Blocks;
            }
        }

        public BlockComponent()
        {
            this.Type = Block.Types.Empty;
            Properties.Add("Variation", -1);
            Properties.Add("Orientation", -1);
            Properties["Height"] = 1.0f;
            Properties[Stat.Density.Name] = 1f;
            Properties["Size"] = -1;
            this["Transparency"] = 0f;
            this.Density = 1f;
            this["HasData"] = false;
            this.Opaque = true;
            this.Data = 0;
            this.BlockEntity = null;
        }

        public override void MakeChildOf(GameObject parent)
        {
            this.Entity = parent;
        }
        public BlockComponent Initialize(Block block)
        {
            this.Block = block;
            this.Data = 0;// new Block.DefaultState();
            return this;
        }
        public BlockComponent Initialize(Block block, IBlockState state)
        {
            this.Block = block;
            byte data = 0;
            state.Apply(ref data);
            this.Data = data;
            return this;
        }
        public BlockComponent Initialize(Block block, byte data)
        {
            this.Block = block;
            this.Data = data;
            return this;
        }
        public BlockComponent Initialize(Block.Types type, bool hasData = false, float transparency = 0f, float density = 1f, bool opaque = true, bool solid = true)
        {
            this.Type = type;
            this.HasData = hasData;
            this.Transparency = transparency;
            this.Density = density;
            this.Opaque = opaque;
            this.Solid = solid;
            //Blocks.Add(type, this);
            Blocks[type] = this;
            return this;
        }


        //public override void Spawn(Net.IObjectProvider net, GameObject parent)
        //{
        //    throw new Exception();
        //    parent.Global.TrySetCell(net, this.Type);
        //    net.DisposeObject(parent);
        //}

        //public override void Despawn(IObjectProvider net, GameObject parent)
        //{
        //    throw new Exception();
        //    parent.Global.TrySetCell(net, Block.Types.Air); // AUTO GAMIETAI
        //}

        //public override bool Activate(GameObject actor, Objects.StaticObject self)
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)// GameObject sender, Message.Types msg)
        {
            if (this.Type == Block.Types.Empty)
                return true;
            Block.Registry[this.Type].OnMessage(parent, e);
            return true;

            Vector3 face, final;
            GameObjectSlot objSlot;
            Message.Types msg = e.Type;
            GameObject sender = e.Sender;
            switch (msg)
            {
                case Message.Types.Structure:
                    throw new NotImplementedException();
                    //GameObject.PostMessage(e.Sender, Message.Types.UIConstruction, parent, (Vector3)e.Parameters[0]);
                    return true;
                    face = (Vector3)e.Parameters[0];
                    GameObject product = e.Parameters[1] as GameObject;

                    GameObject.Types type = product.ID;//(GameObject.Types)e.Parameters[1];

                    // TODO: maybe find a less stupid way to get the blueprint
                    Blueprint bp = WorkbenchComponent.Blueprints.Skip(1).ToList().Find(foo => (foo["Blueprint"]["Blueprint"] as Blueprint).ProductID == type)["Blueprint"]["Blueprint"] as Blueprint; // e.Parameters[1] as Blueprint;
                    //Blueprint bp = product["Blueprint"]["Blueprint"] as Blueprint;
                   final = parent.Global;// + face;



                    GameObject obj = GameObject.Create(GameObject.Types.Construction);
                    //GameObject obj = product.Type == ObjectType.Tile ? GameObjectDb.ConstructionBlock : GameObjectDb.Construction;

                    Position pos = parent.Transform.Position;
                    Chunk.AddObject(obj, pos.Map,final);

                    throw new NotImplementedException();
                    //obj.PostMessage(Message.Types.SetBlueprint, e.Sender, bp, (int)product["Sprite"]["Variation"], (int)product["Sprite"]["Orientation"]);
                    //obj.Initialize().Spawn();
                    //obj.PostMessage(Message.Types.Activate, e.Sender);
                    return true;


                case Message.Types.Give:
                    Give(parent, e.Sender, (Vector3)e.Parameters[1], e.Parameters[0] as GameObjectSlot);
                    return true;

                case Message.Types.DropOn:
                    sender = e.Parameters[0] as GameObject;
                    face = (Vector3)e.Parameters[1];

                    objSlot = sender["Inventory"]["Holding"] as GameObjectSlot;
                    
                    GameObject newObj = objSlot.Take();
                    if (newObj.IsNull())
                        return true;
                   //newObj.Global = parent.Global + face;
                    //newObj.SetGlobal(parent.Global + face);
                    e.Network.Spawn(newObj, parent.Global + face);
                    //e.Network.PostLocalEvent(sender, ObjectEventArgs.Create(Message.Types.Dropped));
                    e.Success();

                    //Chunk.AddObject(newObj, parent.Map);
                    //throw new NotImplementedException();
                    //GameObject.PostMessage(e.Sender, Message.Types.Dropped, parent, objSlot, newObj);
                    return true;

                default:
                    return base.HandleMessage(parent, e);
            }
            return true;
        }

        public override void Query(GameObject parent, List<InteractionOld> actions)//GameObjectEventArgs e)
        {
            //List<Interaction> actions = e.Parameters[0] as List<Interaction>;
            // create a temp object above the tile for the actor to move on
            GameObject temp = new GameObject();
            //Vector3 face = (Vector3)e.Parameters[1];
            //temp.Global = parent.Global + face;// Vector3.UnitZ; // TODO: add vector according to selected face 
          //  actions.Add(new Interaction(TimeSpan.Zero, Message.Types.Structure, parent, "Structure"));
            actions.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Move, temp, "Move", range: (r1, r2) => true));

         //   actions.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.Structure, parent, "Construct"));

            actions.Add(new InteractionOld(TimeSpan.Zero, Message.Types.DropOn, parent, "Drop", 
                cond:
                new ConditionCollection(
                    new Condition(
                        (actor, target)=>InventoryComponent.IsHauling(actor, obj=>true), "Not carrying anything"
                ))));
            actions.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Activate, parent, "Build"));
        }


        public bool Break(Net.IObjectProvider net, GameObject obj)
        {
            return obj.Global.TrySetCell(net, Block.Types.Air, 0, 0);
        }

        public virtual void RandomUpdate(Net.IObjectProvider net, Vector3 global) { }

        public override bool Drop(GameObject self, GameObject actor, GameObject obj)
        {
            Position pos = self.Transform.Position;
            Position above = new Position(pos.Map, pos.Global + new Vector3(0, 0, 1));
            if (!above.Exists)
                return false;
            if (above.GetCell().Block.Type == Block.Types.Air)
                return true;
            return false;
        }
        public bool Drop(GameObject self, GameObject actor, GameObject obj, Vector3 face)
        {
            Position pos = self.Transform.Position;
            Position above = new Position(pos.Map, pos.Global + face);
            if (!above.Exists)
                return false;
            if (above.GetCell().Block.Type == Block.Types.Air)
                return true;
            return false;
        }
        public bool Give(GameObject parent, GameObject giver, Vector3 face, GameObjectSlot objSlot)
        {
            if (!objSlot.HasValue)
                return false;
            GameObject obj = objSlot.Object;// GameObject.Create(objSlot.Object.ID);// objSlot.Object;
            if (obj == null)
                return false;
            //objSlot.StackSize -= 1;
            if (Drop(parent, giver, obj, face))
            {
                Vector3 global = parent.Global + face;// new Vector3(0, 0, 1);
                if (objSlot.StackSize > 1)
                    obj = GameObject.Create(obj.ID);
                Chunk.AddObject(obj, parent.Map, global);
                objSlot.StackSize -= 1;
                return true;
            }
            return false;
        }

        public BlockComponent Register()
        {
            return Blocks[this.Type] = this;
        }

        public void Place(GameObject parent, GameObject actor, Vector3 location)
        {
            var orientation = 0;
            actor.Net.SyncSetBlock(location, this.Block.Type, this.Data, orientation);
        }
        internal override void GetAvailableTasks(GameObject parent, List<Interactions.Interaction> list)
        {
            list.Add(new PlaceBlock());
            //list.Add(new Scripts.Work(
            //    "Place",
            //    2,
            //    (a, t) => this.Place(parent, a, t.Global)
            //    ));
        }
        public override void GetHauledActions(GameObject parent, TargetArgs target, List<Interaction> actions)
        {
            actions.Add(new PlaceBlock());
            //actions.Add(new Scripts.Work(
            //    "Place",
            //    2,
            //    (a, t) => this.Place(parent, a, t.Global),
            //    new TaskConditions(
            //        new RangeCheck(t=>t.Global, max: Interaction.DefaultRange))
            //    ));
        }
        
        //public Action<Map, Vector3, Message.Types> OnMessageReceived = (m, g, msg) => { };


        public virtual bool IsSolid(Map map, Vector3 global)//IObjectProvider net, Vector3 global)
        {
            // for debugging
            if (global.Z < 0)
            {
                ("WARNING! Possible invalid object position " + global.ToString()).ToConsole();
                return true;
            }
            //g.Z = m.World.MaxHeight - 1;
            Cell cell = global.GetCell(map);
            //return Block.Registry[cell.Type].Solid;
            return cell.Block.Solid;
        }


        public override object Clone()
        {
            BlockComponent comp = new BlockComponent();//.Initialize(this.Type, this.HasData, this.Transparency, this.Density, this.Opaque, this.Solid);
            foreach (KeyValuePair<string, object> property in Properties)
            {
                comp.Properties[property.Key] = property.Value;
            }

            return comp;
        }

        //public override void GetTooltip(GameObject parent, UI.Control tooltip)
        //{
        //    tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, Block.Registry[this.Type].Type.ToString()));
        //}
        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            //parent.Global.GetBlock(parent.Map).GetTooltip(tooltip);
            //this.Block.GetTooltip(tooltip);
            this.Block.GetTooltip(tooltip, parent.Map, parent.Global);
            if (this.BlockEntity != null)
                this.BlockEntity.GetTooltip(tooltip);
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.Data);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Data = r.ReadByte();
        }
        internal override List<SaveTag> Save()
        {
            return new List<SaveTag>()
            {
                new SaveTag(SaveTag.Types.Byte, "Data", this.Data)
            };
        }
        internal override void Load(SaveTag save)
        {
            this.Data = save.TagValueOrDefault<byte>("Data", 0);
        }
    }
}
