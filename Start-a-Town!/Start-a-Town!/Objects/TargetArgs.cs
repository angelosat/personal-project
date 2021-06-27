using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public enum TargetType { Null, Entity, Slot, BlockEntitySlot, Position, Direction }

    public class TargetArgs : ITooltippable, IContextable, ISelectable, ILabeled
    {
        public void GetTooltipInfo(UI.Tooltip tooltip)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    if (this.Object != null)
                        this.Object.GetTooltipInfo(tooltip);
                    break;

                case TargetType.Position:
                    //Engine.Map.GetBlock(this.Global).GetTooltip(tooltip, Engine.Map, this.Global);
                    this.Map.GetBlock(this.Global).GetTooltip(tooltip, this.Map, this.Global);

                    break;

                default: break;
            }
            this.Map.Town.OnTooltipCreated(tooltip, this);
            return;
        }
        //public IMap Map;
        IMap _Map;
        public IMap Map
        {
            get { return this._Map; }// this.Type == TargetType.Entity ? this.Object.Map : this._Map; }
            set
            {
                this._Map = value;
                //this.Network = value.Net;
            }
        }

        public IObjectProvider Network
        {
            get { return this.Map.Net; }
            //set
            //        {

            //        }
        }

        public Vector2 Direction;
        public TargetType Type { get; private set; }
        //public Vector3 Global { get; set; }
        Vector3 _Global;
        public Vector3 Global
        {
            set { this._Global = value; }
            get
            {
                if (this.Type == TargetType.Slot)
                    if (this.Slot.Object != null)
                        return this.Slot.Object.Global;
                if (this.Type == TargetType.Entity)
                    return this.Object.Transform.Global;
                return this._Global;
            }
        }
        public int EntityID = -1;
        GameObject CachedObject;
        public GameObject Object// { get; set; }
        {
            get
            {
                if (this.Type == TargetType.Entity)
                {
                    if (this.EntityID == -1)
                        throw new Exception();
                    if (this.CachedObject == null)
                    {
                        this.CachedObject = this.Map.Net.GetNetworkObject(this.EntityID);
                    }
                }
                //GameObject obj = null;
                //if (this.Type == TargetType.Entity)
                //    obj = this.Network.GetNetworkObject(this.EntityID);
                //    //obj = this.Map.Net.GetNetworkObject(this.EntityID);

                else if (this.Type == TargetType.Slot || this.Type == TargetType.BlockEntitySlot)
                {
                    if (this.CachedObject == null)
                    {
                        this.CachedObject = this.Slot.Object;
                        //obj = this.Slot.Object;
                        //if (obj != null)
                        //{
                        //    this.CachedObject = obj;
                        //    return obj;
                        //}
                        //else
                        //    return this.CachedObject;
                    }
                }
                return this.CachedObject;
            }
            //set
            //{ 
            //    this._Object = value;
            //}
        }

        internal T GetBlockEntity<T>() where T : class
        {
            return this.Map.GetBlockEntity(this.Global) as T;
        }

        public Vector3 Face;
        public Vector3 Precise;

        int ParentID, ContainerID, SlotID;
        string ContainerName;
        GameObjectSlot _Slot;
        public GameObjectSlot Slot //{ get; set; }
        {
            get
            {
                if (this._Slot != null)
                {
                    return this._Slot;
                }
                switch (this.Type)
                {
                    case TargetType.Slot:
                        GameObject parent = this.Network.GetNetworkObject(this.ParentID);
                        return parent.GetChild(this.ContainerID, this.SlotID);

                    case TargetType.BlockEntitySlot:
                        var blockentity = this.Map.GetBlockEntity(this.Global);
                        return blockentity.GetChild(this.ContainerName, this.SlotID);

                    default:
                        return null;
                }
            }
            set { this._Slot = value; }
        }

        //public TargetArgs()
        //{
        //    this.Face = Vector3.Zero;
        //    this.Precise = Vector3.Zero;
        //}
        TargetArgs() { }
        public TargetArgs(GameObject obj)
        {
            this.Type = TargetType.Entity;
            this.EntityID = obj.RefID;
            this.CachedObject = obj;
            this._Map = obj.Map;

            // struct assignments
            this._Global = Vector3.Zero;
            this.Face = Vector3.Zero;
            this.Precise = Vector3.Zero;
            this.Direction = Vector2.Zero;
            this.ParentID = -1;
            this.ContainerID = -1;
            this._Slot = null;
            this.SlotID = -1;
            this.ContainerName = "";
        }
        public TargetArgs(IObjectProvider network, int entityID)
        {
            //throw new Exception();
            this.Type = TargetType.Entity;
            this.EntityID = entityID;
            //this.Network = network;
        }
        public TargetArgs(IMap map, int entityID)
        {
            this.Type = TargetType.Entity;
            this.EntityID = entityID;
            this.Map = map;
        }
        public TargetArgs(GameObject obj, Vector3 face)
        {
            //throw new Exception();
            this.Type = TargetType.Entity;
            this.EntityID = obj.RefID;
            this.Global = obj.Global;
            this.CachedObject = obj;
            this.Map = obj.Map;
            //this.Network = obj.Net;

        }
        public TargetArgs(GameObject obj, Vector3? face)
        {
            this.Type = TargetType.Entity;
            this.EntityID = obj.RefID;
            this.Global = obj.Global;
            this.Face = face.HasValue ? face.Value : Vector3.Zero;
            this.CachedObject = obj;
            this.Map = obj.Map;
            //this.Network = obj.Net;
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
        public TargetArgs(IMap map, Vector3 global, Vector3 face, Vector3 precise)
        {
            this.Type = TargetType.Position;
            this.Map = map;
            this.Global = global;
            this.Face = face;
            this.Precise = precise;
        }
        public TargetArgs(GameObjectSlot slot)
        {
            this.Type = TargetType.Slot;
            //if (slot.ContainerNew != null)
            //{
            //    this.ContainerID = slot.ContainerNew.ID;
            //    this.SlotID = slot.ID;
            //}
            this.Slot = slot;
        }
        public TargetArgs(Vector2 direction)
        {
            this.Type = TargetType.Direction;
            this.Direction = direction;
        }
        //public TargetArgs(TargetArgs toCopy)
        //{
        //    this.Type = toCopy.Type;
        //    this.Global = toCopy.Global;
        //    this.Face = toCopy.Face;
        //    this.Precise = toCopy.Precise;
        //    //this.Slot = toCopy.Slot;
        //    this.ContainerName = toCopy.ContainerName;
        //    this.ContainerID = toCopy.ContainerID;
        //    this.SlotID = toCopy.SlotID;
        //    //this.Object = toCopy.Object;
        //    //this.Network = toCopy.Network;
        //    this.Direction = toCopy.Direction;
        //    this.EntityID = toCopy.EntityID;
        //    this._Map = toCopy._Map;
        //}
        public TargetArgs Clone()
        {
            var copy = new TargetArgs();
            copy.Type = this.Type;
            copy.Global = this.Global;
            copy.Face = this.Face;
            copy.Precise = this.Precise;
            //copy.Slot = this.Slot;
            copy.ContainerName = this.ContainerName;
            copy.ContainerID = this.ContainerID;
            copy.SlotID = this.SlotID;
            //copy.Object = this.Object;
            //copy.Network = this.Network;
            copy.Direction = this.Direction;
            copy.EntityID = this.EntityID;
            copy._Map = this._Map;
            return copy;
        }
        public TargetArgs(Vector3 global, GameObjectSlot slot)
            : this(null, global, slot)
        {

        }
        public TargetArgs(IMap map, Vector3 global, GameObjectSlot slot)
        {
            if (slot == null)
                throw new Exception();
            this.Type = TargetType.BlockEntitySlot;
            this.Map = map;
            this.Global = global;
            this.ContainerName = slot.ContainerNew.Name;
            this.SlotID = slot.ID;
            //this.Slot = slot;
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
                    
                    w.Write(this.Slot.Parent.RefID);
                    w.Write(this.Slot.ID);
                    w.Write(this.Slot.ContainerNew.ID);
                    return this;

                //case TargetType.Block:
                case TargetType.Position:
                    w.Write(this.Global);
                    w.Write(this.Face);
                    w.Write(this.Precise);
                    return this;

                case TargetType.Entity:
               
                    w.Write(this.EntityID);
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
                    return this;
            }
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name, this.SaveAsList());
            return tag;
        }
        public List<SaveTag> SaveAsList()
        {
            var tag = new List<SaveTag>();
            tag.Add(new SaveTag(SaveTag.Types.Int, "Type", (int)this.Type));
            switch (this.Type)
            {
                case TargetType.Slot:
                    tag.Add(new SaveTag(SaveTag.Types.Int, "ParentID", this.Slot.Parent.RefID));
                    tag.Add(new SaveTag(SaveTag.Types.Int, "SlotID", this.Slot.ID));
                    tag.Add(new SaveTag(SaveTag.Types.Int, "ContainerID", this.Slot.ContainerNew.ID));
                    break;

                case TargetType.Position:
                    tag.Add(new SaveTag(SaveTag.Types.Vector3, "Global", this.Global));
                    tag.Add(new SaveTag(SaveTag.Types.Vector3, "Face", this.Face));
                    tag.Add(new SaveTag(SaveTag.Types.Vector3, "Precise", this.Precise));
                    break;

                case TargetType.Entity:
                    //if (this.Object.Net == null)
                    //    throw new ArgumentException();
                    //tag.Add(new SaveTag(SaveTag.Types.Int, "InstanceID", this.Object.InstanceID));
                    tag.Add(new SaveTag(SaveTag.Types.Int, "InstanceID", this.EntityID));
                    break;

                case TargetType.Direction:
                    tag.Add(new SaveTag(SaveTag.Types.Vector3, "Direction", new Vector3(this.Direction, 0)));
                    break;

                case TargetType.BlockEntitySlot:
                    tag.Add(new SaveTag(SaveTag.Types.Vector3, "Global", this.Global));
                    tag.Add(new SaveTag(SaveTag.Types.String, "ContainerName", this.Slot.ContainerNew.Name));
                    tag.Add(new SaveTag(SaveTag.Types.Int, "SlotID", this.Slot.ID));
                    break;

                default:
                    break;

            }
            return tag;
        }

        static public TargetArgs Read(IObjectProvider objects, BinaryReader reader)
        {

            TargetType type = (TargetType)reader.ReadInt32();
            switch (type)
            {
                case TargetType.Null:
                    //throw new Exception("Target is null");

                    return TargetArgs.Null;// new TargetArgs();

                case TargetType.Entity:
                    int netID = reader.ReadInt32();
                    //GameObject obj = objects.GetNetworkObject(netID);
                    //if (obj == null)
                    //    throw new Exception(); // force disconnect?
                    return new TargetArgs(objects, netID);// new TargetArgs(obj);

                //case TargetType.Block:
                //    Vector3 global = reader.ReadVector3();
                //    Vector3 face = reader.ReadVector3();
                //    GameObject target;
                //    if (!Cell.TryGetObject(objects.Map, global, out target))
                //        throw new Exception("Could not create object from block at " + global.ToString());
                //    return new TargetArgs(target, face);

                case TargetType.Position:
                    return new TargetArgs(reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3());// { Network = objects, Map = objects.Map };

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
                    return new TargetArgs(slot) { Map = objects.Map };// Network = objects };

                case TargetType.BlockEntitySlot:
                    var vector3 = reader.ReadVector3();
                    var blockentity = objects.Map.GetBlockEntity(vector3);
                    var containerName = reader.ReadString();
                    var slotid = reader.ReadByte();
                    var s = blockentity.GetChild(containerName, slotid);
                    return new TargetArgs(objects.Map, vector3, s) { Map = objects.Map };// { Network = objects };

                case TargetType.Direction:
                    return new TargetArgs(reader.ReadVector2()) { Map = objects.Map };// { Network = objects };;

                default:
                    throw new Exception("Invalid target type " + type.ToString());
            }
        }
        static public TargetArgs Read(IMap map, BinaryReader reader)
        {

            TargetType type = (TargetType)reader.ReadInt32();
            switch (type)
            {
                case TargetType.Null:
                    return TargetArgs.Null;

                case TargetType.Entity:
                    int netID = reader.ReadInt32();
                    return new TargetArgs(map, netID);// new TargetArgs(obj);

               
                case TargetType.Position:
                    var t = new TargetArgs(reader.ReadVector3(), reader.ReadVector3(), reader.ReadVector3());// { Network = objects, Map = objects.Map };
                    t.Map = map;
                    return t;

                case TargetType.Slot:
                    int parentID = reader.ReadInt32();
                    GameObject parent = map.Net.GetNetworkObject(parentID);
                    byte slotID = reader.ReadByte();
                    int containerID = reader.ReadInt32();
                    var slot = parent.GetChild(containerID, slotID);
                    return new TargetArgs(slot) { Map = map };

                case TargetType.BlockEntitySlot:
                    var vector3 = reader.ReadVector3();
                    var blockentity = map.GetBlockEntity(vector3);
                    var containerName = reader.ReadString();
                    var slotid = reader.ReadByte();
                    var s = blockentity.GetChild(containerName, slotid);
                    return new TargetArgs(map, vector3, s);

                case TargetType.Direction:
                    return new TargetArgs(reader.ReadVector2());// { Map = objects.Map };// { Network = objects };;

                default:
                    throw new Exception("Invalid target type " + type.ToString());
            }
        }

        

        public void Load(IMap map, SaveTag tag)
        {
            //this.Network = net;
            this.Map = map;
            this.Type = (TargetType)tag.GetValue<int>("Type");
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.EntityID = tag.GetValue<int>("InstanceID");
                    break;

                case TargetType.Position:
                    this.Global = tag.GetValue<Vector3>("Global");
                    this.Face = tag.GetValue<Vector3>("Face");
                    this.Precise = tag.GetValue<Vector3>("Precise");
                    break;

                case TargetType.Slot:
                    int parentID = tag.GetValue<int>("ParentID");
                    int slotID = tag.GetValue<int>("SlotID");
                    int containerID = tag.GetValue<int>("ContainerID");
                    this.ParentID = parentID;
                    this.SlotID = slotID;
                    this.ContainerID = containerID;
                    break;

                case TargetType.Direction:
                    var dir3d = tag.GetValue<Vector3>("Direction");
                    this.Direction = new Vector2(dir3d.X, dir3d.Y);
                    break;

                case TargetType.BlockEntitySlot:
                    this.Global = tag.GetValue<Vector3>("Global");
                    var containerName = tag.GetValue<string>("ContainerName");
                    var slotid = tag.GetValue<int>("SlotID");
                    this.ContainerName = containerName;
                    this.SlotID = slotid;
                    break;
            }
        }

        public TargetArgs(SaveTag tag)
        {
            this.Load(null, tag);
        }

        public TargetArgs(IMap map, SaveTag tag)
        {
            this.Load(map, tag);
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

            }
        }
        public Vector3 FaceGlobal
        {
            get
            {
                return this.Global + this.Face;

            }
        }

        static readonly public TargetArgs Null = new TargetArgs();

        public string Label
        {
            get
            {
                switch (this.Type)
                {
                    case TargetType.Entity:
                        //return this.Object.Name;
                        return string.Format("[id:{0}] {1}", this.EntityID, this.Object.Name);

                    case TargetType.Position:
                        return this.Map.GetBlock(this.Global).Name;

                    case TargetType.Slot:
                        return this.Slot.ToString();//Object.Name;

                    default:
                        return this.Type.ToString();
                }
            }
        }
        public override string ToString()
        {
            //return this.Type.ToString();
            //return this.Object.IsNull() ?  "<null>" :
            //    "Object: " + this.Object.Name +
            //    "\nFace: " + this.Face + 
            //    "\nGlobal: " + this.FinalGlobal;
            switch (this.Type)
            {
                case TargetType.Entity:
                    //return this.Object.Name;
                    return string.Format("[id:{0}] {1}", this.EntityID, this.Object.Name);

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
                    //var dropInter = new DropCarriedSnap();
                    var dropInter = new UseHauledOnTarget();
                    inters.Add(dropInter.Name, dropInter); // TODO: WORKAROUND until i decide wether to use an interaction registry or add some basic interactions in the base block object
                    return inters;

                default:
                    var list = new Dictionary<string, Interaction>();
                    var dropinvitem = new DropInventoryItem();
                    var dropeq = new InteractionDropEquipped();
                    //var dropeqt = new DropEquippedTarget();

                    var unequip = new Unequip();
                    var throwInter = new InteractionThrow();
                    list.Add(dropinvitem.Name, dropinvitem);
                    list.Add(dropeq.Name, dropeq);
                    //list.Add(dropeqt.Name, dropeqt);

                    list.Add(throwInter.Name, throwInter);
                    list.Add(unequip.Name, unequip);
                    return list;
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
                    //tasks.Add(new DropCarriedSnap()); // TODO: WORKAROUND until i decide wether to use an interaction registry or add some basic interactions in the base block object
                    tasks.Add(new UseHauledOnTarget()); // TODO: WORKAROUND until i decide wether to use an interaction registry or add some basic interactions in the base block object
                    return tasks.FirstOrDefault(i => i.Name == name);

                default:
                    return null;
            }
        }
        public Interaction GetInteraction(string name)
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
                    Block block = this.Map.GetBlock(rounded);
                    var tasks = block.GetAvailableTasks(this.Map, rounded);
                    //tasks.Add(new DropCarriedSnap()); // TODO: WORKAROUND until i decide wether to use an interaction registry or add some basic interactions in the base block object
                    tasks.Add(new UseHauledOnTarget()); // TODO: WORKAROUND until i decide wether to use an interaction registry or add some basic interactions in the base block object
                    return tasks.FirstOrDefault(i => i.Name == name);

                default:
                    return null;
            }
        }
        internal List<Interaction> GetAvailableTasks(IObjectProvider net)
        {
            switch (this.Type)
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
                    var block = Client.Instance.Map.GetBlock(this.Global);
                    return block.GetRightClickAction(this.Global);

                //return this.Global.GetBlock(Client.Instance.Map).GetRightClickAction(this.Global);

                default:
                    return null;
            }
        }

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
                    return this.Object.GetContextRB(PlayerOld.Actor);

                case TargetType.Position:
                    var block = this.Network.Map.GetBlock(this.Global);
                    return block.GetContextRB(PlayerOld.Actor, this.Global);

                default:
                    return null;
            }
        }

        internal ContextAction GetContextActivate()
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    //this.Object.GetContextActions(a);
                    return this.Object.GetContextActivate(PlayerOld.Actor);
                //return null;

                case TargetType.Position:
                    var block = this.Network.Map.GetBlock(this.Global);
                    return block.GetContextActivate(PlayerOld.Actor, this.Global);

                default:
                    return null;
            }
        }

        internal ContextAction GetContextAction()
        {
            //return this.GetContextActions().Actions.FirstOrDefault();
            return this.GetContextActions().Actions.FirstOrDefault(a => a.Available());
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
                    block.GetPlayerActionsWorld(PlayerOld.Actor, this.Global, list);
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
                    block.GetContextActions(PlayerOld.Actor, this.Global, a);

                    // check if block is part of any town designations such as stockpiles or fields, and add corresponding actions
                    this.Map.Town.GetContextActions(this.Global, a);

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

                    if (block != null)
                        block.GetPlayerActionsWorld(player, this.Global, interactions);
                    return interactions;

                default:
                    return new Dictionary<PlayerInput, Interaction>();
                    //return null;
            }
        }

        public void HandleRemoteCall(IObjectProvider net, ObjectEventArgs e)
        {
            switch (this.Type)
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


        public bool IsEqual(TargetArgs target)
        {
            if (this.Type != target.Type)
                return false;
            if (this.Type == TargetType.Entity && this.Object != null && this.Object == target.Object)
                return true;
            //else if (this.Type == TargetType.Position && this.FaceGlobal == target.FaceGlobal) // this.Global == target.Global
            else if (this.Type == TargetType.Position && this.Global == target.Global 
                && this.Face == target.Face) // newly added
                return true;
            return false;
        }
        public bool IsEqualFace(TargetArgs target)
        {
            if (this.Type != target.Type)
                return false;
            if (this.Type == TargetType.Entity && this.Object != null && this.Object == target.Object)
                return true;
            else if (this.Type == TargetType.Position && this.Global == target.Global && this.Face == target.Face)
                return true;
            return false;
        }

        public Block Block => this.GetBlock();
        internal Block GetBlock()
        {
            return this.Map.GetBlock(this.Global);
        }
        internal Blocks.BlockEntity GetBlockEntity()
        {
            return this.Map.GetBlockEntity(this.Global);
        }
        public string GetName()
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    return this.Object.Name;

                case TargetType.Position:
                    return this.GetBlock().GetName(this.Map, this.Global);

                default:
                    return "";
            }
        }
        //public void Select(UISelectedInfo info)
        //{
        //    switch (this.Type)
        //    {
        //        case TargetType.Entity:
        //            this.Object.Select(info);
        //            break;

        //        case TargetType.Position:
        //            this.GetBlock().Select(info, this.Map, this.Global);
        //            break;

        //        default:
        //            return;
        //    }
        //}
        public IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    foreach (var i in this.Object.GetInfoTabs())
                        yield return i;
                    break;

                case TargetType.Position:
                    foreach (var i in this.GetBlock().GetInfoTabs())
                        yield return i;
                        break;

                default:
                    yield break;
            }
            foreach (var i in this.Map.GetInfoTabs())
                yield return i;
        }
        public void GetSelectionInfo(IUISelection info)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.Object.GetSelectionInfo(info);
                    break;

                case TargetType.Position:
                    this.GetBlock().GetSelectionInfo(info, this.Map, this.Global);
                    break;

                default:
                    return;
            }
            this.Map.OnTargetSelected(info, this);

            //this.Map.Town.OnTargetSelected(info, this);
        }
        public void GetQuickButtons(UISelectedInfo info)
        {
            switch (this.Type)
            {
                case TargetType.Entity:
                    this.Object.GetQuickButtons(info);
                    break;

                case TargetType.Position:
                    this.GetBlock().GetQuickButtons(info, this.Map, this.Global);
                    break;

                default:
                    return;
            }
        }

        public void TabGetter(Action<string, Action> getter)
        {
            throw new Exception();
        }

       

        public bool Exists
        {
            get
            {
                switch (this.Type)
                {
                    case TargetType.Entity:
                        //return this.Object != null || this.Object.IsSpawned;
                        return this.Object != null && this.Object.IsSpawned;

                    case TargetType.Position:
                        return this.GetBlock() != BlockDefOf.Air;
                    //return map.GetBlock(this.Global) != Block.Air;

                    default:
                        throw new Exception();
                }
            }
        }
        public bool IsForbidden
        {
            get
            {
                return this.Type == TargetType.Entity ? this.Object.IsForbidden : false;
            }
        }

        public bool HasObject { get { return this.Object != null; } }

        public static implicit operator GameObject(TargetArgs b) => b.Object;
        public static implicit operator Entity(TargetArgs b) => b.Object as Entity;
        public static implicit operator Actor(TargetArgs b) => b.Object as Actor;

        public static implicit operator TargetArgs(GameObject obj)
        {
            return new TargetArgs(obj);
        }
        public static implicit operator TargetArgs((IMap map, Vector3 global) location)
        {
            return new TargetArgs(location.map, location.global);
        }
    }
}
