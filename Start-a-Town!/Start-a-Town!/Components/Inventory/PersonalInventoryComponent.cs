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

                static void handleSetHaulSlot(INetwork net, BinaryReader r)
                {
                    var actor = net.GetNetworkObject(r.ReadInt32()) as Actor;
                    var item = net.GetNetworkObject(r.ReadInt32()) as Entity;
                    actor.Carry(item);
                }

                PacketSetHaulSlot = Network.RegisterPacketHandler(handleSetHaulSlot);
            }

            public static void SendSyncInsert(INetwork net, Actor actor, Entity item)
            {
                if (net is Server)
                    actor.Inventory.Insert(item);
                net.GetOutgoingStream().Write(PacketSyncInsert, actor.RefID, item.RefID);
            }
            private static void HandleSyncInsert(INetwork net, BinaryReader r)
            {
                var actor = net.GetNetworkObject(r.ReadInt32()) as Actor;
                var item = net.GetNetworkObject(r.ReadInt32()) as Entity;
                if (net is Server)
                    SendSyncInsert(net, actor, item);
                else
                    actor.Inventory.Insert(item);
            }

            public static void SyncSetHaulSlot(INetwork net, Actor actor, Entity item)
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

        public int Capacity = 16;//this.Slots.Slots.Count;
        public float PercentageEmpty => this.Slots.Count / (float)this.Capacity;
        public float PercentageFull => 1 - this.PercentageEmpty;
        public bool HasFreeSpace => this.PercentageEmpty < 1;

        readonly Container HaulContainer;
        readonly GameObjectSlot HaulSlot;
        //public Container Slots;
        public ContainerList Slots = new();

        internal void Remove(Entity obj)
        {
            this.Slots.Remove(obj);
        }
        internal void Remove(GameObject obj)
        {
            this.Remove(obj as Entity);
        }

        internal void SyncInsert(GameObject split)
        {
            var actor = this.Parent as Actor;
            var net = actor.Net;
            if (net is not Server server)
                throw new Exception();
            Packets.SendSyncInsert(net, actor, split as Entity);
        }

        public override string ComponentName => "PersonalInventory";


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

            this.Slots.Parent = parent;
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
            //list.Add(this.Slots);
        }
        public PersonalInventoryComponent()
            : base()
        {
            this.Parent = null;
            this.HaulContainer = new Container(1) { Name = "Hauling" };
            this.HaulSlot = this.HaulContainer.Slots.First();
        }
        public PersonalInventoryComponent(byte capacity)
            : this()
        {
            this.Capacity = capacity;
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
        public GameObject Drop(GameObject item, int amount)
        {
            var parent = this.Parent;
            var slot = this.Slots.First(i => i == item);
            // TODO instantiate new item if necessary
            if (amount < item.StackSize)
            {
                //obj = slot.Object.Clone();
                //obj.StackSize = amount;
            }
            item.Spawn(parent.Map, parent.Global + new Vector3(0, 0, parent.Physics.Height));
            //item.StackSize -= amount;
            return item;
        }
        public void Drop(GameObject item)
        {
            var parent = this.Parent;
            item.Spawn(parent.Map, parent.Global + new Vector3(0, 0, parent.Physics.Height));
        }
        public void HaulFromInventory(GameObject obj, int amount = -1)
        {
            if (amount == 0)
                throw new Exception();
            amount = amount == -1 ? obj.StackSize : amount;
            var slots = this.Slots;
            var parent = this.Parent;
            var currentAmount = obj.StackSize;
            if (amount > currentAmount)
                throw new Exception();
            else if (amount == currentAmount)
            {
                this.Haul(obj);
            }
            else if (amount < currentAmount)
            {
                obj.StackSize -= amount;
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
                (parent as Actor).Work.Perform(new Interactions.EquipFromInventory(), new TargetArgs(slot));
            else
            {
                this.HaulSlot.Swap(slot);
            }
        }

        public bool StoreHauled()
        {
            if (this.HaulSlot.Object == null)
                return false;
            this.Slots.Add(this.HaulSlot.Object);
            //{
            //    // throw? or return false and raise event so we can handle it and display a message : not enough space?
            //    //inv.Throw(parent, Vector3.Zero);
            //    this.Parent.Net.EventOccured(Message.Types.NotEnoughSpace, this.Parent);
            //    return false;
            //}

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
            if (obj == null)
                return false;
            this.Slots.Add(obj);

            //if (!this.Slots.Insert(obj))
            //{
            //    // throw? or return false and raise event so we can handle it and display a message : not enough space?
            //    //inv.Throw(parent, Vector3.Zero);
            //    actor.Net.EventOccured(Message.Types.NotEnoughSpace, actor);
            //    return false;
            //}
            return true;
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
                    /// NO NEED FOR THE BELOW, it's handled by the gameobject.stacksize property
                    //if (transferAmount == obj.StackSize)
                    //{
                    //    obj.Despawn();
                    //    parent.Net.DisposeObject(obj);
                    //}
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
            var slot = (this.Parent as Entity).Gear.GetSlot(item);
            return this.Receive(slot);
        }
        public bool Receive(GameObjectSlot objSlot, bool report = true)
        {
            // TODO: if can't receive, haul item instead or drop on ground?
            var obj = objSlot.Object as Entity;
            var parent = this.Parent;
            this.Slots.Add(obj);
            objSlot.Clear();
            if (report)
                parent.Net.EventOccured(Message.Types.ItemGot, parent, obj);
            return true;
            // TODO: drop object if can't receive? here? or let whoever called this method do something else if it fails?
        }

        public IEnumerable<Entity> GetItems()
        {
            foreach (var sl in this.Slots)
                yield return sl as Entity;
        }
        public IEnumerable<Entity> All => this.GetItems();

        public GameObject First(Func<GameObject, bool> filter)
        {
            foreach (var slot in this.Slots)
                if (filter(slot))
                    return slot;
            if (this.HaulSlot.Object != null && filter(this.HaulSlot.Object))
                return this.HaulSlot.Object;
            return null;
        }
        public int Count(ItemDef def)
        {
            return this.Count(e => e.Def == def);
        }
        public int Count(ItemDef def, MaterialDef mat)
        {
            return this.Count(e => e.Def == def && e.PrimaryMaterial == mat);
        }
        public int Count(Func<Entity, bool> filter)
        {
            return this.FindItems(filter).Sum(i => i.StackSize);

        }
        public bool Contains(GameObject item)
        {
            return this.Slots.FirstOrDefault(s => s == item) != null;
        }
        public bool Contains(Func<GameObject, bool> filter)// Predicate<GameObject> filter)
        {
            return (from slot in this.Slots
                    where filter(slot)
                    select slot).FirstOrDefault() != null;
        }
        public bool Equip(GameObject item)
        {
            foreach (var slot in this.Slots)
                if (slot == item)
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
                if (current.CanAbsorb(obj))
                {
                    current.StackSize++;
                    obj.Despawn();
                    net.DisposeObject(obj);
                    return true;
                }

            this.Throw(Vector3.Zero, true); //or store carried object in backpack? (if available)

            obj.Despawn();
            this.HaulSlot.Object = obj;
            return true;
        }

        public bool Throw(Vector3 direction, bool all = false)
        {
            var parent = this.Parent;
            var velocity = direction * 0.1f + parent.Velocity;
            // throws hauled object, if hauling nothing throws equipped object, make it so it only throws hauled object?
            var slot = this.HaulSlot;
            if (slot.Object == null)
                return false;
            GameObject newobj;
            if (!all && slot.Object.StackSize > 1)
            {
                newobj = slot.Object.Clone();
                newobj.StackSize = 1;
                slot.Object.StackSize -= 1;
            }
            else
                newobj = slot.Object;
            // TODO instantiate new obj as necessary
            newobj.Global = parent.Global + new Vector3(0, 0, parent.Physics.Height);
            newobj.Velocity = velocity;
            newobj.Physics.Enabled = true;
            newobj.SyncSpawnNew(parent.Map);

            if (all)
                slot.Clear();
            return true;
        }

        public IEnumerable<ObjectAmount> Take(Func<Entity, bool> filter, int amount)
        {
            var remaining = amount;

            var e = this.FindItems(filter).GetEnumerator();
            while (e.MoveNext() && remaining > 0)
            {
                var i = e.Current;
                var amountToReturn = Math.Min(i.StackSize, remaining);
                remaining -= amountToReturn;
                yield return new ObjectAmount(i, amountToReturn);
            }
        }
        public override object Clone()
        {
            var comp = new PersonalInventoryComponent((byte)this.Capacity);

            using BinaryWriter w = new(new MemoryStream());
            using BinaryReader r = new(w.BaseStream);

            this.Write(w);
            w.BaseStream.Position = 0;
            comp.Read(r);
            return comp;
        }

        public override void Write(BinaryWriter w)
        {
            this.Slots.Write(w);
            this.HaulSlot.Write(w);
        }
        public override void Read(BinaryReader r)
        {
            this.Slots.Read(r);
            this.HaulSlot.Read(r);
        }

        internal override List<SaveTag> Save()
        {
            var data = new List<SaveTag>();
            data.Add(this.Slots.Save("Contents"));
            var isHauling = this.HaulSlot.Object != null;
            data.Add(new SaveTag(SaveTag.Types.Bool, "IsHauling", isHauling));
            if (isHauling)
                data.Add(new SaveTag(SaveTag.Types.Compound, "Hauling", this.HaulSlot.Save()));

            return data;
        }
        internal override void Load(SaveTag data)
        {
            var container = new Container(16);
            if(!data.TryGetTag("Contents", t => this.Slots.Load(t)))
            {
                var tmpslots = new Container(16);
                data.TryGetTag("Inventory", tag => tmpslots.Load(tag));

                /// temp
                foreach (var i in tmpslots.Slots.Where(s => s.HasValue).Select(s => s.Object))
                    this.Slots.Add(i);
            }
            if (data.TryGetTagValue("IsHauling", out bool isHauling) && isHauling)
                data.TryGetTag("Hauling", tag => this.HaulSlot.Load(tag));
        }

        public override string ToString()
        {
            var text = base.ToString() +
                //'\n' + this.Slots.ToStringFull() +
                '\n' + this.HaulContainer.ToStringFull();
            ;
            return text;
        }

        readonly Label CachedGuiLabelCarrying = new();
        internal override void GetSelectionInfo(IUISelection info, GameObject parent)
        {
            info.AddInfo(this.CachedGuiLabelCarrying.SetTextFunc(() => $"Carrying: {this.HaulSlot.Object?.ToString() ?? "Nothing"}"));
        }

        public IEnumerable<Entity> FindItems(Func<Entity, bool> p)
        {
            foreach (var s in this.Slots)
            {
                if (s is not Entity e)
                    continue;
                if (p(e))
                    yield return e;
            }
        }
        //public IEnumerable<GameObjectSlot> GetEmptySlots()
        //{
        //    return this.Slots.GetEmpty();
        //}
        public override void Instantiate(Action<GameObject> instantiator)
        {
            this.Slots.Instantiate(instantiator);
        }
    }
    //public class PersonalInventoryComponent : EntityComponent
    //{
    //    class Packets
    //    {
    //        static int PacketSyncInsert, PacketSetHaulSlot;
    //        static public void Init()
    //        {
    //            PacketSyncInsert = Network.RegisterPacketHandler(HandleSyncInsert);

    //            static void handleSetHaulSlot(INetwork net, BinaryReader r)
    //            {
    //                var actor = net.GetNetworkObject(r.ReadInt32()) as Actor;
    //                var item = net.GetNetworkObject(r.ReadInt32()) as Entity;
    //                actor.Carry(item);
    //            }

    //            PacketSetHaulSlot = Network.RegisterPacketHandler(handleSetHaulSlot);
    //        }

    //        public static void SendSyncInsert(INetwork net, Actor actor, Entity item)
    //        {
    //            if(net is Server)
    //                actor.Inventory.Insert(item);
    //            net.GetOutgoingStream().Write(PacketSyncInsert, actor.RefID, item.RefID);
    //        }
    //        private static void HandleSyncInsert(INetwork net, BinaryReader r)
    //        {
    //            var actor = net.GetNetworkObject(r.ReadInt32()) as Actor;
    //            var item = net.GetNetworkObject(r.ReadInt32()) as Entity;
    //            if (net is Server)
    //                SendSyncInsert(net, actor, item);
    //            else
    //                actor.Inventory.Insert(item);
    //        }

    //        public static void SyncSetHaulSlot(INetwork net, Actor actor, Entity item)
    //        {
    //            var server = net as Server;
    //            var w = server.GetOutgoingStream();
    //            w.Write(PacketSetHaulSlot);
    //            w.Write(actor.RefID);
    //            w.Write(item.RefID);
    //        }
    //    }
    //    static PersonalInventoryComponent()
    //    {
    //        Packets.Init();
    //    }

    //    public int Capacity => this.Slots.Slots.Count;
    //    public float PercentageEmpty => this.GetEmptySlots().Count() / (float)this.Capacity;
    //    public float PercentageFull => 1 - this.PercentageEmpty;
    //    readonly Container HaulContainer;
    //    readonly GameObjectSlot HaulSlot;
    //    public Container Slots;
    //    public ContainerList Contents = new();

    //    internal void Remove(Entity obj)
    //    {
    //        this.Slots.Remove(obj);
    //    }
    //    internal void Remove(GameObject obj)
    //    {
    //        this.Remove(obj as Entity);
    //    }

    //    internal void SyncInsert(GameObject split)
    //    {
    //        var actor = this.Parent as Actor;
    //        var net = actor.Net;
    //        if (net is not Server server)
    //            throw new Exception();
    //        Packets.SendSyncInsert(net, actor, split as Entity);
    //    }

    //    public override string ComponentName => "PersonalInventory";

    //    public bool HasFreeSpace => this.GetEmptySlots().Any();

    //    public float Distance(GameObject obj1, GameObject obj2)
    //    {
    //        return obj1.Inventory.Contains(obj => obj == obj2) ? 0 : -1;
    //    }
    //    public Vector3? DistanceVector(GameObject obj1, GameObject obj2)
    //    {
    //        return obj1.Inventory.Contains(obj => obj == obj2) ? Vector3.Zero : null;
    //    }

    //    public override void MakeChildOf(GameObject parent)
    //    {
    //        this.Parent = parent;

    //        parent.RegisterContainer(this.Slots);
    //        parent.RegisterContainer(this.HaulContainer);
    //    }
    //    public GameObjectSlot GetHauling()
    //    {
    //        return this.HaulSlot;
    //    }
    //    public static GameObjectSlot GetHauling(GameObject parent)
    //    {
    //        return parent.GetComponent<PersonalInventoryComponent>().GetHauling();
    //    }

    //    public override void GetContainers(List<Container> list)
    //    {
    //        list.Add(this.HaulContainer);
    //        //list.Add(this.Slots);
    //    }
    //    public PersonalInventoryComponent()
    //        : base()
    //    {
    //        this.Parent = null;
    //        this.Slots = new Container() { Name = "Inventory" };
    //        this.HaulContainer = new Container(1) { Name = "Hauling" };
    //        this.HaulSlot = this.HaulContainer.Slots.First();
    //    }
    //    public PersonalInventoryComponent(byte capacity)
    //        : this()
    //    {
    //        this.Slots = new Container(capacity)
    //        {
    //            Name = "Inventory"

    //        };
    //    }

    //    public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
    //    {
    //        switch (e.Type)
    //        {
    //                // TODO: maybe create a new message called InventoryInteraction that individual components can respond too?
    //            case Message.Types.SlotInteraction:
    //                this.SlotInteraction(parent, e.Parameters[0] as GameObject, e.Parameters[1] as GameObjectSlot);
    //                return true;

    //            default:
    //                return false;
    //        }
    //    }
    //    public GameObject Drop(GameObject item, int amount)
    //    {

    //        var parent = this.Parent;
    //        var slot = this.Slots.Slots.First(i => i.Object == item);
    //        // TODO instantiate new item if necessary
    //        throw new NotImplementedException();
    //        if (amount < slot.Object.StackSize)
    //        {
    //            //obj = slot.Object.Clone();
    //            //obj.StackSize = amount;
    //        }
    //        item.Spawn(parent.Map, parent.Global + new Vector3(0, 0, parent.Physics.Height));
    //        item.StackSize -= amount;
    //        return item;
    //    }
    //    public void Drop(GameObject item)
    //    {
    //        var slots = this.Slots;
    //        var parent = this.Parent;
    //        var slot = slots.Slots.Where(s => s.Object == item).First();
    //        slot.Clear();
    //        item.Spawn(parent.Map, parent.Global + new Vector3(0, 0, parent.Physics.Height));
    //    }
    //    public void HaulFromInventory(GameObject item, int amount = -1)
    //    {
    //        if (amount == 0)
    //            throw new Exception();
    //        amount = amount == -1 ? item.StackSize : amount;
    //        var slots = this.Slots;
    //        var parent = this.Parent;
    //        var slot = slots.Slots.Where(s => s.Object == item).First();
    //        var obj = slot.Object;
    //        var currentAmount = obj.StackSize;
    //        if (amount > currentAmount)
    //            throw new Exception();
    //        else if(amount == currentAmount)
    //        {
    //            this.Haul(item);
    //        }
    //        else if(amount < currentAmount)
    //        {
    //            slot.Object.StackSize -= amount;
    //            if (parent.Net is Server server)
    //            {
    //                var splitItem = obj.Clone();
    //                splitItem.StackSize = amount;
    //                splitItem.SyncInstantiate(server);
    //                Packets.SyncSetHaulSlot(server, parent as Actor, splitItem as Entity);
    //                this.Haul(splitItem);
    //            }
    //        }
    //    }

    //    void SlotInteraction(GameObject parent, GameObject actor, GameObjectSlot slot)
    //    {
    //        if (!slot.HasValue)
    //        {
    //            // if right clicking an empty slot, put player's hauled item in it
    //            var hauled = PersonalInventoryComponent.GetHauling(actor);
    //            if (hauled.HasValue)
    //                hauled.Swap(slot);
    //            return;
    //        }
    //        var obj = slot.Object;

    //        if (obj.HasComponent<EquipComponent>())
    //            (parent as Actor).Work.Perform(new Interactions.EquipFromInventory(), new TargetArgs(slot));
    //        else
    //        {
    //            this.HaulSlot.Swap(slot);
    //        }
    //    }

    //    public bool StoreHauled()
    //    {
    //        if (this.HaulSlot.Object == null)
    //            return false;
    //        if (!this.Slots.Insert(this.HaulSlot))

    //        {
    //            // throw? or return false and raise event so we can handle it and display a message : not enough space?
    //            //inv.Throw(parent, Vector3.Zero);
    //            this.Parent.Net.EventOccured(Message.Types.NotEnoughSpace, this.Parent);
    //            return false;
    //        }


    //        //NpcComponent.AddPossesion(parent, obj); // why was i adding the item as a possesion here? the item becomes a possesion during ownership assignment
    //                                                    // BECAUSE i want npc to claim ownership when picking up and storing ie. food in their inventory
    //                                                    // but other problems arise if i set ownership here

    //        return true;
    //    }
    //    public bool Insert(GameObject obj)
    //    {
    //        return this.Insert(obj as Entity);
    //    }
    //    public bool Insert(Entity obj)
    //    {
    //        var actor = this.Parent as Actor;
    //        if (obj == null)
    //            return false;
    //        if (!this.Slots.Insert(obj))

    //        {
    //            // throw? or return false and raise event so we can handle it and display a message : not enough space?
    //            //inv.Throw(parent, Vector3.Zero);
    //            actor.Net.EventOccured(Message.Types.NotEnoughSpace, actor);
    //            return false;
    //        }
    //        return true;
    //    }

    //    public bool PickUp(GameObject obj, int amount)
    //    {
    //        var parent = this.Parent;
    //        if (this.HaulSlot.Object is GameObject currentHauled)
    //        {
    //            if (currentHauled.CanAbsorb(obj))
    //            {
    //                var transferAmount = Math.Min(amount == -1 ? obj.StackSize : amount, currentHauled.StackMax - currentHauled.StackSize);
    //                if (transferAmount == 0)
    //                    throw new Exception();
    //                currentHauled.StackSize += transferAmount;
    //                if (transferAmount == obj.StackSize)
    //                {
    //                    obj.Despawn();
    //                    parent.Net.DisposeObject(obj);
    //                }
    //                obj.StackSize -= transferAmount;
    //                return false;
    //            }
    //            //else
    //            // if we maxed out our hauling stack, but there is some remaining amout of item on the ground, store hauled stack and haul the remaining item stack
    //            else if (!this.StoreHauled())
    //                return false;
    //        }
    //        else
    //        {
    //            if (amount == obj.StackSize)
    //            {
    //                //parent.Net.Despawn(obj);// i despawn the obj in the next method below
    //                this.Haul(obj);
    //                return true;
    //            }
    //            else
    //            {
    //                if (parent.Net is Server server)
    //                {
    //                    var newobj = obj.Clone();
    //                    newobj.StackSize = amount;
    //                    newobj.SyncInstantiate(parent.Net);
    //                    obj.StackSize -= amount;
    //                    this.Haul(newobj);
    //                    Packets.SyncSetHaulSlot(server, parent as Actor, newobj as Entity);
    //                }
    //                return true;
    //            }
    //        }
    //        return false;
    //    }

    //    public bool Unequip(GameObject item)
    //    {
    //        var slot = GearComponent.GetSlot(this.Parent, item);
    //        return this.Receive(slot);
    //    }
    //    public bool Receive(GameObjectSlot objSlot, bool report = true)
    //    {
    //        // TODO: if can't receive, haul item instead or drop on ground?
    //        var obj = objSlot.Object;
    //        var parent = this.Parent;
    //        if(this.Slots.Insert(objSlot))
    //        {
    //            if(report)
    //                parent.Net.EventOccured(Message.Types.ItemGot, parent, obj);
    //            return true;
    //        }
    //        // TODO: drop object if can't receive? here? or let whoever called this method do something else if it fails?
    //        return false;
    //    }

    //    public IEnumerable<Entity> GetItems()
    //    {
    //        foreach (var sl in this.Slots.Slots)
    //            if (sl.HasValue)
    //                yield return sl.Object as Entity;
    //    }
    //    public IEnumerable<Entity> All => this.GetItems();

    //    public GameObject First(Func<GameObject, bool> filter)
    //    {
    //        foreach (var slot in this.Slots.Slots)
    //            if (slot.Object != null)
    //                if (filter(slot.Object))
    //                    return slot.Object;
    //        if (this.HaulSlot.Object != null && filter(this.HaulSlot.Object))
    //            return this.HaulSlot.Object;
    //        return null;
    //    }
    //    public int Count(ItemDef def)
    //    {
    //        return this.Count(e => e.Def == def);
    //    }
    //    public int Count(ItemDef def, MaterialDef mat)
    //    {
    //        return this.Count(e => e.Def == def && e.PrimaryMaterial == mat);
    //    }
    //    public int Count(Func<Entity, bool> filter)
    //    {
    //        return this.FindItems(filter).Sum(i => i.StackSize);

    //    }
    //    public bool Contains(GameObject item)
    //    {
    //        return this.Slots.Slots.FirstOrDefault(s => s.Object == item) != null;
    //    }
    //    public bool Contains(Func<GameObject, bool> filter)// Predicate<GameObject> filter)
    //    {
    //        return (from slot in this.Slots.Slots
    //                    where slot.HasValue
    //                    where filter(slot.Object)
    //                    select slot).FirstOrDefault() != null;
    //    }
    //    public bool Equip(GameObject item)
    //    {
    //        foreach (var slot in this.Slots.Slots)
    //            if (slot.Object == item)
    //                return GearComponent.Equip(this.Parent, slot);
    //        return false;
    //    }
    //    public bool CheckWeight(GameObject obj)
    //    {
    //        return true;
    //    }

    //    public bool Haul(GameObject obj)
    //    {
    //        if (obj is null)
    //            return true;
    //        var parent = this.Parent;
    //        var current = this.HaulSlot.Object;

    //        if (obj == current)
    //            return true;
    //        if (!this.CheckWeight(obj))
    //            return true;
    //        var net = parent.Net;
    //        // if currently hauling object of same type, increase held stacksize and dispose other object
    //        if (current != null)
    //            if(current.CanAbsorb(obj))
    //            {
    //                current.StackSize++;
    //                obj.Despawn();
    //                net.DisposeObject(obj);
    //                return true;
    //            }

    //        this.Throw(Vector3.Zero, true); //or store carried object in backpack? (if available)

    //        obj.Despawn();
    //        this.HaulSlot.Object = obj;
    //        return true;
    //    }

    //    public bool Throw(Vector3 direction, bool all = false)
    //    {
    //        var parent = this.Parent;
    //        var velocity = direction * 0.1f + parent.Velocity;
    //        // throws hauled object, if hauling nothing throws equipped object, make it so it only throws hauled object?
    //        var slot = this.HaulSlot;
    //        if (slot.Object == null)
    //            return false;
    //        GameObject newobj;
    //        if (!all && slot.Object.StackSize > 1)
    //        {
    //            newobj = slot.Object.Clone();
    //            newobj.StackSize = 1;
    //            slot.Object.StackSize -= 1;
    //        }
    //        else
    //            newobj = slot.Object;
    //        // TODO instantiate new obj as necessary
    //        newobj.Global = parent.Global + new Vector3(0, 0, parent.Physics.Height);
    //        newobj.Velocity = velocity;
    //        newobj.Physics.Enabled = true;
    //        newobj.SyncSpawnNew(parent.Map);

    //        if (all)
    //            slot.Clear();
    //        return true;
    //    }

    //    public IEnumerable<ObjectAmount> Take(Func<Entity, bool> filter, int amount)
    //    {
    //        var remaining = amount;

    //        var e = this.FindItems(filter).GetEnumerator();
    //        while (e.MoveNext() && remaining>0)
    //        {
    //            var i = e.Current;
    //            var amountToReturn = Math.Min(i.StackSize, remaining);
    //            remaining -= amountToReturn;
    //            yield return new ObjectAmount(i, amountToReturn);
    //        }
    //    }
    //    public override object Clone()
    //    {
    //        var comp = new PersonalInventoryComponent((byte)this.Slots.Slots.Count);

    //        using BinaryWriter w = new(new MemoryStream());
    //        using BinaryReader r = new(w.BaseStream);

    //        this.Write(w);
    //        w.BaseStream.Position = 0;
    //        comp.Read(r);
    //        return comp;
    //    }

    //    public override void Write(BinaryWriter w)
    //    {
    //        this.Contents.Write(w);
    //        //this.Slots.Write(w);
    //        this.HaulSlot.Write(w);
    //    }
    //    public override void Read(BinaryReader r)
    //    {
    //        //this.Slots.Read(r);
    //        this.Contents.Read(r);
    //        this.HaulSlot.Read(r);
    //    }

    //    internal override List<SaveTag> Save()
    //    {
    //        List<SaveTag> data = new List<SaveTag>();
    //        data.Add(new SaveTag(SaveTag.Types.Compound, "Inventory", this.Slots.Save()));
    //        var isHauling = this.HaulSlot.Object != null;
    //        data.Add(new SaveTag(SaveTag.Types.Bool, "IsHauling", isHauling));
    //        if (isHauling)
    //        data.Add(new SaveTag(SaveTag.Types.Compound, "Hauling", this.HaulSlot.Save()));

    //        return data;
    //    }
    //    internal override void Load(SaveTag data)
    //    {
    //        data.TryGetTag("Inventory", tag => this.Slots.Load(tag));

    //        /// temp
    //        foreach (var i in this.GetItems())
    //            this.Contents.Add(i);

    //        bool isHauling;
    //        if (data.TryGetTagValue("IsHauling", out isHauling))
    //            if (isHauling)
    //                data.TryGetTag("Hauling", tag => this.HaulSlot.Load(tag));
    //    }

    //    public override string ToString()
    //    {
    //        var text = base.ToString() + 
    //            '\n' + this.Slots.ToStringFull() +
    //            '\n' + this.HaulContainer.ToStringFull();
    //            ;
    //            return text;
    //    }

    //    readonly Label CachedGuiLabelCarrying = new();
    //    internal override void GetSelectionInfo(IUISelection info, GameObject parent)
    //    {
    //        info.AddInfo(this.CachedGuiLabelCarrying.SetTextFunc(() => $"Carrying: {this.HaulSlot.Object?.ToString() ?? "Nothing"}"));
    //    }

    //    public IEnumerable<Entity> FindItems(Func<Entity, bool> p)
    //    {
    //        foreach (var s in this.Slots.Slots)
    //        {
    //            if (s.Object is not Entity e)
    //                continue;
    //            if (p(e))
    //                yield return e;
    //        }
    //    }
    //    public IEnumerable<GameObjectSlot> GetEmptySlots()
    //    {
    //        return this.Slots.GetEmpty();
    //    }
    //    public override void Instantiate(Action<GameObject> instantiator)
    //    {
    //        this.Contents.Instantiate(instantiator);
    //    }
    //}
}
