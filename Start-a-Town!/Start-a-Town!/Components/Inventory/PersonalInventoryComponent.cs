using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    public class PersonalInventoryComponent : EntityComponent, IObjectSpace
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
        
        public Container Slots { get { return (Container)this["Slots"]; } set { this["Slots"] = value; } }

        internal void SyncInsert(GameObject split)
        {
            var actor = this.Parent as Actor;
            var net = actor.NetNew;
            if (net is not Server server)
                throw new Exception();
            Packets.SendSyncInsert(net, actor, split as Entity);
        }

        public override string ComponentName
        {
            get
            {
                return "PersonalInventory";
            }
        }

        public bool HasEmptySpace => this.GetEmptySlots().Any();

        public float Distance(GameObject obj1, GameObject obj2)
        {
            return HasObject(obj1, obj => obj == obj2) ? 0 : -1;
        }
        public Vector3? DistanceVector(GameObject obj1, GameObject obj2)
        {
            return HasObject(obj1, obj => obj == obj2) ? Vector3.Zero : null;
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

                case Message.Types.Insert:
                    GameObjectSlot objSlot = e.Parameters[0] as GameObjectSlot;
                    return PickUp(parent, objSlot);

                //case Message.Types.Receive:
                //    GameObject obj = e.Parameters.Translate<SenderEventArgs>(e.Network).Sender;

                //    // TODO: this is a WORKAROUND
                //    // FAILSAFE in case player is picking items too fast resulting in picking the same item before the server processes the input
                //    GameObjectSlot existingSlot;
                //    if (HasObject(parent, o => o == obj, out existingSlot))
                //    {
                //        //parent.PostMessage(e.Network, Message.Types.Hold, w => w.Write(obj.NetworkID));
                //        return true;
                //    }
                //    GiveObject(e.Network, parent, obj.ToSlotLink());
                //    return true;

                case Message.Types.ArrangeInventory:
                    GameObjectSlot source = e.Parameters[0] as GameObjectSlot;
                    if (!source.HasValue)
                        return true;

                    int objSize = (int)source.Object["Physics"]["Size"];
                    if (objSize > 0)
                    {
                        // TODO: code to handle case where object is haulable
                        e.Network.PopLoot(source.Take(), parent.Global, parent.Velocity); // WARNING!
                        source.Clear();
                        return true;
                    }

                    GameObjectSlot target = e.Parameters[1] as GameObjectSlot;
                    int amount = (int)e.Parameters[2];
                    if (source.Object == target.Object)
                    {
                        int d = Math.Max(0, target.StackSize + amount - target.StackMax);
                        int a = amount - d;
                        target.StackSize += a;
                        source.StackSize -= a;
                        return true;
                    }
                    if (amount == source.StackSize)
                    {
                        source.Swap(target);
                        return true;
                    }
                    if (target.HasValue)
                        return true;
                    source.StackSize -= amount;
                    target.Object = source.Object;
                    target.StackSize = amount;
                    return true;

                case Message.Types.DropInventoryItem:
                    var slotID = (int)e.Parameters[0];
                    amount = (int)e.Parameters[1];
                    DropInventoryItem(parent, slotID, amount);
                    return true;

                default:
                    return false;
            }
        }
        public static void DropInventoryItem(GameObject parent, GameObject item)
        {
            var slots = GetSlots(parent);
            var slot = slots.Slots.Where(s => s.Object == item).First();
            slot.Clear();
            item.Spawn(parent.Map, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));
        }
        public static void HaulFromInventory(GameObject parent, GameObject item, int amount = -1)
        {
            if (amount == 0)
                throw new Exception();
            amount = amount == -1 ? item.StackSize : amount;
            var slots = GetSlots(parent);
            var slot = slots.Slots.Where(s => s.Object == item).First();
            var obj = slot.Object;
            var currentAmount = obj.StackSize;
            if (amount > currentAmount)
                throw new Exception();
            else if(amount == currentAmount)
            {
                parent.GetComponent<PersonalInventoryComponent>().Haul(parent, item);
            }
            else if(amount < currentAmount)
            {
                slot.Object.StackSize -= amount;
                if (parent.Net is Server server)
                {
                    var splitItem = obj.Clone();
                    splitItem.StackSize = amount;
                    server.SyncInstantiate(splitItem);
                    Packets.SyncSetHaulSlot(server, parent as Actor, splitItem as Entity);
                    parent.GetComponent<PersonalInventoryComponent>().Haul(parent, splitItem);
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

        internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.DropInventoryItem:
                    e.Data.Translate(e.Network, r =>
                    {
                        int slotID = (int)r.ReadByte();
                        int amount = r.ReadInt32();
                        GameObjectSlot slot = parent.GetChildren()[slotID];
                        GameObject obj;
                        if (amount < slot.Object.StackSize)
                        {
                            obj = slot.Object.Clone();
                            obj.StackSize = amount;
                        }
                        else
                            obj = slot.Object;
                        e.Network.Spawn(obj, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));
                        slot.StackSize -= amount;
                    });
                    return;
                default:
                    return;
            }
        }
        internal override void HandleRemoteCall(GameObject parent, Message.Types type, BinaryReader r)
        {
            switch (type)
            {
                case Message.Types.Haul:
                    var tohaulobj = parent.Net.GetNetworkObject(r.ReadInt32());
                    var sourceobj = parent.Net.GetNetworkObject(r.ReadInt32());
                    var hauledobj = this.HaulSlot.Object;
                    var amount = r.ReadInt32();
                    if (hauledobj != null)
                    {
                        if (hauledobj.IDType == tohaulobj.IDType)
                        {
                            var transferAmount = Math.Min(amount == -1 ? tohaulobj.StackSize : amount, hauledobj.StackMax - hauledobj.StackSize);
                            hauledobj.StackSize += transferAmount;
                            if (tohaulobj.StackSize == amount)
                            {
                                parent.Net.Despawn(tohaulobj);
                                parent.Net.DisposeObject(tohaulobj);
                                return; // if we didn't leave any amount left on the ground, return
                            }
                            else
                            tohaulobj.StackSize -= transferAmount;
                            break;
                        }
                        else if (!StoreHauled(parent))
                            return;
                    }
                    else
                    {
                        if (amount == sourceobj.StackSize)
                        {
                            //parent.Net.Despawn(sourceobj); // i call despawn in the haul method 
                        }
                        else
                            sourceobj.StackSize -= amount;
                        this.Haul(parent, tohaulobj);
                    }
                    break;

                default:
                    break;
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
                parent.GetComponent<WorkComponent>().Perform(parent, new Interactions.EquipFromInventory(), new TargetArgs(slot));
            else
            {
                this.HaulSlot.Swap(slot);
            }
        }

        static public bool HasItems(GameObject actor, Dictionary<GameObject.Types, int> items)
        {
            foreach (KeyValuePair<GameObject.Types, int> mat in items)
                if (InventoryComponent.GetAmount(actor, foo => foo.IDType == mat.Key) < mat.Value)
                    return false;
            return true;
        }

        static public bool StoreHauled(GameObject parent)
        {
            var inv = parent.GetComponent<PersonalInventoryComponent>();
            var obj = inv.HaulSlot.Object;
            if (inv.HaulSlot.Object == null)
                return false;
            if (!inv.Slots.InsertObject(inv.HaulSlot))

            {
                // throw? or return false and raise event so we can handle it and display a message : not enough space?
                //inv.Throw(parent, Vector3.Zero);
                parent.Net.EventOccured(Message.Types.NotEnoughSpace, parent);
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
        static public bool InsertItem(Actor actor, Entity obj)
        {
            return actor.Inventory.Insert(obj);
        }
        static public bool RemoveItem(Actor actor, Entity obj)
        {
            var inv = actor.GetComponent<PersonalInventoryComponent>();
            if (obj == null)
                return false;
            return inv.Slots.Remove(obj);
        }
        bool PickUpOld(GameObject parent, GameObjectSlot objSlot)
        {
            if (objSlot.Object == null)
                return true;
            var haulObj = this.HaulSlot.Object;
            if (haulObj != null)
            {
                
                if (!this.Slots.InsertObject(this.HaulSlot))
                    return false;
            }
            this.Haul(parent, objSlot);
            return true;
        }
       
        static public bool PickUp(GameObject parent, GameObjectSlot objSlot)
        {
            var inv = parent.GetComponent<PersonalInventoryComponent>();
            if (inv.HaulSlot.Object != null)
            {
                // check if item of same type, and if true, add to stack
                var haulObj = inv.HaulSlot.Object;
                if (haulObj.IDType == objSlot.Object.IDType)
                {
                    var transferAmount = Math.Min(objSlot.Object.StackSize, haulObj.StackMax - haulObj.StackSize);
                    haulObj.StackSize += transferAmount;
                    if (transferAmount == objSlot.Object.StackSize)
                    {
                        parent.Net.Despawn(objSlot.Object);
                        parent.Net.DisposeObject(objSlot.Object);
                        return true; // if we didn't leave any amount left on the ground, return
                    }
                }
                //else
                // if we maxed out our hauling stack, but there is some remaining amout of item on the ground, store hauled stack and haul the remaining item stack
                    if (!StoreHauled(parent))
                        return false;
            }
            return inv.PickUpOld(parent, objSlot);
        }
       
        static public bool PickUpNewNew(GameObject parent, GameObject obj, int amount)
        {
            if (parent.Net is Net.Client)
                return false;
            var server = parent.Net as Net.Server;
            var inv = parent.GetComponent<PersonalInventoryComponent>();
            if (inv.HaulSlot.Object != null)
            {
                // check if item of same type, and if true, add to stack
                var haulObj = inv.HaulSlot.Object;
                if(haulObj.CanAbsorb(obj))
                {
                    var transferAmount = Math.Min(amount == -1 ? obj.StackSize : amount, haulObj.StackMax - haulObj.StackSize);
                    if (transferAmount == 0)
                        throw new Exception();
                    haulObj.StackSize += transferAmount;
                    if (transferAmount == obj.StackSize)
                    {
                        parent.Net.Despawn(obj);
                        parent.Net.DisposeObject(obj);
                    }
                    obj.StackSize -= transferAmount;

                    (parent.Net as Net.Server).RemoteProcedureCall(new TargetArgs(parent), Message.Types.Haul, w => { w.Write(obj.RefID); w.Write(obj.RefID); w.Write(amount); });
                    return false;
                }
                //else
                // if we maxed out our hauling stack, but there is some remaining amout of item on the ground, store hauled stack and haul the remaining item stack
                else if (!StoreHauled(parent))
                    return false;
            }
            else
            {
                if (amount == obj.StackSize)
                {
                    //parent.Net.Despawn(obj);// i despawn the obj in the next method below
                    inv.Haul(parent, obj);
                    server.RemoteProcedureCall(new TargetArgs(parent), Message.Types.Haul, w => { w.Write(obj.RefID); w.Write(obj.RefID); w.Write(amount); });
                    return true;
                }
                else
                {
                    var newobj = obj.Clone();
                    newobj.StackSize = amount;
                    parent.Net.InstantiateAndSync(newobj);

                    if (server != null)
                    {
                        obj.StackSize -= amount;
                        inv.Haul(parent, newobj);
                        server.RemoteProcedureCall(new TargetArgs(parent), Message.Types.Haul, w => { w.Write(newobj.RefID); w.Write(obj.RefID); w.Write(amount); });
                    }
                    return true;
                }
            }
            return false;
        }
        static public bool Unequip(GameObject actor, GearType geartype)
        {
            var slot = GearComponent.GetSlot(actor, geartype);
            return Receive(actor, slot);
        }
        static public bool Unequip(GameObject actor, GameObject item)
        {
            var slot = GearComponent.GetSlot(actor, item);
            return Receive(actor, slot);
        }
        static public bool Receive(GameObject parent, GameObjectSlot objSlot, bool report = true)
        {
            var inv = parent.GetComponent<PersonalInventoryComponent>();
            // TODO: if can't receive, haul item instead or drop on ground?
            var obj = objSlot.Object;
            if(inv.Slots.InsertObject(objSlot))
            {
                if(report)
                    parent.Net.EventOccured(Message.Types.ItemGot, parent, obj);
                return true;
            }
            // TODO: drop object if can't receive? here? or let whoever called this method do something else if it fails?
            return false;
        }
        
        /// <summary>
        /// Returns true if empty slots are found in actor's inventory.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="emptySlots">A queue containing the empty slots found, if any.</param>
        /// <returns></returns>
        static public bool TryGetEmptySlots(GameObject actor, out Queue<GameObjectSlot> emptySlots)
        {
            if(!actor.TryGetComponent<InventoryComponent>("Inventory", out var invComp))
                throw (new Exception(actor.Name + " doesn't have an inventory component."));
            return invComp.TryGetEmptySlots(out emptySlots);
        }

        static public bool TryGetEmptySlots(GameObject actor, Queue<GameObjectSlot> emptySlots)
        {
            InventoryComponent invComp;
            if (!actor.TryGetComponent<InventoryComponent>("Inventory", out invComp))
                throw (new Exception(actor.Name + " doesn't have an inventory component."));
            return invComp.TryGetEmptySlots(emptySlots);
        }
        static public Container GetSlots(GameObject actor)
        {
            return actor.GetComponent<PersonalInventoryComponent>().Slots;
        }
        static public List<GameObject> GetAllItems(GameObject actor)
        {
            var slots = actor.GetComponent<PersonalInventoryComponent>().Slots;
            return slots.Slots.Where(s => s.Object != null).Select(s => s.Object).ToList();
        }
        public IEnumerable<Entity> GetItems()
        {
            foreach (var sl in this.Slots.Slots)
                if (sl.HasValue)
                    yield return sl.Object as Entity;
        }
        public IEnumerable<Entity> All => this.GetItems();
        /// <summary>
        /// Returns true if any slots meeting the conditions is found.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="filter"></param>
        /// <param name="slots"></param>
        /// <returns></returns>
        static public bool GetSlots(GameObject actor, Func<GameObject, bool> filter, Queue<GameObjectSlot> slots)
        {
            InventoryComponent invComp;
            if (!actor.TryGetComponent<InventoryComponent>("Inventory", out invComp))
                throw (new Exception(actor.Name + " doesn't have an inventory component."));
            foreach (var container in invComp.Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if (filter(slot.Object))
                            slots.Enqueue(slot);
            return slots.Count > 0;
        }
        static public GameObject GetFirstObject(GameObject actor, Func<GameObject, bool> filter)
        {
            var comp = actor.GetComponent<PersonalInventoryComponent>();
            foreach (var slot in comp.Slots.Slots)
                if (slot.Object != null)
                    if (filter(slot.Object))
                        return slot.Object;
            if (comp.HaulSlot.Object != null && filter(comp.HaulSlot.Object))
                return comp.HaulSlot.Object;
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
        static public int Count(Actor actor, Func<Entity, bool> filter)
        {
            return actor.Inventory.Count(filter);
            
        }
        public bool Contains(Entity item)
        {
            return this.Slots.Slots.FirstOrDefault(s => s.Object == item) != null;
        }
        static public bool HasObject(GameObject subject, Func<GameObject, bool> filter)// Predicate<GameObject> filter)
        {
            PersonalInventoryComponent inv;
            if (!subject.TryGetComponent<PersonalInventoryComponent>("Inventory", out inv))
                return false;

            return (from slot in inv.Slots.Slots
                        where slot.HasValue
                        where filter(slot.Object)
                        select slot).FirstOrDefault() != null;

        }
        static public bool HasObject(GameObject subject, Predicate<GameObject> filter, out GameObjectSlot objSlot)
        {
            objSlot = new GameObjectSlot();
            InventoryComponent inv;
            if (!subject.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return false;

            foreach (var container in inv.Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if (filter(slot.Object))
                        {
                            objSlot = slot;
                            return true;
                        }
            return false;
        }
        static public bool Equip(GameObject actor, GameObject item)
        {
            var comp = actor.GetComponent<PersonalInventoryComponent>();
            foreach (var slot in comp.Slots.Slots)
                if (slot.Object == item)
                    return GearComponent.Equip(actor, slot);
            return false;
        }
        static public bool CheckWeight(GameObject actor, GameObject obj)
        {
            return true;
        }

        static public bool GiveObject(IObjectProvider net, GameObject receiver, GameObjectSlot objSlot)
        {
            if (!CheckWeight(receiver, objSlot.Object))
            {
                // can't fit in inventory, drop in place
                net.Spawn(objSlot.Object, receiver.Global + receiver.GetComponent<PhysicsComponent>().Height * Vector3.UnitZ); // add speed also?
                return false;
            }
            

            GameObject obj = objSlot.Object;
            Queue<GameObjectSlot> slots = new Queue<GameObjectSlot>();
          
            GetSlots(receiver, foo => foo.IDType == objSlot.Object.IDType, slots);
            TryGetEmptySlots(receiver, slots);
            int stackMax = (int)objSlot.Object["Gui"]["StackMax"];
            while (slots.Count > 0 && objSlot.StackSize > 0)
            {
                GameObjectSlot slot = slots.Dequeue();
                if (!slot.HasValue)
                    slot.Object = objSlot.Object;
                while (slot.StackSize < stackMax && objSlot.StackSize > 0)
                {
                    slot.StackSize += 1;
                    objSlot.StackSize -= 1;
                }
            }
            if (!objSlot.HasValue)
                    if (obj.IsSpawned)
                //obj.Remove(); // local remove
                net.Despawn(obj);
            return true;
        }

        public bool Haul(GameObject parent, GameObjectSlot objSlot)
        {
            if (objSlot == null)
                return true;
            if (!objSlot.HasValue)
                return true;
            var current = this.HaulSlot.Object;

            if (objSlot.Object == current)
                return true;
            if (!CheckWeight(parent, objSlot.Object))
                return true;
            var net = parent.Net;
            // if currently hauling object of same type, increase held stacksize and dispose other object
            if (current != null)
                if (current.IDType == objSlot.Object.IDType)
                {
                    current.StackSize++;
                    objSlot.Object.Despawn();
                    net.DisposeObject(objSlot.Object);
                    return true;
                }
            // else
            // drop currently hauled object and pick up new one
            this.Throw(Vector3.Zero, parent); //or store carried object in backpack? (if available)

            net.Despawn(objSlot.Object);
            this.HaulSlot.Object = objSlot.Object;
            return true;
        }
        public bool Haul(GameObject parent, GameObject obj)
        {
            if (obj == null)
                return true;

            var current = this.HaulSlot.Object;

            if (obj == current)
                return true;
            if (!CheckWeight(parent, obj))
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
        public bool Take(List<ItemRequirement> reqs)
        {
            if (!Has(reqs))//check that here? or keep taking until run out? regardless whether parent has full amount? or create a TryTake method which checks beforehand instead of cheking here?
                return false;
            var container = this.GetContents();
            var parent = this.Slots.Parent;
            var net = parent.Net;
            
            foreach (var item in reqs)
            {
                int amountRemaining = item.AmountRequired;
                GameObject current;
                foreach (var found in from slot in container where slot.HasValue where (int)slot.Object.IDType == item.ObjectID select slot)
                {
                    current = found.Object;
                    int amountToTake = Math.Min(found.Object.StackSize, amountRemaining);
                    amountRemaining -= amountToTake;
                    if (amountToTake == found.Object.StackSize)
                    {
                        net.Despawn(found.Object);
                        net.DisposeObject(found.Object);
                        found.Clear();
                    }
                    else
                        found.Object.StackSize -= amountToTake;

                    if (amountRemaining == 0)
                    {
                        Net.Client.Instance.EventOccured(Message.Types.ItemLost, parent, current, item.AmountRequired);
                        break;
                    }
                }
            }
            return true;
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
        public bool Has(List<ItemRequirement> reqs)
        {
            var container = this.GetContents();

            foreach (var item in reqs)
            {
                int amountFound = 0;
                foreach (var found in from slot in container where slot.HasValue where (int)slot.Object.IDType == item.ObjectID select slot.Object)
                    amountFound += found.StackSize;
                if (amountFound < item.AmountRequired)
                    return false;
            }
            return true;
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
