using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{
    class InventoryComponent : EntityComponent, IObjectSpace
    {
        public GameObjectSlot Holding { get { return (GameObjectSlot)this["Holding"]; } set { this["Holding"] = value; } }
       // Dictionary<string, ItemContainer> Containers = new Dictionary<string,ItemContainer>();// { get { return (Dictionary<string, ItemContainer>)this["Containers"]; } set { this["Containers"] = value; } }
        GameObject Parent { get { return (GameObject)this["Parent"]; } set { this["Parent"] = value; } }
        public List<ItemContainer> Containers = new();

        public override void MakeChildOf(GameObject parent)
        {
            parent.AddComponent<ContainerComponent>();
            this.Parent = parent;
            if (Containers.Count > 0)
                parent.GetComponent<ContainerComponent>().Add(Containers.First());
        }

        public override string ComponentName
        {
            get
            {
                return "Inventory";
            }
        }

        public float Distance(GameObject obj1, GameObject obj2)
        {
            return InventoryComponent.HasObject(obj1, obj => obj == obj2) ? 0 : -1;
        }
        public Vector3? DistanceVector(GameObject obj1, GameObject obj2)
        {
            return InventoryComponent.HasObject(obj1, obj => obj == obj2) ? Vector3.Zero : new Nullable<Vector3>();
        }

        static public bool TryGetHeldObject(GameObject entity, out GameObjectSlot hauled)
        {
            hauled = entity["Inventory"]["Holding"] as GameObjectSlot;
            return hauled.HasValue;
        }
        static public void TryGetHeldObject(GameObject entity, Action<GameObjectSlot> action)
        {
            entity.TryGetComponent<InventoryComponent>(i =>
            {
                if (i.Holding.HasValue)
                    action(i.Holding);
            });
        }
        static public GameObjectSlot GetHeldObject(GameObject entity)
        {
            InventoryComponent inv;
            if (!entity.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return GameObjectSlot.Empty;
            return inv.Holding;
        }
        static public GameObjectSlot GetHeldObject(GameObject actor, Action<GameObjectSlot> action)
        {
            InventoryComponent inv;
            if (!actor.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return GameObjectSlot.Empty;
            action(inv.Holding);
            return inv.Holding;
        }
        public InventoryComponent()
            : base()
        {
            this.Parent = null;
            this.Holding = GameObjectSlot.Empty;
            this.Containers = new List<ItemContainer>();// new Dictionary<string, ItemContainer>();
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Death:
                   // Loot.PopLoot(parent, Holding);
                    e.Network.PopLoot(Holding.Take(), parent.Global, parent.Velocity);//warning
                    foreach (var container in Containers)
                        foreach (GameObjectSlot slot in container)
                            //Loot.PopLoot(parent, slot);
                            e.Network.PopLoot(slot.Take(), parent.Global, parent.Velocity);//warning
                    return true;

                case Message.Types.UseHauledItem:
                    e.Translate(r =>
                    {
                        TargetArgs t = TargetArgs.Read(e.Network, r);
                        if (!this.Holding.HasValue)
                            return;
                        throw new Exception();
                        //UseComponentOld use;
                        //if (!this.Holding.Object.TryGetComponent<UseComponentOld>(out use))
                        //    return;
                        //parent.GetComponent<ControlComponent>().TryStartScript(use.InstantiatedScripts.FirstOrDefault(), new ScriptArgs(e.Network, parent, t));
                    });
                    return true;

                //case Message.Types.UseInventoryItem:
                //    e.Translate(r =>
                //    {
                //        GameObject usedItem = TargetArgs.Read(e.Network, r).Object;
                //        if (Containers.First().FirstOrDefault(_slot => _slot.Object == usedItem).IsNull())
                //        {
                //            return;
                //        }
                //        usedItem.TryGetComponent<InteractiveComponent>(comp =>
                //        {
                //            //TargetArgs recipient = TargetArgs.Read(Instance, r);
                //            ObjectEventArgs a = ObjectEventArgs.Create(Message.Types.StartScript, w =>
                //            {
                //                Ability.Write(w, comp.Abilities.First(), new TargetArgs(usedItem));
                //            });
                //            e.Network.PostLocalEvent(parent, a);

                //            // NO POSTPLAYERINPUT ON COMPONENTS BECAUSE IT CAN BE CALLED BY SERVER OR BY AI
                //            //Client.PostPlayerInput(Message.Types.StartScript, w =>
                //            //{
                //            //    Ability.Write(w, comp.Abilities.First(), new TargetArgs(usedItem));
                //            //});
                //        });
                //    });
                //    return true;

                case Message.Types.HoldInventoryItem:
                    // a player initiated event that has its parameters in the bytearray instead of the object[] array
                    int slotID = e.Data.Translate<InventoryEventArgs>(e.Network).SlotID;
                    //int slotID = (int)e.Parameters[0];
                    GameObjectSlot sl = Containers.First()[slotID];
                    if (!sl.HasValue)
                        return true;
                    Hold(e.Network, parent, sl);
 
                    //e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Hold, new object[] { sl.Object }));
                    //parent.PostMessage(e.Network, Message.Types.Hold, w => w.Write(sl.Object.NetworkID));


                    //parent.PostMessage(e.Net, Message.Types.PerformAbility, w =>
                    //{
                    //    w.Write((int)Script.Types.PickUp);
                    //    w.Write(Player.Actor.NetworkID);
                    //    w.Write(Target.NetworkID);
                    //});
                    return true;

                case Message.Types.EquipInventoryItem:
                    slotID = e.Data.Translate<InventoryEventArgs>(e.Network).SlotID;
                    GameObjectSlot eqSlot = Containers.First()[slotID];
                    GameObject eq = eqSlot.Object;
                    if (eq is null)
                        return true;
                    //string slotType = eq.GetComponent<EquipComponent>().Slot;
                    //parent.GetComponent<BodyComponent>().BodyParts[slotType].Wearing.Swap(eqSlot);
                    return true;

                case Message.Types.StoreCarried:
                    if (!this.Holding.HasValue)
                        return true;
                    GameObject o1;
                    if (!this.Holding.Take(out o1))
                        return true;
                    ObjectSize oSize = (ObjectSize)o1["Physics"]["Size"];
                    e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Dropped));
                    if (oSize == ObjectSize.Inventoryable)
                    {
                        e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Receive, new object[] { o1 }));
                        return true;
                    }
                    e.Network.Spawn(o1.SetGlobal(parent.Global + new Vector3(0, 0, (float)parent["Physics"]["Height"])));
                    return true;

                    // make this a script?
                case Message.Types.Unequip:
                    e.Translate(r => {
                        eqSlot = parent.GetComponent<BodyComponent>().BodyParts[r.ReadString()].Wearing;
                        eq = eqSlot.Object;
                        eqSlot.Clear();
                        e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Receive, new object[] { eq }));
                    });
                    return true;
                
                case Message.Types.Receive:
                    GameObject obj = e.Parameters.Translate<SenderEventArgs>(e.Network).Sender;

                    

                    // TODO: this is a WORKAROUND
                    GameObjectSlot existingSlot;
                    // FAILSAFE in case player is picking items too fast resulting in picking the same item before the server processes the input
                    if (HasObject(parent, o => o == obj, out existingSlot))
                    {
                        //parent.PostMessage(e.Network, Message.Types.Hold, w => w.Write(obj.NetworkID));
                        return true;
                    }
                    GiveObject(e.Network, parent, obj.ToSlotLink());

                    //GiveObject(parent, objSlot);
                    return true;

                case Message.Types.ArrangeInventory:
                    GameObjectSlot source = e.Parameters[0] as GameObjectSlot;
                    
                    //int index = (int)e.Parameters[2];
                    //Give(parent, source, target, index);
                    if (!source.HasValue)
                        return true;

                    int objSize = (int)source.Object["Physics"]["Size"];
                    if (objSize > 0)
                    {
                        // TODO: code to handle case where object is haulable
                        //Loot.PopLoot(parent, source);
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
                case Message.Types.ConsumeItem:

                    return true;
               // case Message.Types.PickUp:

               
                case Message.Types.Hold:
                    //objSlot = e.Parameters[0] as GameObjectSlot;
                    //Hold(parent, objSlot);

                    // temp workaround to save on client
                  //  e.Network.LogStateChange(parent.NetworkID);
                    obj = e.Parameters.Translate<SenderEventArgs>(e.Network).Sender;//e.TargetArgs.Object;// e.Data.Translate<SenderEventArgs>(e.Network).Sender;
                    GameObjectSlot objSlot = obj.IsSpawned ? obj.ToSlotLink() : InventoryComponent.GetFirstOrDefault(parent, foo => foo == obj);
                    Hold(e.Network, parent, objSlot);

                    return true;

                case Message.Types.DropInventoryItem:
                    e.Data.Translate(e.Network, r =>
                    {
                        slotID = r.ReadInt32();
                        GameObjectSlot slot = this.Containers.First()[slotID];
                        GameObject toDrop;
                        if(!slot.Take(out toDrop))
                            return;

                        toDrop.SetGlobal(parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));
                        e.Network.Spawn(toDrop);
                    });
                    return true;

                case Message.Types.Throw:
                    if (!Holding.HasValue)
                        return true;

                    Vector3 dir = e.Data.Translate<DirectionEventArgs>(e.Network).Direction;
                    Vector3 speed = dir * 0.1f + parent.Velocity;

                    GameObject newobj = Holding.Take();
                    //e.Network.Spawn(newobj, new Position(e.Network.Map, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height), speed));
                    newobj.Velocity = speed;
                    e.Network.Spawn(newobj, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));

                    //if (!this.Holding.HasValue)
                    //    parent.GetComponent<SpriteComponent>().UpdateHeldObject(null);

                    return true;

                //case Message.Types.CraftObject:
                //    BlueprintComponent bpComp = e.Parameters[0] as BlueprintComponent;
                //    Blueprint bp = bpComp.Blueprint;
                //    if (!HasItems(parent, bp[0]))
                //        return true;
                //    foreach (KeyValuePair<GameObject.Types, int> mat in bp[0])
                //        InventoryComponent.RemoveObjects(parent, foo => foo.IDType == mat.Key, mat.Value);
                //    throw new NotImplementedException();
                //    //parent.PostMessage(Message.Types.Receive, null, GameObject.Create(bp.ProductID).ToSlot());
                //    return true;

                default:
                    return false;
            }
        }

        bool Hold(IObjectProvider net, GameObject parent, GameObjectSlot objSlot)
        {
            if (objSlot == null)
                return true;
            if (!objSlot.HasValue)
                return true;
        //    GameObjectSlot.Swap(this.Holding, objSlot);

            if (this.Holding.HasValue)
                net.PostLocalEvent(parent, Message.Types.Receive, this.Holding.Object);
                //net.Spawn(this.Holding.Object, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));

            this.Holding.Clear();
            net.Despawn(objSlot.Object);
            this.Holding.Swap(objSlot);

            //if (this.Holding.Object.Exists)
            //{
            //    if (objSlot.HasValue)
            //        net.Spawn(objSlot.Object.SetGlobal(this.Holding.Object.Global));
            //    net.Despawn(this.Holding.Object);
            //}
          //  parent.GetComponent<ActorSpriteComponent>().UpdateHeldObject(this.Holding.Object);
            return true;
        }
        static public bool HasItems(GameObject actor, Dictionary<GameObject.Types, int> items)
        {
            foreach (KeyValuePair<GameObject.Types, int> mat in items)
                if (InventoryComponent.GetAmount(actor, foo => foo.IDType == mat.Key) < mat.Value)
                    return false;
            return true;
        }

        //public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        //{
        //    //List<Interaction> list = e.Parameters[0] as List<Interaction>;
        //    list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.DropOn, parent, "Give"));
        //    list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Give, parent, "Give"));
        //}


        public ItemContainer AddContainer(byte size)
        {
            ItemContainer container = new ItemContainer(size);

            Containers.Add(container);
            this.Parent.GetComponent<ContainerComponent>().Add(container);

            return container;
        }
        public bool TryGetContainer(string name, out ItemContainer container)
        {
            //return Containers.TryGetValue(name, out container);
            container = this.Containers.First();
            return container != null;
        }
        public ItemContainer GetContainer(string name)
        {
            //return Containers[name];
            return this.Containers.First();
        }

        public bool ContainsItem(GameObject obj)
        {
            foreach (var c in Containers)
                foreach (GameObjectSlot slot in c)
                    if (slot.Object == obj)
                        return true;
            return false;
        }


        public bool TryTake(GameObject obj, out GameObjectSlot slot)
        {
            foreach (var container in Containers)
                foreach (GameObjectSlot invSlot in container)
                    if (invSlot.Object == obj)
                    {
                        slot = invSlot;
                        return true;
                    }
            slot = null;
            return false;
        }


        public bool TryTake(GameObject.Types objID, out GameObjectSlot slot)
        {
            foreach (var container in Containers)
                foreach (GameObjectSlot invSlot in container)
                    if (invSlot.Object != null)
                        if ((invSlot.Object as GameObject).IDType == objID)
                        {
                            slot = invSlot;
                            return true;
                        }
            slot = null;
            return false;
        }

        static public bool IsHauling(GameObject agent, Predicate<GameObject> condition)
        {
            GameObjectSlot hauled;
            if (!TryGetHeldObject(agent, out hauled))
                return false;
            return condition(hauled.Object);
        }

        /// <summary>
        /// Returns true if there are empty slots.
        /// </summary>
        /// <param name="emptySlots">A queue containing the empty slots found, if any.</param>
        /// <returns></returns>
        public bool TryGetEmptySlots(out Queue<GameObjectSlot> emptySlots)
        {
            emptySlots = new Queue<GameObjectSlot>();
            foreach (var container in Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object == null)
                        emptySlots.Enqueue(slot);
            return emptySlots.Count > 0;
        }

        public bool TryGetEmptySlots(Queue<GameObjectSlot> emptySlots)
        {
            foreach (var container in Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object == null)
                        emptySlots.Enqueue(slot);
            return emptySlots.Count > 0;
        }

        /// <summary>
        /// Returns true if empty slots are found in actor's inventory.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="emptySlots">A queue containing the empty slots found, if any.</param>
        /// <returns></returns>
        static public bool TryGetEmptySlots(GameObject actor, out Queue<GameObjectSlot> emptySlots)
        {
            InventoryComponent invComp;
            if(!actor.TryGetComponent<InventoryComponent>("Inventory", out invComp))
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

        /// <summary>
        /// Returns true if objects with the same objectID are found.
        /// </summary>
        /// <param name="objID">The objectID to look for.</param>
        /// <param name="slotsFound">A queue containing all the slots the objectID has been found in.</param>
        /// <returns></returns>
        public bool TryFind(GameObject.Types objID, out Queue<GameObjectSlot> slotsFound)
        {
            slotsFound = new Queue<GameObjectSlot>();
            //GuiComponent gui;
            foreach (var container in Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if (slot.Object.IDType == objID)
                            //if (slot.Object.GetComponent<GuiComponent>("Gui").GetSpace() > 0)
                            if (slot.FreeSpace> 0)
                                slotsFound.Enqueue(slot);
            return slotsFound.Count > 0;
        }


        /// <summary>
        /// Returns true if an object meeting the conditions is found
        /// </summary>
        /// <param name="filter">The condition to compare</param>
        /// <param name="objSlot">The slot containing the first object found</param>
        /// <returns></returns>
        public bool TryFind(Predicate<GameObject> filter, out GameObjectSlot objSlot)
        {
            objSlot = GameObjectSlot.Empty;
            foreach (var container in Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if (filter(slot.Object))
                        {
                            objSlot = slot;
                            return true;
                        }
            return false;
        }

        /// <summary>
        /// Returns true if any slots meeting the conditions is found.
        /// </summary>
        /// <param name="filter">The condition to check slots to.</param>
        /// <param name="slots">A list of all the slots satisfying the provided condition.</param>
        /// <returns></returns>
        public bool TryFind(Func<GameObject, bool> filter, out List<GameObjectSlot> slots)
        {
            slots = new List<GameObjectSlot>();
            foreach (var container in Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if (filter(slot.Object))
                            slots.Add(slot);
            return slots.Count > 0;
        }

        /// <summary>
        /// Returns true if any slots meeting the conditions is found.
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="filter"></param>
        /// <param name="slots"></param>
        /// <returns></returns>
        static public bool GetSlots(GameObject actor, Func<GameObject, bool> filter, Queue<GameObjectSlot> slots)
        {
            PersonalInventoryComponent invComp;
            if (!actor.TryGetComponent<PersonalInventoryComponent>(out invComp))
                throw (new Exception(actor.Name + " doesn't have an inventory component."));

            //foreach (var container in invComp.Containers)
                foreach (GameObjectSlot slot in invComp.Slots.Slots)// container)
                    if (slot.Object != null)
                        if (filter(slot.Object))
                            slots.Enqueue(slot);
            return slots.Count > 0;
        }
        static public bool GetSlots(GameObject actor, Func<GameObject, bool> filter, List<GameObjectSlot> slots)
        {
            Queue<GameObjectSlot> queue = new();
            bool found = GetSlots(actor, filter, queue);
            slots = queue.ToList();
            return found;
        }
        static public List<GameObjectSlot> GetSlots(GameObject actor, Func<GameObject, bool> filter)
        {
            Queue<GameObjectSlot> queue = new Queue<GameObjectSlot>();
            GetSlots(actor, filter, queue);
            return queue.ToList();
        }
        static public List<GameObjectSlot> GetSlots(GameObject actor)
        {
            return GetSlots(actor, foo => true);
        }
        static public GameObjectSlot GetFirstOrDefault(GameObject actor, Func<GameObject, bool> pred)
        {
            InventoryComponent inv;
            if (!actor.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return GameObjectSlot.Empty;
            foreach (var container in inv.Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if (pred(slot.Object))
                            return slot;
            return GameObjectSlot.Empty;
        }
        static public bool HasObject(GameObject subject, Predicate<GameObject> filter)
        {
            InventoryComponent inv;
            if (!subject.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return false;

            foreach (var container in inv.Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if (filter(slot.Object))
                            return true;
            return false;
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

        static public int GetAmount(GameObject subject, Predicate<GameObject> filter)
        {
            int amount = 0;
            InventoryComponent inv;
            if (!subject.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return 0;

            foreach (var container in inv.Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if (filter(slot.Object))
                            amount += slot.StackSize;
            return amount;
        }
        static public void RemoveObjects(GameObject subject, Predicate<GameObject> filter, int amount = 1)
        {
            int remaining = amount;
            InventoryComponent inv;
            if (!subject.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return;


                foreach (var container in inv.Containers)
                    foreach (GameObjectSlot slot in container)
                        if (slot.Object != null)
                            if (filter(slot.Object))
                                while (slot.HasValue)
                                {
                                    slot.StackSize--;
                                    remaining--;
                                    if (remaining == 0)
                                        return;
                                }
        }

        /// <summary>
        /// Tries to find the first interaction, within objects in the inventory, that has the potential to satisfy the supplied interaction condition.
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="cond"></param>
        /// <param name="objSlot"></param>
        /// <param name="interaction"></param>
        /// <returns></returns>
        //public bool TryFind(GameObject parent, Condition cond, out GameObjectSlot objSlot, out InteractionOld interaction)
        //{
        //    objSlot = GameObjectSlot.Empty;
        //    interaction = null;
        //    foreach (var container in Containers)
        //        foreach (GameObjectSlot slot in container)
        //            if (slot.Object != null)
        //            {
        //                List<InteractionOld> interactions = new List<InteractionOld>();
        //                slot.Object.Query(parent, interactions);
        //                foreach (InteractionOld i in interactions)
        //                    if (cond.Evaluate(i))
        //                    {
        //                        objSlot = slot;
        //                        interaction = i;
        //                        return true;
        //                    }
        //            }
        //    return false;
        //}

        public bool TryGive(GameObject obj, int amount = 1)
        {
            //get slots that contain a same object and can fit more
            Queue<GameObjectSlot> existingSlots;
            GameObject.Types objID = obj.IDType;
            TryFind(objID, out existingSlots);

            //get empty slots
            Queue<GameObjectSlot> emptySlots;
            TryGetEmptySlots(out emptySlots);

            GuiComponent existingGui, objGui = obj.GetComponent<GuiComponent>("Gui");
            int capacity = (int)objGui["StackMax"];

            Queue<GameObjectSlot> allSlots = existingSlots;
            while (emptySlots.Count > 0)
                allSlots.Enqueue(emptySlots.Dequeue());
            if (allSlots.Count == 0)
                return false;
            while (allSlots.Count > 0 && amount > 0)
            {
                GameObjectSlot currentSlot = allSlots.Dequeue();
                if (currentSlot.Object == null)
                {
                    currentSlot.Object = GameObject.Create(objID);
                    currentSlot.StackSize = 1;
                    amount -= 1;
                }
                existingGui = currentSlot.Object.GetComponent<GuiComponent>("Gui");
                
                while (currentSlot.StackSize < capacity && amount > 0)
                {
                    currentSlot.StackSize += 1;
                    amount -= 1;
                }
            }
            return true;
        }

        static public void UseHeldObject(GameObject actor, TargetArgs target)
        {
            GameObjectSlot obj;
            if (!InventoryComponent.TryGetHeldObject(actor, out obj))
                return;
            throw new NotImplementedException();
            //obj.Object.PostMessage(Message.Types.Use, null, Player.Actor, target.Object, target.Face);
        }

        static public bool CheckWeight(GameObject actor, GameObject obj)
        {
            if ((int)obj["Physics"]["Size"] < 1)
                return true;

            //check if parent can carry weight
            float str = StatsComponent.GetStat(actor, Stat.Strength.Name);
            float weight = (float)obj["Physics"]["Weight"];
            //if (str < weight)
            //    return false;
            if (str >= weight)
                return true;
            //if already hauling, drop currently hauled object (switch places with new item)
        //    Hold(actor, obj);
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
            //// why not do this on the sender object straight away?
            //if ((int)objSlot.Object["Physics"]["Size"] == 1)
            //{
            //    GameObject.PostMessage(receiver, Message.Types.Hold, receiver, objSlot, objSlot.Object);
            //    return true;
            //}

            GameObject obj = objSlot.Object;
            Queue<GameObjectSlot> slots = new Queue<GameObjectSlot>();
            //if (!TryFind(receiver, foo => foo.ID == objSlot.Object.ID, slots))
            //    return true;
            GetSlots(receiver, foo => foo.IDType == objSlot.Object.IDType, slots);
            TryGetEmptySlots(receiver, slots);
            int stackMax = (int)objSlot.Object["Gui"]["StackMax"];
            while (slots.Count > 0 && objSlot.StackSize > 0)
            {
                GameObjectSlot slot = slots.Dequeue();
                if (!slot.HasValue)
                    slot.Object = objSlot.Object;// GameObject.Create(objSlot.Object.ID);
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
        static public bool GiveObject(IObjectProvider net, GameObject receiver, GameObject obj, int amount = 1)
        {
            InventoryComponent inv;
            if (!receiver.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return false;


            if (CheckWeight(receiver, obj))
                return true;

            //get slots that contain a same object and can fit more
            Queue<GameObjectSlot> existingSlots;
            GameObject.Types objID = obj.IDType;
            inv.TryFind(objID, out existingSlots);

            //get empty slots
            Queue<GameObjectSlot> emptySlots;
            inv.TryGetEmptySlots(out emptySlots);

            GuiComponent existingGui, objGui = obj.GetComponent<GuiComponent>("Gui");
            int capacity = (int)objGui["StackMax"];
            Queue<GameObjectSlot> allSlots = existingSlots;
            while (emptySlots.Count > 0)
                allSlots.Enqueue(emptySlots.Dequeue());

            while (allSlots.Count > 0 && amount > 0)
            {
                GameObjectSlot currentSlot = allSlots.Dequeue();
                if (currentSlot.Object == null)
                {
                    // DONT CREATE NEW OBJECT
                    currentSlot.Object = obj;// GameObject.Create(objID);
                    net.Despawn(obj);
                    //obj.Remove();
                    currentSlot.StackSize = 1;
                    amount -= 1;
                }
                //existingGui = currentSlot.Object.GetComponent<GuiComponent>("Gui");

                while (currentSlot.StackSize < capacity && amount > 0)
                {
                    currentSlot.StackSize += 1;
                    amount -= 1;
                }
            }
            //Map.RemoveObject(obj);
            if (amount == 0)
            {
                net.Despawn(obj);
                //obj.Remove();
                return true;
            }

            if (inv.Holding.Object != null)
                return true;

            inv.Holding.Object = obj;
            net.Despawn(obj);
            //obj.Remove();
            return true;
        }

        //static public GameObject TakeHolding(GameObject parent)
        //{
        //    GameObjectSlot holding = parent.GetComponent<InventoryComponent>().Holding;
        //    GameObject obj = holding.Take();
        //    parent.GetComponent<ActorSpriteComponent>().UpdateHeldObject(holding.Object);
        //    return obj;
        //}

        private static void Hold(IObjectProvider net, GameObject receiver, GameObject obj)
        {
            if (obj is null)
                return;
            if (receiver == obj)
                return;
            GameObjectSlot currentHaul;
            if (TryGetHeldObject(receiver, out currentHaul))
            {
                GameObject currentObj = currentHaul.Object;
                if (currentObj != null)
                {
                    Chunk.AddObject(currentObj, receiver.Map, obj.Global);
                }
            }
            (receiver["Inventory"]["Holding"] as GameObjectSlot).Object = obj;// = new GameObjectSlot(obj);
            net.Despawn(obj);
            //obj.Remove();
        }

        //public void Give(GameObject parent, GameObjectSlot source, GameObjectSlot drag)
        //{
        //    GameObjectSlot empty = null;
        //    foreach (GameObjectSlot slot in Containers.First().Value)
        //    {
        //        if (!slot.HasValue)
        //        {
        //            if (empty == null)
        //                empty = slot;
        //            continue;
        //        }
        //        if(slot.Object.ID == GameObject.T
        //    }
        //}

        public void Give(GameObject parent, GameObjectSlot source, GameObjectSlot drag, int index)
        {
            ItemContainer container = Containers.First();
            GameObjectSlot target = container[index];
            if (source == target)
                return;

            if ((int)drag.Object["Physics"]["Size"] > 0)
            {

            //    Chunk.AddObject(drag.Object, parent.Global);
                this.GetProperty<GameObjectSlot>("Holding").Swap(drag);

                return;
            }

            if (target.Object == drag.Object)
            {
                target.StackSize += drag.StackSize;
                source.StackSize -= drag.StackSize;
                return;
            }
            if (target.Object == null)
            {
                target.Object = source.Object;
                target.StackSize = drag.StackSize;
                source.StackSize -= drag.StackSize;
                return;
            }
            if (source.Object != target.Object)
            {
                if (source.Object == DragDropManager.Instance.Item)
                    target.Swap(source);
                else
                    target.Swap(drag);
                return;
            }
            //container[index].Swap(obj);
        }

        public void Give(GameObject obj, int amount = 1)
        {
            //get slots that contain a same object and can fit more
            Queue<GameObjectSlot> existingSlots;
            GameObject.Types objID = obj.IDType;
            TryFind(objID, out existingSlots);

            //get empty slots
            Queue<GameObjectSlot> emptySlots;
            TryGetEmptySlots(out emptySlots);

            GuiComponent objGui = obj.GetComponent<GuiComponent>("Gui");
            int capacity = (int)objGui["StackMax"];
            Queue<GameObjectSlot> allSlots = existingSlots;
            while (emptySlots.Count > 0)
                allSlots.Enqueue(emptySlots.Dequeue());

            while (allSlots.Count > 0 && amount > 0)
            {
                GameObjectSlot currentSlot = allSlots.Dequeue();
                if (currentSlot.Object == null)
                {
                    currentSlot.Object = GameObject.Create(objID);
                    currentSlot.StackSize = 1;
                    amount -= 1;
                }
                while (currentSlot.StackSize < capacity && amount > 0)
                {
                    currentSlot.StackSize += 1;
                    amount -= 1;
                }
            }
        }

        public bool Contains(GameObject.Types objID, out int count, out List<GameObjectSlot> slots)
        {
            count = 0;
            slots = new List<GameObjectSlot>();
            foreach (var container in Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if (slot.Object.IDType == objID)
                        {
                            slots.Add(slot);
                            count += slot.Object.GetComponent<GuiComponent>("Gui").GetProperty<int>("StackMax");
                        }
            return count > 0;
        }

        public bool Remove(GameObject.Types objID, int count)
        {
            GuiComponent gui;
            int myCount = 0;
            List<GameObjectSlot> slotsToTakeFrom = new List<GameObjectSlot>();
            foreach (var container in Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if ((slot.Object as GameObject).IDType == objID)
                            if (slot.Object.TryGetComponent<GuiComponent>("Gui", out gui))
                            {
                                if (myCount >= count)
                                    continue;
                                myCount += gui.GetProperty<int>("StackMax");
                                slotsToTakeFrom.Add(slot);
                            }
            foreach (GameObjectSlot slot in slotsToTakeFrom)
                if (slot.Object != null)
                    if (slot.Object.TryGetComponent<GuiComponent>("Gui", out gui))
                    {
                        if (count <= 0)
                            continue;
                        int taken = Math.Min(count, gui.GetProperty<int>("StackMax"));
                        //gui.StackSize -= taken;
                        gui.Properties["StackMax"] = gui.GetProperty<int>("StackMax") - taken;
                        if (gui.GetProperty<int>("StackMax") == 0)
                            slot.Clear();
                        count -= taken;
                    }
            return true;
        }

        //internal bool Find(GameObject obj, out GameObject found)
        //{
        //    found = null;
        //    foreach (KeyValuePair<string, ItemContainer> container in GetProperty<Dictionary<string, ItemContainer>>("Containers"))
        //        foreach (GameObjectSlot slot in container.Value)
        //            if (slot.Object == obj)
        //            {
        //                if (slot.StackSize == 1)
        //                {
        //                    found = slot.Object;
        //                    return true;
        //                }
        //                else if (slot.StackSize > 1)
        //                {
        //                    found = GameObject.Create(slot.Object.ID);
        //                    return true;
        //                }
        //            }
        //    return false;
        //}

        //internal bool TryTake(GameObject obj, out GameObject found)
        //{
        //    found = null;
        //    foreach (KeyValuePair<string, ItemContainer> container in GetProperty<Dictionary<string, ItemContainer>>("Containers"))
        //        foreach (GameObjectSlot slot in container.Value)
        //            if (slot.Object == obj)
        //            {
        //                if (slot.StackSize == 1)
        //                {
        //                    found = slot.Object;
        //                    slot.Object = null;
        //                    slot.StackSize = 0;
        //                    return true;
        //                }
        //                else if (slot.StackSize > 1)
        //                {
        //                    slot.StackSize -= 1;
        //                    found = GameObject.Create(slot.Object.ID);
        //                    return true;
        //                }
        //            }
        //    return false;
        //}

        internal bool TryRemove(GameObject parent)
        {
            foreach (var container in Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object == parent)
                    {
                        slot.StackSize -= 1;
                        if (slot.StackSize == 0)
                            slot.Object = null;
                        return true;

                        //int stackSize = slot.Object["Gui"].GetProperty<int>("StackMax");
                        //if (stackSize == 1)
                        //{
                        //    slot.Object = null;
                        //   // return true;
                        //}
                        //else
                        //    slot.Object["Gui"]["StackMax"] = stackSize - 1;
                        //return true;
                    }
            return false;
        }

        //public int Find(GameObject obj)
        //{
        //    int found = 0;
        //    foreach (KeyValuePair<string, ItemContainer> container in GetProperty<Dictionary<string, ItemContainer>>("Containers"))
        //        foreach (GameObjectSlot slot in container.Value)
        //            if (slot.Object != null)
        //                if ((slot.Object as GameObject).ID == (obj as GameObject).ID)
        //                    found++;
        //    return found;
        //}

        //public int Find(GameObject.Types objID)
        //{
        //    int found = 0;
        //    foreach (KeyValuePair<string, ItemContainer> container in GetProperty<Dictionary<string, ItemContainer>>("Containers"))
        //        foreach (GameObjectSlot slot in container.Value)
        //            if (slot.Object != null)
        //                if ((slot.Object as GameObject).ID == objID)
        //                {
        //                    GuiComponent gui;
        //                    if ((slot.Object as GameObject).TryGetComponent<GuiComponent>("Gui", out gui))
        //                        found += gui.GetProperty<int>("StackMax");
        //                    //found++;
        //                }
        //    return found;
        //}

        static public bool Give(GameObject actor, GameObject.Types objID, int amount)
        {
            InventoryComponent inv;
            if (!actor.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return false;

            GameObject obj = GameObject.Create(objID);
            //obj.GetComponent<GuiComponent>("Gui").Properties["StackMax"] = count;
            // for (int i = 0; i < count; i++)
            inv.Give(obj, amount);
            //InventoryComponent.GiveObject(actor, obj);
            return true;
        }

       

        static public void CollectBonuses(GameObject obj, BonusCollection list)
        {
            BonusesComponent.GetBonuses(obj["Inventory"]["Holding"] as GameObjectSlot, list);
        }

       
        public override object Clone()
        {
            InventoryComponent comp = new InventoryComponent();
            //foreach (KeyValuePair<string, ItemContainer> c in this.Containers)
            //    comp.AddContainer(c.Key, (byte)c.Value.Capacity);


            using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                this.Write(w);
                w.BaseStream.Position = 0;
                using (BinaryReader r = new BinaryReader(w.BaseStream))
                    comp.Read(r);
            }

            return comp;
        }
        public override void Write(System.IO.BinaryWriter writer)
        {
            this.Holding.Write(writer);
            // update this
            writer.Write(this.Containers.Count);
            foreach (var cont in this.Containers)
            {
            //    writer.Write(cont.ID);
                cont.Write(writer);
            }
            //for (int i = 0; i < this.Containers.Count; i++)
            //{
            //    Containers.ElementAt(i).Value.Write(writer);
            //}
            //Containers.First().Value.Write(writer);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Holding.Read(reader);
            var containerCount = reader.ReadInt32();
            if (this.Parent is not null)
                foreach (var cont in this.Containers)
                    this.Parent.GetComponent<ContainerComponent>().Remove(cont);
            this.Containers = new List<ItemContainer>();
            for (int i = 0; i < containerCount; i++)
                this.Containers.Add(ItemContainer.Create(this.Parent, reader));
            if (this.Parent is not null)
                foreach (var cont in this.Containers)
                    this.Parent.GetComponent<ContainerComponent>().Add(cont);
            ////this.Containers = new List<ItemContainer>();
            //for (int i = 0; i < containerCount; i++)
            //{
            //    byte index = reader.ReadByte();
            //    ItemContainer existing = this.Containers.ElementAtOrDefault(index);
            //    if (existing.IsNull())
            //    {
            //        ItemContainer cont = ItemContainer.Create(reader);
            //        this.Containers.Add(cont);
            //        this.Parent.GetComponent<ContainerComponent>().Add(cont);
            //    }
            //    else
            //        existing.Read(reader);
            //}
        }

        public override void OnObjectCreated(GameObject parent)
        {
            //ActorSpriteComponent already does that
         //   ActorSpriteComponent.UpdateHeldObjectSprite(parent, this.Holding.Object);
        }
        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();// Tag(Tag.Types.Compound, "Inventory");
            SaveTag containersTag = new SaveTag(SaveTag.Types.Compound, "Containers");

            foreach (var container in Containers)
            {
                //SaveTag containerTag = new SaveTag(SaveTag.Types.Compound, "Container");
                ////    containerTag.Add(new SaveTag(SaveTag.Types.String, "Name", container.Key));
                //containerTag.Add(new SaveTag(SaveTag.Types.Byte, "Capacity", (byte)container.Capacity));
                //SaveTag items = new SaveTag(SaveTag.Types.Compound, "Slots");
                //for (int i = 0; i < container.Count; i++)
                //{
                //    GameObjectSlot objSlot = container[i];
                //    if (objSlot.Object != null)
                //        items.Add(new SaveTag(SaveTag.Types.Compound, i.ToString(), objSlot.Save()));
                //}
                //containerTag.Add(items);

                containersTag.Add(new SaveTag(SaveTag.Types.Compound, "Container", container.Save()));
            }

            if (this.GetProperty<GameObjectSlot>("Holding").Object != null)
                data.Add(new SaveTag(SaveTag.Types.Compound, "Holding", this.GetProperty<GameObjectSlot>("Holding").Save()));

            data.Add(containersTag);

            return data;
        }
        internal override void Load(SaveTag data)
        {
            List<SaveTag> tag = data.Value as List<SaveTag>;
            Dictionary<string, SaveTag> byName = tag.ToDictionary(foo => foo.Name);

            if (byName.ContainsKey("Holding"))
            {
                SaveTag haulTag = byName["Holding"];
                this["Holding"] = GameObjectSlot.Create(haulTag);
            }
            SaveTag containersTag = byName["Containers"];
            if ((containersTag.Value as List<SaveTag>).Count > 1)
            {
                List<SaveTag> containers = containersTag.Value as List<SaveTag>;
                // WARNING!!! TEMPORARY
                //for (int i = 0; i < containers.Count - 1; i++)
                //{
                    SaveTag containerTag = containers[containers.Count - 2];//[i];
                    //ItemContainer container = ItemContainer.Create(containerTag);
                    //container.ID = 0;
                    //this.Containers[0] = container;
                    this.Containers = new List<ItemContainer>();
                    this.Containers.Add(ItemContainer.Create(containerTag));
                    //this.Containers[0].Load(containerTag);
               //     Console.WriteLine(this.Containers[0] == this.Parent.GetComponent<ContainerComponent>()[0]);

                    //this.Parent.GetComponent<ContainerComponent>().Add(container);
                    //this.Containers.Add(container);
                //}
            }
        }

        public override void Instantiate(GameObject parent, Action<GameObject> instantiator)
        {
            //this.Holding.Instantiate(instantiator);
            //foreach (var slot in
            //    from slot in this.Containers.First()
            //    where slot.HasValue
            //    select slot)
            //{
            //    slot.Object.Instantiate(instantiator);
            //}
        }

        public override void GetChildren(List<GameObjectSlot> list)
        {
            foreach (var cont in this.Containers)
                list.AddRange(cont);
            list.Add(this.Holding);
        }

        static public void ConsumeEquipped(GameObject parent, int amount)
        {
            InventoryComponent invComp = parent.Components.Values.FirstOrDefault(foo => foo is InventoryComponent) as InventoryComponent;
            if (invComp.IsNull())
                return;
            invComp.Holding.StackSize -= amount;
            throw new NotImplementedException();
            //parent.PostMessage(Message.Types.Dropped, parent);
        }
        static public bool ConsumeEquipped(GameObject parent, Func<GameObjectSlot, bool> check, int amount = 1)
        {
            InventoryComponent invComp = parent.Components.Values.FirstOrDefault(foo => foo is InventoryComponent) as InventoryComponent;
            if (invComp.IsNull())
                return false;
            if (!invComp.Holding.HasValue)
                return false;
            if (invComp.Holding.StackSize < amount)
                return false;
            if (!check(invComp.Holding))
                return false;
            invComp.Holding.StackSize -= amount;
           // throw new NotImplementedException();
            //parent.PostMessage(Message.Types.Dropped, parent);
            return true;
        }

        public override string GetStats()
        {
            //BonusCollection list = new BonusCollection();
            //BonusesComponent.GetBonuses(this.Holding, list);
            //return list.ToString() + "\n";
            string text = "";
            if (Holding.HasValue)
                text += Holding.Object.GetStats();
            return text;
        }

        static public void PollStats(GameObject obj, StatCollection list)
        {
            //InventoryComponent inv;
            //if (!obj.TryGetComponent<InventoryComponent>("Inventory", out inv))
            //    return;
            EquipComponent.GetStats(InventoryComponent.GetHeldObject(obj), list);
        }

        
    }
}
