using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    public class PersonalInventoryComponent : EntityComponent
    {
        class Packets
        {
            static int PacketSyncInsert, PacketSetHaulSlot;
            static public void Init()
            {
                PacketSyncInsert = Network.RegisterPacketHandler(HandleSyncInsert);
                void handleSetHaulSlot(IObjectProvider net, BinaryReader r)
                {
                    var actor = net.GetNetworkObject(r.ReadInt32()) as Actor;
                    var item = net.GetNetworkObject(r.ReadInt32()) as Entity;
                    actor.Carry(item);
                }

                PacketSetHaulSlot = Network.RegisterPacketHandler(handleSetHaulSlot);
            }

            public static void SendSyncInsert(IObjectProvider net, Actor actor, Entity item)
            {
                if(net is Server)
                    actor.Inventory.Insert(item);
                net.GetOutgoingStream().Write(PacketSyncInsert, actor.RefID, item.RefID);
            }
            private static void HandleSyncInsert(IObjectProvider net, BinaryReader r)
            {
                var actor = net.GetNetworkObject(r.ReadInt32()) as Actor;
                var item = net.GetNetworkObject(r.ReadInt32()) as Entity;
                if (net is Server)
                    SendSyncInsert(net, actor, item);
                else
                    actor.Inventory.Insert(item);
            }

            public static void SyncSetHaulSlot(IObjectProvider net, Actor actor, Entity item)
            {
                var server = net as Server;
                var w = server.GetOutgoingStream();
                w.Write(PacketSetHaulSlot);
                w.Write(actor.RefID);
                w.Write(item.RefID);
            }
        }
        static PersonalInventoryComponent()
        {
            Packets.Init();
        }
        public int Capacity => this.Slots.Slots.Count;
        public float PercentageEmpty => this.GetEmptySlots().Count() / (float)this.Capacity;
        public float PercentageFull => 1 - this.PercentageEmpty;

        readonly Container HaulContainer;
        readonly GameObjectSlot HaulSlot;

        internal void Remove(Entity obj)
        {
            this.Slots.Remove(obj);
        }
        internal void Remove(GameObject obj)
        {
            this.Remove(obj as Entity);
        }

        public Container Slots;

        internal void SyncInsert(GameObject split)
        {
            var actor = this.Parent as Actor;
            var net = actor.Net;
            if (net is not Server server)
                throw new Exception();
            Packets.SendSyncInsert(net, actor, split as Entity);
        }

        public override string ComponentName => "PersonalInventory";
          

        public bool HasEmptySpace => this.GetEmptySlots().Any();

        public float Distance(GameObject obj1, GameObject obj2)
        {
            return obj1.Inventory.Contains(obj => obj == obj2) ? 0 : -1;
        }
        public Vector3? DistanceVector(GameObject obj1, GameObject obj2)
        {
            return obj1.Inventory.Contains(obj => obj == obj2) ? Vector3.Zero : null;
        }

        public override void MakeChildOf(GameObject parent)
        {
            this.Parent = parent;
           
            parent.RegisterContainer(this.Slots);
            parent.RegisterContainer(this.HaulContainer);
        }
        public GameObjectSlot GetHauling()
        {
            return this.HaulSlot;
        }
        public static GameObjectSlot GetHauling(GameObject parent)
        {
            return parent.GetComponent<PersonalInventoryComponent>().GetHauling();
        }
        
        public override void GetContainers(List<Container> list)
        {
            list.Add(this.HaulContainer);
            list.Add(this.Slots);
        }
        public PersonalInventoryComponent()
            : base()
        {
            this.Parent = null;
            this.Slots = new Container() { Name = "Inventory" };
            this.HaulContainer = new Container(1) { Name = "Hauling" };
            this.HaulSlot = this.HaulContainer.Slots.First();
        }
        public PersonalInventoryComponent(byte capacity)
            : this()
        {
            this.Slots = new Container(capacity)
            {
                Name = "Inventory"
                
            };
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                    // TODO: maybe create a new message called InventoryInteraction that individual components can respond too?
                case Message.Types.SlotInteraction:
                    this.SlotInteraction(parent, e.Parameters[0] as GameObject, e.Parameters[1] as GameObjectSlot);
                    return true;

                default:
                    return false;
            }
        }
        public void DropInventoryItem(GameObject item)
        {
            var slots = this.Slots;
            var parent = this.Parent;
            var slot = slots.Slots.Where(s => s.Object == item).First();
            slot.Clear();
            item.Spawn(parent.Map, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));
        }
        public void HaulFromInventory(GameObject item, int amount = -1)
        {
            if (amount == 0)
                throw new Exception();
            amount = amount == -1 ? item.StackSize : amount;
            var slots = this.Slots;
            var parent = this.Parent;
            var slot = slots.Slots.Where(s => s.Object == item).First();
            var obj = slot.Object;
            var currentAmount = obj.StackSize;
            if (amount > currentAmount)
                throw new Exception();
            else if(amount == currentAmount)
            {
                this.Haul(item);
            }
            else if(amount < currentAmount)
            {
                slot.Object.StackSize -= amount;
                if (parent.Net is Server server)
                {
                    var splitItem = obj.Clone();
                    splitItem.StackSize = amount;
                    splitItem.SyncInstantiate(server);
                    Packets.SyncSetHaulSlot(server, parent as Actor, splitItem as Entity);
                    this.Haul(splitItem);
                }
            }
        }
        public static GameObject DropInventoryItem(GameObject parent, GameObject item, int amount)
        {
            GameObject obj;
            var children = parent.GetChildren();
            GameObjectSlot childslot = children.First(i => i.Object == item);

            if (amount < childslot.Object.StackSize)
            {
                obj = childslot.Object.Clone();
                obj.StackSize = amount;
            }
            else
                obj = childslot.Object;
            parent.Net.Spawn(obj, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));
            childslot.StackSize -= amount;
            return obj;
        }
        private static GameObject DropInventoryItem(GameObject parent, int slotID, int amount)
        {
            GameObject obj;
            var children = parent.GetChildren();
            GameObjectSlot childslot = children[slotID];
            
            if (amount < childslot.Object.StackSize)
            {
                obj = childslot.Object.Clone();
                obj.StackSize = amount;
            }
            else
                obj = childslot.Object;
            parent.Net.Spawn(obj, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));
            childslot.StackSize -= amount;
            return obj;
        }

        static public GameObjectSlot FindFirst(GameObject parent, Func<GameObject, bool> condition)
        {
            PersonalInventoryComponent comp;
            if (!parent.TryGetComponent<PersonalInventoryComponent>(out comp))
                return null;
            var contents = comp.GetContents();
            var found = contents.FirstOrDefault(foo => condition(foo.Object));
            return found;
        }

        void SlotInteraction(GameObject parent, GameObject actor, GameObjectSlot slot)
        {
            if (!slot.HasValue)
            {
                // if right clicking an empty slot, put player's hauled item in it
                var hauled = PersonalInventoryComponent.GetHauling(actor);
                if (hauled.HasValue)
                    hauled.Swap(slot);
                return;
            }
            var obj = slot.Object;

            if (obj.HasComponent<EquipComponent>())
                parent.GetComponent<WorkComponent>().Perform(parent, new Interactions.EquipFromInventory(), new TargetArgs(slot));
            else
            {
                this.HaulSlot.Swap(slot);
            }
        }

        public bool StoreHauled()
        {
            if (this.HaulSlot.Object == null)
                return false;
            if (!this.Slots.InsertObject(this.HaulSlot))

            {
                // throw? or return false and raise event so we can handle it and display a message : not enough space?
                //inv.Throw(parent, Vector3.Zero);
                this.Parent.Net.EventOccured(Message.Types.NotEnoughSpace, this.Parent);
                return false;
            }

            
            //NpcComponent.AddPossesion(parent, obj); // why was i adding the item as a possesion here? the item becomes a possesion during ownership assignment
                                                        // BECAUSE i want npc to claim ownership when picking up and storing ie. food in their inventory
                                                        // but other problems arise if i set ownership here

            return true;
        }
        public bool Insert(GameObject obj)
        {
            return this.Insert(obj as Entity);
        }
        public bool Insert(Entity obj)
        {
            var actor = this.Parent as Actor;
            if (obj == null)
                return false;
            if (!this.Slots.InsertObject(obj))

            {
                // throw? or return false and raise event so we can handle it and display a message : not enough space?
                //inv.Throw(parent, Vector3.Zero);
                actor.Net.EventOccured(Message.Types.NotEnoughSpace, actor);
                return false;
            }
            return true;
        }
        
        public bool RemoveItem(Entity obj)
        {
            if (obj is null)
                return false;
            return this.Slots.Remove(obj);
        }
        public bool PickUp(GameObject obj, int amount)
        {
            var parent = this.Parent;
            if (this.HaulSlot.Object is GameObject currentHauled)
            {
                if (currentHauled.CanAbsorb(obj))
                {
                    var transferAmount = Math.Min(amount == -1 ? obj.StackSize : amount, currentHauled.StackMax - currentHauled.StackSize);
                    if (transferAmount == 0)
                        throw new Exception();
                    currentHauled.StackSize += transferAmount;
                    if (transferAmount == obj.StackSize)
                    {
                        parent.Net.Despawn(obj);
                        parent.Net.DisposeObject(obj);
                    }
                    obj.StackSize -= transferAmount;
                    return false;
                }
                //else
                // if we maxed out our hauling stack, but there is some remaining amout of item on the ground, store hauled stack and haul the remaining item stack
                else if (!this.StoreHauled())
                    return false;
            }
            else
            {
                if (amount == obj.StackSize)
                {
                    //parent.Net.Despawn(obj);// i despawn the obj in the next method below
                    this.Haul(obj);
                    return true;
                }
                else
                {
                    if (parent.Net is Server server)
                    {
                        var newobj = obj.Clone();
                        newobj.StackSize = amount;
                        newobj.SyncInstantiate(parent.Net);
                        obj.StackSize -= amount;
                        this.Haul(newobj);
                        Packets.SyncSetHaulSlot(server, parent as Actor, newobj as Entity);
                    }
                    return true;
                }
            }
            return false;
        }
      
        public bool Unequip(GameObject item)
        {
            var slot = GearComponent.GetSlot(this.Parent, item);
            return this.Receive(slot);
        }
        public bool Receive(GameObjectSlot objSlot, bool report = true)
        {
            // TODO: if can't receive, haul item instead or drop on ground?
            var obj = objSlot.Object;
            var parent = this.Parent;
            if(this.Slots.InsertObject(objSlot))
            {
                if(report)
                    parent.Net.EventOccured(Message.Types.ItemGot, parent, obj);
                return true;
            }
            // TODO: drop object if can't receive? here? or let whoever called this method do something else if it fails?
            return false;
        }
        
        public IEnumerable<Entity> GetItems()
        {
            foreach (var sl in this.Slots.Slots)
                if (sl.HasValue)
                    yield return sl.Object as Entity;
        }
        public IEnumerable<Entity> All => this.GetItems();
       
        public GameObject First(Func<GameObject, bool> filter)
        {
            foreach (var slot in this.Slots.Slots)
                if (slot.Object != null)
                    if (filter(slot.Object))
                        return slot.Object;
            if (this.HaulSlot.Object != null && filter(this.HaulSlot.Object))
                return this.HaulSlot.Object;
            return null;
        }
        public int Count(ItemDef def, Material mat)
        {
            return this.Count(e => e.Def == def && e.PrimaryMaterial == mat);
        }
        public int Count(Func<Entity, bool> filter)
        {
            return this.FindItems(filter).Sum(i => i.StackSize);
            
        }
        public bool Contains(GameObject item)
        {
            return this.Slots.Slots.FirstOrDefault(s => s.Object == item) != null;
        }
        public bool Contains(Func<GameObject, bool> filter)// Predicate<GameObject> filter)
        {
            return (from slot in this.Slots.Slots
                        where slot.HasValue
                        where filter(slot.Object)
                        select slot).FirstOrDefault() != null;
        }
        public bool Equip(GameObject item)
        {
            foreach (var slot in this.Slots.Slots)
                if (slot.Object == item)
                    return GearComponent.Equip(this.Parent, slot);
            return false;
        }
        public bool CheckWeight(GameObject obj)
        {
            return true;
        }

        public bool Haul(GameObject obj)
        {
            if (obj is null)
                return true;
            var parent = this.Parent;
            var current = this.HaulSlot.Object;

            if (obj == current)
                return true;
            if (!this.CheckWeight(obj))
                return true;
            var net = parent.Net;
            // if currently hauling object of same type, increase held stacksize and dispose other object
            if (current != null)
                if(current.CanAbsorb(obj))
                {
                    current.StackSize++;
                    obj.Despawn();
                    net.DisposeObject(obj);
                    return true;
                }
            
            this.Throw(Vector3.Zero, parent); //or store carried object in backpack? (if available)

            net.Despawn(obj);
            this.HaulSlot.Object = obj;
            return true;
        }
        
        public bool Throw(Vector3 velocity, GameObject parent, bool all = false)
        {
            return this.Throw(parent.Net, velocity, parent, all);
        }
        public bool Throw(GameObject parent, Vector3 direction, bool all = false)
        {
            Vector3 velocity = direction * 0.1f + parent.Velocity;
            return this.Throw(parent.Net, velocity, parent, all);
        }
        bool Throw(IObjectProvider net, Vector3 velocity, GameObject parent, bool all)
        {
            // throws hauled object, if hauling nothing throws equipped object, make it so it only throws hauled object?
            
            var slot = this.HaulSlot;
            GameObjectSlot hauling = slot;// this.Slot;
            if (hauling.Object == null)
                return false;
            GameObject newobj = all ? hauling.Object : hauling.Take();

            newobj.Global = parent.Global + new Vector3(0, 0, parent.Physics.Height);
            newobj.Velocity = velocity;
            newobj.Physics.Enabled = true;
            net.Spawn(newobj);

            if (all)
                hauling.Clear();
            return true;
        }

        public List<GameObjectSlot> GetContents()
        {
            var actor = this.HaulSlot.Parent;
            var inv = actor.GetComponent<PersonalInventoryComponent>();
            var list = new List<GameObjectSlot>();
            list.Add(inv.HaulSlot);
            foreach (var c in inv.Slots.Slots)
                list.Add(c);
            return list;
        }
        public IEnumerable<ObjectAmount> Take(Func<Entity, bool> filter, int amount)
        {
            var remaining = amount;

            var e = this.FindItems(filter).GetEnumerator();
            while (e.MoveNext() && remaining>0)
            {
                var i = e.Current;
                var amountToReturn = Math.Min(i.StackSize, remaining);
                remaining -= amountToReturn;
                yield return new ObjectAmount(i, amountToReturn);
            }
        }
        public override object Clone()
        {
            var comp = new PersonalInventoryComponent((byte)this.Slots.Slots.Count);

            using BinaryWriter w = new BinaryWriter(new MemoryStream());
            using BinaryReader r = new BinaryReader(w.BaseStream);

            this.Write(w);
            w.BaseStream.Position = 0;
            comp.Read(r);
            return comp;
        }

        public override void Write(BinaryWriter writer)
        {
            this.Slots.Write(writer);
            this.HaulSlot.Write(writer);
        }
        public override void Read(BinaryReader reader)
        {
            this.Slots.Read(reader);
            this.HaulSlot.Read(reader);
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            data.Add(new SaveTag(SaveTag.Types.Compound, "Inventory", this.Slots.Save()));
            var isHauling = this.HaulSlot.Object != null;
            data.Add(new SaveTag(SaveTag.Types.Bool, "IsHauling", isHauling));
            if (isHauling)
            data.Add(new SaveTag(SaveTag.Types.Compound, "Hauling", this.HaulSlot.Save()));

            return data;
        }
        internal override void Load(SaveTag data)
        {
            data.TryGetTag("Inventory", tag => this.Slots.Load(tag));
            bool isHauling;
            if (data.TryGetTagValue("IsHauling", out isHauling))
                if (isHauling)
                    data.TryGetTag("Hauling", tag => this.HaulSlot.Load(tag));
        }

        internal static void DropHauled(GameObject parent)
        {
            var inv = parent.GetComponent<PersonalInventoryComponent>();
            if (inv == null)
                return;
            inv.Throw(parent, Vector3.Zero);
        }

        public override string ToString()
        {
            var text = base.ToString() + 
                '\n' + this.Slots.ToStringFull() +
                '\n' + this.HaulContainer.ToStringFull();
                ;
                return text;
        }

        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(new Label() { TextFunc = () => string.Format("Carrying: {0}", this.HaulSlot.Object != null ? this.HaulSlot.Object.ToString() : "Nothing") });
        }

        public IEnumerable<Entity> FindItems(Func<Entity, bool> p)
        {
            foreach (var s in this.Slots.Slots)
            {
                if (s.Object is not Entity e)
                    continue;
                if (p(e))
                    yield return e;
            }
        }
        public IEnumerable<GameObjectSlot> GetEmptySlots()
        {
            return this.Slots.GetEmpty();
        }
       
    }
}
