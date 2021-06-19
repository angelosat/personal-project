using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    public enum TargetType { Null, Entity, Slot, BlockEntitySlot, Position, Direction }

    /// <summary>
    /// A wrapper for a GameObject which includes the additional information of the object's face that has been targeted (in case of block objects).
    /// Also provides methods for serializing/deserializing.
    /// </summary>
    public class TargetArgs : ITooltippable, IContextable
    {
        public void GetTooltipInfo(UI.Tooltip tooltip)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.Object.GetTooltipInfo(tooltip);
                    break;

                case TargetType.Position:
                    //this.Global.GetBlock(Engine.Map).GetTooltipInfo(tooltip);
                    //Engine.Map.GetBlock(this.Global).GetTooltipInfo(tooltip);
                    Engine.Map.GetBlock(this.Global).GetTooltip(tooltip, Engine.Map, this.Global);
                    
                    break;

                default: break;
            }
            return;
        }
        public IMap Map;
        public IObjectProvider Network;
        public Vector2 Direction;
        public TargetType Type { get; protected set; }
        //public Vector3 Global { get; set; }
        Vector3 _Global;
        public Vector3 Global
        {
            set { this._Global = value; }
            get {
                if (this.Type == TargetType.Slot)
                    if (this.Slot.Object != null)
                        return this.Slot.Object.Global;
                if (this.Type == TargetType.Entity)
                    return this.Object.Transform.Global;
                return this._Global;
            }
        }
        GameObject _Object;
        public GameObject Object// { get; set; }
        {
            get
            {
                if (this.Type == TargetType.Entity)
                    return this._Object;
                else if (this.Type == TargetType.Slot || this.Type == TargetType.BlockEntitySlot)
                    return this.Slot.Object;
                else
                    return null;
            }
            set { this._Object = value; }
        }
        public Vector3 Face { get; set; }
        public Vector3 Precise { get; set; }
        public GameObjectSlot Slot { get; set; }
        public TargetArgs()
        {
            this.Face = Vector3.Zero;
            this.Precise = Vector3.Zero;
        }
        public TargetArgs(GameObject obj)
        {
            this.Type = TargetType.Entity;
            this.Object = obj;
            this.Global = obj.Global;
            this.Network = obj.Net;
        }
        public TargetArgs(GameObject obj, Vector3 face)
        {
            this.Type = TargetType.Entity;
            //this.Type = TargetType.Block;
            this.Object = obj;
            this.Global = obj.Global;
            this.Network = obj.Net;

        }
        public TargetArgs(GameObject obj, Vector3? face)
        {
            //this.Type = TargetType.Block;
            this.Type = TargetType.Entity;
            this.Object = obj;
            this.Global = obj.Global;
            this.Face = face.HasValue ? face.Value : Vector3.Zero;
            this.Network = obj.Net;

           // this.Precise = Vector3.Zero;
        }
        public TargetArgs(IMap map, Vector3 global)
        {
            this.Map = map;
            this.Type = TargetType.Position;
            this.Global = global;
        }
        public TargetArgs(Vector3 global)
        {
            this.Type = TargetType.Position;
            this.Global = global;
        }
        public TargetArgs(Vector3 global, Vector3 face)
        {
            this.Type = TargetType.Position;
            this.Global = global;
            this.Face = face;
        }
        public TargetArgs(Vector3 global, Vector3 face, Vector3 precise)
        {
            this.Type = TargetType.Position;
            this.Global = global;
            this.Face = face;
            this.Precise = precise;
        }
        public TargetArgs(GameObjectSlot slot)
        {
            this.Type = TargetType.Slot;
            this.Slot = slot;
        }
        public TargetArgs(Vector2 direction)
        {
            this.Type = TargetType.Direction;
            this.Direction = direction;
        }
        public TargetArgs(TargetArgs toCopy)
        {
            this.Type = toCopy.Type;
            this.Global = toCopy.Global;
            this.Face = toCopy.Face;
            this.Precise = toCopy.Precise;
            this.Slot = toCopy.Slot;
            this.Object = toCopy.Object;
            this.Direction = toCopy.Direction;
        }
        public TargetArgs(Vector3 global, GameObjectSlot slot)
        {
            this.Type = TargetType.BlockEntitySlot;
            this.Global = global;
            this.Slot = slot;
        }
        static public TargetArgs Write(BinaryWriter writer, GameObjectSlot slot)
        {
            return new TargetArgs(slot).Write(writer);
        }
        static public TargetArgs Write(BinaryWriter writer, GameObject obj, Vector3? face = null)
        {
              return new TargetArgs(obj, face).Write(writer);
        }
        public TargetArgs Write(BinaryWriter w)
        {
            w.Write((int)this.Type);
            switch (this.Type)
            {
                case TargetType.Slot:
                    //if (this.Slot.Parent is GameObject)
                    //{
                    //    w.Write(0);
                    //    w.Write((this.Slot.Parent as GameObject).Network.ID);
                    //}
                    //else if (this.Slot.Parent is Blocks.BlockEntity)
                    //{
                    //    w.Write(1);
                    //    w.Write((this.Slot.Parent as Blocks.BlockEntity).Global);
                    //}
                    w.Write(this.Slot.Parent.Network.ID);
                    w.Write(this.Slot.ID);
                    w.Write(this.Slot.ContainerNew.ID);
                    return this;

                //case TargetType.Block:
                case TargetType.Position:
                    //w.Write(this.Global + this.Face);
                    w.Write(this.Global);
                    w.Write(this.Face);
                    return this;

                case TargetType.Entity:
                    if (this.Object.Net == null)
                        throw new ArgumentException();
                    w.Write(this.Object.Network.ID);
                    return this;

                case TargetType.Direction:
                    w.Write(this.Direction);
                    return this;

                case TargetType.BlockEntitySlot:
                    w.Write(this.Global);
                    w.Write(this.Slot.ContainerNew.Name);
                    w.Write(this.Slot.ID);
                    return this;

                default:
                    //throw new Exception("Invalid target");
                    return this;
            }
        }

        static public TargetArgs Read(IObjectProvider objects, BinaryReader reader)
        {
            
            TargetType type = (TargetType)reader.ReadInt32();
            switch (type)
            {
                case TargetType.Null:
                    //throw new Exception("Target is null");
                    return TargetArgs.Empty;// new TargetArgs();

                case TargetType.Entity:
                    int netID = reader.ReadInt32();
                    GameObject obj = objects.GetNetworkObject(netID);
                    if (obj == null)
                        throw new Exception(); // force disconnect?
                    //if (obj.IsNull())
                    //    "asdasd".ToConsole();
                    return new TargetArgs(obj);

                //case TargetType.Block:
                //    Vector3 global = reader.ReadVector3();
                //    Vector3 face = reader.ReadVector3();
                //    GameObject target;
                //    if (!Cell.TryGetObject(objects.Map, global, out target))
                //        throw new Exception("Could not create object from block at " + global.ToString());
                //    return new TargetArgs(target, face);

                case TargetType.Position:
                    return new TargetArgs(reader.ReadVector3(), reader.ReadVector3()) { Network = objects };

                case TargetType.Slot:
                    //var parentType = reader.ReadInt32();
                    //GameObjectSlot slot = null;
                    //Components.IHasChildren parent = null;
                    //switch (parentType)
                    //{
                    //    case 0:
                    //        int parentID = reader.ReadInt32();
                    //        parent = objects.GetNetworkObject(parentID);
                    //        break;

                    //    case 1:
                    //        var global = reader.ReadVector3();
                    //        parent = objects.Map.GetBlockEntity(global);
                    //        break;

                    //    default:
                    //        throw new Exception();
                    //        break;
                    //}
                    int parentID = reader.ReadInt32();
                    GameObject parent = objects.GetNetworkObject(parentID);
                    byte slotID = reader.ReadByte();
                    int containerID = reader.ReadInt32();
                    var slot = parent.GetChild(containerID, slotID);
                    return new TargetArgs(slot) { Network = objects };;

                case TargetType.BlockEntitySlot:
                    var vector3 = reader.ReadVector3();
                    var blockentity = objects.Map.GetBlockEntity(vector3);
                    var containerName = reader.ReadString();
                    var slotid = reader.ReadByte();
                    var s = blockentity.GetChild(containerName, slotid);
                    return new TargetArgs(vector3, s) { Network = objects };;

                case TargetType.Direction:
                    return new TargetArgs(reader.ReadVector2()) { Network = objects };;

                default:
                    throw new Exception("Invalid target type " + type.ToString());
            }
        }
        public Vector3 GlobalFinalBlockHeight
        {
            get
            {
                return this.FaceGlobalBlockHeight + this.Precise;
            }
        }
        public Vector3 FaceGlobalBlockHeight
        {
            get
            {
                var blockheight = Block.GetBlockHeight(this.Map, this.Global);
                return this.Global + this.Face * new Vector3(1, 1, blockheight);
            }
        }
        public Vector3 FinalGlobal
        {
            get
            {
                return this.FaceGlobal + this.Precise;
                return this.Global + this.Face + this.Precise;

                //return this.Object.Global + this.Face + this.Precise;
            }
        }
        public Vector3 FaceGlobal
        {
            get
            {
                return this.Global + this.Face;

                //return this.Object.Global + this.Face + this.Precise;
            }
        }
        static public TargetArgs Empty
        {
            get { return new TargetArgs(); }
        }

        public override string ToString()
        {
            //return this.Type.ToString();
            //return this.Object.IsNull() ?  "<null>" :
            //    "Object: " + this.Object.Name +
            //    "\nFace: " + this.Face + 
            //    "\nGlobal: " + this.FinalGlobal;
            switch(this.Type)
            {
                case TargetType.Entity:
                    return this.Object.Name;

                case TargetType.Position:
                    return this.FinalGlobal.ToString();

                case TargetType.Slot:
                    return this.Slot.ToString();//Object.Name;

                default:
                    return this.Type.ToString();
            }
        }
        public Dictionary<string, Interaction> GetInteractions(IObjectProvider net)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    //return this.Object.GetAvailableTasks().FirstOrDefault(i=>i.Name == name);
                    return this.Object.GetInteractions();

                case TargetType.Position:
                    //Block block = this.Global.GetBlock(net.Map);
                    Block block = net.Map.GetBlock(this.Global);
                    var inters = block.GetAvailableTasks(net.Map, this.Global).ToDictionary(foo => foo.Name);
                    var dropInter = new DropCarriedSnap();
                    inters.Add(dropInter.Name, dropInter); // TODO: WORKAROUND until i decide wether to use an interaction registry or add some basic interactions in the base block object
                    return inters;

                default:
                    return null;
            }
        }
        public Interaction GetInteraction(IObjectProvider net, string name)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    //return this.Object.GetAvailableTasks().FirstOrDefault(i=>i.Name == name);
                    Interaction interaction;
                    this.Object.GetInteractions().TryGetValue(name, out interaction);
                    return interaction;


                case TargetType.Position:
                    var rounded = this.Global.RoundXY();
                    //Block block = rounded.GetBlock(net.Map);
                    Block block = net.Map.GetBlock(rounded);
                    var tasks = block.GetAvailableTasks(net.Map, rounded);
                    tasks.Add(new DropCarriedSnap()); // TODO: WORKAROUND until i decide wether to use an interaction registry or add some basic interactions in the base block object
                    return tasks.FirstOrDefault(i => i.Name == name);

                default:
                    return null;
            }
        }
        internal List<Interaction> GetAvailableTasks(IObjectProvider net)
        {
            switch(this.Type)
            {
                case TargetType.Entity:
                    return this.Object.GetAvailableTasks();

                case TargetType.Position:
                    //Block block = this.Global.GetBlock(net.Map);
                    Block block = net.Map.GetBlock(this.Global);

                    return block.GetAvailableTasks(net.Map, this.Global);

                default:
                    return new List<Interaction>();
            }
        }
        internal ContextAction GetRightClickAction()
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    return this.Object.GetRightClickActions().FirstOrDefault();//.First();

                case TargetType.Position:
                    var block = Net.Client.Instance.Map.GetBlock(this.Global);
                    return block.GetRightClickAction(this.Global);

                    //return this.Global.GetBlock(Net.Client.Instance.Map).GetRightClickAction(this.Global);

                default:
                    return null;
            }
        }
        //internal Interaction GetContextAction(KeyBinding key)
        //{
        //    switch (this.Type)
        //    {
        //        case TargetType.Entity:
        //            return this.Object.GetPlayerActions().FirstOrDefault(foo=>foo.Key == key).Value;

        //        default:
        //            return null;
        //    }
        //}
        internal void GetContextAll(ContextArgs args)
        {
            //var rb = this.GetContextRB();
            //if (rb != null)
            //    args.Actions.Add(rb);
            var list = new ContextAction[]{
                this.GetContextRB(),
                this.GetContextActivate()
            };
            args.Actions.AddRange(list.Where(i => i != null));
        }

        internal ContextAction GetContextRB()
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    return this.Object.GetContextRB(Player.Actor);
                    break;

                case TargetType.Position:
                    var block = this.Network.Map.GetBlock(this.Global);
                    return block.GetContextRB(Player.Actor, this.Global);
                    break;

                default:
                    return null;
                    break;
            }
        }
        internal ContextAction GetContextActivate()
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    //this.Object.GetContextActions(a);
                    return this.Object.GetContextActivate(Player.Actor);
                    //return null;
                    break;

                case TargetType.Position:
                    var block = this.Network.Map.GetBlock(this.Global);
                    return block.GetContextActivate(Player.Actor, this.Global);
                    break;

                default:
                    return null;
                    break;
            }
        }

        internal ContextAction GetContextAction()
        {
            //return this.GetContextActions().Actions.FirstOrDefault();
            return this.GetContextActions().Actions.FirstOrDefault(a=>a.Available());
        }
        internal ContextArgs GetContextActions()
        {
            var a = new ContextArgs();
            this.GetContextActions(a);
            return a;
        }
        internal Dictionary<PlayerInput, ContextAction> GetContextActionsFromInput()
        {
            var list = new Dictionary<PlayerInput, ContextAction>();
            this.GetContextActions(list);
            return list;
        }
        internal ContextAction GetContextActionsFromInput(PlayerInput input)
        {
            //var list = new Dictionary<PlayerInput, ContextAction>();
            //this.GetContextActions(list);
            return this.GetContextActionsFromInput().FirstOrDefault(i => i.Key == input).Value;
        }
        public void GetContextActions(Dictionary<PlayerInput, ContextAction> list)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    //this.Object.GetContextActions(a);
                    break;

                case TargetType.Position:
                    Dictionary<PlayerInput, Interaction> interactions = new Dictionary<PlayerInput, Interaction>();
                    var block = this.Network.Map.GetBlock(this.Global);
                    block.GetPlayerActionsWorld(Player.Actor, this.Global, list);
                    break;

                default:
                    break;
            }

        }
        public void GetContextActions(ContextArgs a)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.Object.GetContextActions(a);
                    break;

                case TargetType.Position:
                    Dictionary<PlayerInput, Interaction> interactions = new Dictionary<PlayerInput, Interaction>();
                    var block = this.Network.Map.GetBlock(this.Global);
                    block.GetContextActions(Player.Actor, this.Global, a);
                    break;

                default:
                    break;
            }

        }
        public void GetContextActions(GameObject player, ContextArgs a)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.Object.GetContextActions(a);
                    break;

                case TargetType.Position:
                    Dictionary<PlayerInput, Interaction> interactions = new Dictionary<PlayerInput, Interaction>();
                    var block = this.Network.Map.GetBlock(this.Global);
                    block.GetContextActions(player, this.Global, a);
                    break;

                default:
                    break;
            }

        }

        internal Interaction GetContextActionWorld(GameObject player, PlayerInput input)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    return this.Object.GetPlayerActionsWorld().FirstOrDefault(foo => foo.Key.Action == input.Action && foo.Key.Hold == input.Hold).Value;

                case TargetType.Position:
                    
                    Dictionary<PlayerInput, Interaction> interactions = new Dictionary<PlayerInput, Interaction>();
                    var block = player.Map.GetBlock(this.Global);
                    block.GetPlayerActionsWorld(player, this.Global, interactions);

                    return interactions.FirstOrDefault(foo => foo.Key.Action == input.Action && foo.Key.Hold == input.Hold).Value;

                default:
                    return null;
            }
        }
        internal Dictionary<PlayerInput, Interaction> GetContextActionsWorld(GameObject player)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    return this.Object.GetPlayerActionsWorld();

                case TargetType.Position:
                    Dictionary<PlayerInput, Interaction> interactions = new Dictionary<PlayerInput, Interaction>();
                    //var block = this.Global.GetBlock(net.Map);
                    var block = player.Map.GetBlock(this.Global);

                    if(block != null)
                        block.GetPlayerActionsWorld(player, this.Global, interactions);
                    return interactions;

                default:
                    return new Dictionary<PlayerInput, Interaction>();
                    //return null;
            }
        }

        public void HandleRemoteCall(IObjectProvider net, ObjectEventArgs e)
        {
            switch(this.Type)
            {
                case TargetType.Entity:
                    this.Object.HandleRemoteCall(e);
                    break;

                case TargetType.Position:
                    //net.Map.GetBlock(this.Global).HandleRemoteCall(e);
                    var blockEntity = net.Map.GetBlockEntity(this.Global);
                    if (blockEntity == null)
                        throw new Exception();
                    blockEntity.HandleRemoteCall(net.Map, this.Global, e);
                    break;

                default:
                    break;
            }
        }

        //override eq



        
    }

}
