using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{
    class PersonalInventoryComponent : Component, IObjectSpace
    {
        Container HaulContainer;
        GameObjectSlot HaulSlot;// = new GameObjectSlot();
        GameObject Parent { get { return (GameObject)this["Parent"]; } set { this["Parent"] = value; } }
        //public ItemContainer Children { get { return (ItemContainer)this["Children"]; } set {
        //    this["Children"] = value; 
        //} }
        public Container Slots { get { return (Container)this["Slots"]; } set { this["Slots"] = value; } }
        public byte Capacity { get { return (byte)this["Capacity"]; } set { this["Capacity"] = value; } }
        int HaulIndex = 0;
        public override string ComponentName
        {
            get
            {
                return "PersonalInventory";
            }
        }

        public float Distance(GameObject obj1, GameObject obj2)
        {
            return PersonalInventoryComponent.HasObject(obj1, obj => obj == obj2) ? 0 : -1;
        }
        public Vector3? DistanceVector(GameObject obj1, GameObject obj2)
        {
            return PersonalInventoryComponent.HasObject(obj1, obj => obj == obj2) ? Vector3.Zero : new Nullable<Vector3>();
        }

        public PersonalInventoryComponent Initialize(byte capacity)
        {
            this.Capacity = capacity;
            //this.Children = new ItemContainer(this.Capacity, () => this.Parent.ChildrenSequence);
            //this.Children.Parent = this.Parent;
            return this;
        }

        public override void MakeChildOf(GameObject parent)
        {
            this.Parent = parent;
            //this.Holding.Parent = parent;
            //this.Slots.Parent = parent;
            //this.Slots.ID = parent.ContainerSequence;
            parent.RegisterContainer(this.Slots);
            parent.RegisterContainer(this.HaulContainer);
        }
        public GameObjectSlot GetHauling()
        {
            return this.HaulSlot;
            return this.Slots.GetSlot(this.HaulIndex);
        }
        public static GameObjectSlot GetHauling(GameObject parent)
        {
            return parent.GetComponent<PersonalInventoryComponent>().GetHauling();
        }
        //public override void GetChildren(List<GameObjectSlot> list)
        //{
        //    list.AddRange(this.Children);
        //}
        public override void GetContainers(List<Container> list)
        {
            list.Add(this.HaulContainer);
            list.Add(this.Slots);
        }
        public PersonalInventoryComponent()
            : base()
        {
            this.Parent = null;
            //this.Holding = GameObjectSlot.Empty;
            this.Capacity = 0;
            //this.Children = new ItemContainer();
            this.Slots = new Container() { Name = "Inventory" };
            this.HaulContainer = new Container(1) { Name = "Hauling" };
            this.HaulSlot = this.HaulContainer.Slots.First();
        }
        public PersonalInventoryComponent(byte capacity)
            : this()
        {
            this.Capacity = capacity;
            //this.Children = new ItemContainer();
            this.Slots = new Container(capacity)
            {
                Name = "Inventory"
                //,
                //Filter =
                //    o =>
                //    {
                //        return o.GetPhysics().Size == ObjectSize.Inventoryable;
                //    }
            };
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                //case Message.Types.UseHauledItem:
                //    e.Translate(r =>
                //    {
                //        TargetArgs t = TargetArgs.Read(e.Network, r);
                //        if (!this.Holding.HasValue)
                //            return;
                //        UseComponent use;
                //        if (!this.Holding.Object.TryGetComponent<UseComponent>(out use))
                //            return;
                //        parent.GetComponent<ControlComponent>().TryStartScript(use.InstantiatedScripts.FirstOrDefault(), new ScriptArgs(e.Network, parent, t));
                //    });
                //    return true;

                case Message.Types.UseInventoryItem:
                    e.Translate(r =>
                    {
                        TargetArgs t = TargetArgs.Read(e.Network, r);
                        GameObject usedItem = t.Object;
                        if (parent.GetChildren().FirstOrDefault(_slot => _slot.Object == usedItem).IsNull())
                        {
                            return;
                        }
                        usedItem.TryGetComponent<InteractiveComponent>(comp =>
                        {
                            //TargetArgs recipient = TargetArgs.Read(Instance, r);
                            ObjectEventArgs a = ObjectEventArgs.Create(Message.Types.StartScript, w =>
                            {
                                Ability.Write(w, comp.Abilities.First(), new TargetArgs(usedItem));
                            });
                            e.Network.PostLocalEvent(parent, a);

                            // NO POSTPLAYERINPUT ON COMPONENTS BECAUSE IT CAN BE CALLED BY SERVER OR BY AI
                            //Net.Client.PostPlayerInput(Message.Types.StartScript, w =>
                            //{
                            //    Ability.Write(w, comp.Abilities.First(), new TargetArgs(usedItem));
                            //});
                        });
                    });
                    return true;

                case Message.Types.HoldInventoryItem:
                    // a player initiated event that has its parameters in the bytearray instead of the object[] array
                    int slotID = e.Data.Translate<InventoryEventArgs>(e.Network).SlotID;
                    //int slotID = (int)e.Parameters[0];
                    GameObjectSlot sl = parent.GetChildren()[slotID];
                    if (!sl.HasValue)
                        return true;
                    //Hold(e.Network, parent, sl);
                    return true;

                case Message.Types.EquipInventoryItem:
                    slotID = e.Data.Translate<InventoryEventArgs>(e.Network).SlotID;
                    GameObjectSlot eqSlot = parent.GetChildren()[slotID];
                    GameObject eq = eqSlot.Object;
                    if (eq.IsNull())
                        return true;
                    //string slotType = eq.GetComponent<EquipComponent>().Slot;
                    //parent.GetComponent<BodyComponent>().BodyParts[slotType].Wearing.Swap(eqSlot);
                    return true;

                //case Message.Types.StoreCarried:
                //    if (!this.Holding.HasValue)
                //        return true;
                //    GameObject o1;
                //    if (!this.Holding.Take(out o1))
                //        return true;
                //    ObjectSize oSize = (ObjectSize)o1["Physics"]["Size"];
                //    e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Dropped));
                //    if (oSize == ObjectSize.Inventoryable)
                //    {
                //        e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Receive, new object[] { o1 }));
                //        return true;
                //    }
                //    e.Network.Spawn(o1.SetGlobal(parent.Global + new Vector3(0, 0, (float)parent["Physics"]["Height"])));
                //    return true;

                // make this a script?

                //case Message.Types.Unequip:
                //    e.Translate(r =>
                //    {
                //        eqSlot = parent.GetComponent<GearComponent>().EquipmentSlots[GearType.Dictionary[(GearType.Types)r.ReadInt32()]];
                //        this.Children.InsertObject(e.Network, eqSlot);
                //        //eqSlot = parent.GetComponent<BodyComponent>().BodyParts[r.ReadString()].Wearing;
                //        //this.Children.InsertObject(e.Network, eqSlot);

                //        //eq = eqSlot.Object;
                //        //eqSlot.Clear();
                //        // e.Network.PostLocalEvent(parent, ObjectEventArgs.Create(Message.Types.Receive, new object[] { eq }));
                //    });
                //    return true;

                    // TODO: maybe create a new message called InventoryInteraction that individual components can respond too?
                case Message.Types.SlotInteraction:
                    this.SlotInteraction(parent, e.Parameters[0] as GameObject, e.Parameters[1] as GameObjectSlot);
                    //this.HaulIndex = (e.Parameters[1] as GameObjectSlot).ID;

                    return true;

                case Message.Types.Insert:
                    GameObjectSlot objSlot = e.Parameters[0] as GameObjectSlot;
                    //if (objSlot.Object == null)
                    //    return true;
                    //if (objSlot.Object.GetPhysics().Size != ObjectSize.Inventoryable)
                    //{
                    //    parent.Net.PopLoot(objSlot.Object, parent);
                    //    objSlot.StackSize--;
                    //    return true;
                    //}

                    ////if (objSlot.Object.GetPhysics().Size == ObjectSize.Haulable)
                    ////{
                    ////    if (this.Holding.HasValue)
                    ////    {
                    ////        // drop incoming item or do nothing?
                    ////        e.Network.Spawn(objSlot.Take(), new Position(parent.Global + parent.GetPhysics().Height * Vector3.UnitZ, parent.Velocity));
                    ////        // or pop loot?
                    ////        return true;
                    ////    }
                    ////    e.Network.Despawn(objSlot.Object);
                    ////    this.Holding.Swap(objSlot);

                    ////    return true;
                    ////}
                    //this.Children.InsertObject(parent.Net, objSlot);
                    //return true;
                    return PickUp(parent, objSlot);

                //case Message.Types.InsertAt:
                //    parent.Global.GetChunk(e.Network.Map).Changed = true;
                //    Net.IObjectProvider net = e.Network;
                //    ArrangeChildrenArgs args = e.Data.Translate<ArrangeChildrenArgs>(e.Network);
                //    GameObject sourceObj = args.Object.Object;
                //    GameObjectSlot targetSlot, sourceSlot;
                //    if (!args.SourceEntity.Object.TryGetChild(args.TargetSlotID, out targetSlot) ||
                //        !parent.TryGetChild(args.SourceSlotID, out sourceSlot))
                //        return true;
                //    int amount = args.Amount;

                //    if (sourceObj.IsNull())
                //    {
                //        // object originating from a splitstack operation, instantiate it on network and resend message to parent
                //        GameObject newObj  =sourceSlot.Object.Clone();
                //        e.Network.InstantiateObject(newObj);
                //        args.Object = new TargetArgs(newObj);
                //        e.Network.SyncEvent(parent, Message.Types.InsertAt, args.Write);
                //        return true;
                //    }
                //    if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
                //    {
                //        targetSlot.Set(sourceObj, amount);
                //        sourceSlot.StackSize -= amount;
                //        return true;
                //    }
                //    if (sourceSlot.Object.ID == targetSlot.Object.ID)
                //    {
                //        if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
                //        {
                //            targetSlot.StackSize += sourceSlot.StackSize;
                //            e.Network.DisposeObject(sourceSlot.Object.NetworkID);
                //            sourceSlot.Clear();
                //            //merge slots
                //            return true;
                //        }
                //    }
                //    else
                //        if (amount < sourceSlot.StackSize)
                //            return true;

                //    targetSlot.Swap(sourceSlot);
                //    return true;

                case Message.Types.Receive:
                    GameObject obj = e.Parameters.Translate<SenderEventArgs>(e.Network).Sender;

                    // TODO: this is a WORKAROUND
                    // FAILSAFE in case player is picking items too fast resulting in picking the same item before the server processes the input
                    GameObjectSlot existingSlot;
                    if (HasObject(parent, o => o == obj, out existingSlot))
                    {
                        //parent.PostMessage(e.Network, Message.Types.Hold, w => w.Write(obj.NetworkID));
                        return true;
                    }
                    GiveObject(e.Network, parent, obj.ToSlot());

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


                //case Message.Types.Hold:
                //    obj = e.Parameters.Translate<SenderEventArgs>(e.Network).Sender;//e.TargetArgs.Object;// e.Data.Translate<SenderEventArgs>(e.Network).Sender;
                //    objSlot = obj.Exists ? obj.ToSlot() : (from slot in this.Children where slot.HasValue select slot).FirstOrDefault(foo => foo.Object == obj);// PersonalInventoryComponent.GetFirstOrDefault(parent, foo => foo == obj);
                //    //Hold(e.Network, parent, objSlot);

                //    return true;

                case Message.Types.DropInventoryItem:
                    //e.Data.Translate(e.Network, r =>
                    //{
                    slotID = (int)e.Parameters[0];// r.ReadInt32();
                    amount = (int)e.Parameters[1]; //r.ReadInt32();
                    DropInventoryItem(parent, slotID, amount);
                    return true;

                //     case Message.Types.Throw:
                //         if (!Holding.HasValue)
                //             return true;

                //         Vector3 dir = e.Data.Translate<DirectionEventArgs>(e.Network).Direction;
                //         Vector3 speed = dir * 0.1f + parent.Velocity;

                //         //e.Network.Spawn(Holding.Take(), new Position(parent.Global + new Vector3(0, 0, (float)parent["Physics"]["Height"]), speed));
                //         GameObject newobj = Holding.Take();
                //         e.Network.Spawn(newobj, new Position(e.Network.Map, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height), speed));
                //         //e.Network.Sync(newobj);
                //         if (!this.Holding.HasValue)
                //             parent.GetComponent<ActorSpriteComponent>().UpdateHeldObject(null);
                ////         parent.PostMessage(e.Network, Message.Types.Thrown, w => { });
                //         return true;

                case Message.Types.CraftObject:
                    BlueprintComponent bpComp = e.Parameters[0] as BlueprintComponent;
                    Blueprint bp = bpComp.Blueprint;
                    if (!HasItems(parent, bp[0]))
                        return true;
                    foreach (KeyValuePair<GameObject.Types, int> mat in bp[0])
                        InventoryComponent.RemoveObjects(parent, foo => foo.ID == mat.Key, mat.Value);
                    throw new NotImplementedException();
                    //parent.PostMessage(Message.Types.Receive, null, GameObject.Create(bp.ProductID).ToSlot());
                    return true;

                default:
                    return false;
            }
        }

        private static GameObject DropInventoryItem(GameObject parent, int slotID, int amount)
        {
            GameObject obj;
            var children = parent.GetChildren();
            GameObjectSlot childslot = children[slotID];
            //GameObject toDrop;
            //if(!slot.Take(out toDrop))
            //    return;
            //e.Network.Spawn(toDrop, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));

            if (amount < childslot.Object.StackSize)
            {
                obj = childslot.Object.Clone();
                obj.StackSize = amount;
            }
            else
                obj = childslot.Object;
            //e.Network.Spawn(slot.Object, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));
            parent.Net.Spawn(obj, parent.Global + new Vector3(0, 0, parent.GetComponent<PhysicsComponent>().Height));
            //slot.Clear();
            childslot.StackSize -= amount;
            //});
            return obj;
        }

        static public GameObjectSlot FindFirst(GameObject parent, Func<GameObject, bool> condition)
        {
            PersonalInventoryComponent comp;
            if (!parent.TryGetComponent<PersonalInventoryComponent>(out comp))
                return null;
            var contents = comp.GetContents();
            //var found = comp.Slots.GetNonEmpty().FirstOrDefault(foo => condition(foo.Object));
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
                        int slotID = (int)r.ReadByte();// r.ReadInt32();
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
                //StoreHauled(parent);
                //this.Haul(parent, slot);
                this.HaulSlot.Swap(slot);
            }
            return;

            var action = obj.GetInventoryActions(parent, slot).FirstOrDefault();
            if (action != null)
                action.Action();
        }

        static public bool HasItems(GameObject actor, Dictionary<GameObject.Types, int> items)
        {
            foreach (KeyValuePair<GameObject.Types, int> mat in items)
                if (InventoryComponent.GetAmount(actor, foo => foo.ID == mat.Key) < mat.Value)
                    return false;
            return true;
        }

        public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        {
            //List<Interaction> list = e.Parameters[0] as List<Interaction>;
            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.DropOn, parent, "Give"));
            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Give, parent, "Give"));
        }

        static public void InsertOld(GameObject actor, GameObjectSlot objSlot)
        {
            //var inv = actor.GetComponent<PersonalInventoryComponent>();
            //if (objSlot.Object.GetPhysics().Size != ObjectSize.Inventoryable)
            //{
            //    actor.Net.PopLoot(objSlot.Object, actor);
            //    objSlot.StackSize--;
            //    return;
            //}
            //inv.Children.InsertObject(actor.Net, objSlot);
            var inv = actor.GetComponent<PersonalInventoryComponent>();
            inv.PickUpOld(actor, objSlot);
        }
        static public bool StoreHauled(GameObject parent)
        {
            var inv = parent.GetComponent<PersonalInventoryComponent>();
            if (inv.HaulSlot.Object == null)
                return false;
            //if (!inv.PickUpOld(parent, inv.HaulSlot))
            if (!inv.Slots.InsertObject(inv.HaulSlot))

            {
                // throw? or return false and raise event so we can handle it and display a message : not enough space?
                //inv.Throw(parent, Vector3.Zero);
                parent.Net.EventOccured(Message.Types.NotEnoughSpace, parent);
                return false;
            }
            return true;
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
                if (haulObj.ID == objSlot.Object.ID)
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

            //if (objSlot.Object == null)
            //    return true;

            //if (inv.HaulSlot.HasValue)
            //{
            //    if (!inv.Slots.InsertObject(inv.HaulSlot))
            //        return false;
            //}
            //inv.Haul(parent, objSlot);
            //return true;
        }
        static public bool Receive(GameObject parent, GameObjectSlot objSlot, bool report = true)
        {
            var inv = parent.GetComponent<PersonalInventoryComponent>();
            // TODO: if can't receive, haul item instead or drop on ground?
            //return inv.Slots.InsertObject(objSlot);
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
        //static public bool Insert(GameObject parent, GameObjectSlot objSlot)
        //{
        //    var inv = parent.GetComponent<PersonalInventoryComponent>();
        //    // TODO: if can't receive, haul item instead or drop on ground?
        //    return inv.Slots.InsertObject(objSlot);
        //}
        //static public bool PickUp(GameObject parent, GameObjectSlot objSlot)
        //{
        //    var inv = parent.GetComponent<PersonalInventoryComponent>();
        //    if (objSlot.Object == null)
        //        return true;

        //    if (inv.HaulSlot.HasValue)
        //    {
        //        if (!inv.Slots.InsertObject(inv.HaulSlot))
        //            return false;
        //    }
        //    //this.HaulSlot.SetObject(objSlot.Object);
        //    inv.Haul(parent, objSlot);
        //    //this.Slots.InsertObject(objSlot);//, out this.HaulIndex);

        //    return true;
        //}
        public bool InsertOld(GameObjectSlot objSlot)
        {
            if (objSlot.Object == null)
                return true;

            // TRYING TO MAKE HAULABLE ITEMS INVENTORYABLE
            //if (objSlot.Object.GetPhysics().Size != ObjectSize.Inventoryable)
            //{
            //    //this.Parent.Net.PopLoot(objSlot.Object, this.Parent);
            //    this.Parent.Net.PopLoot(objSlot.Take(), this.Parent);
            //    //objSlot.StackSize--;
            //    return true;
            //}

            //VERY OLD BELOW
            //if (objSlot.Object.GetPhysics().Size == ObjectSize.Haulable)
            //{
            //    if (this.Holding.HasValue)
            //    {
            //        // drop incoming item or do nothing?
            //        e.Network.Spawn(objSlot.Take(), new Position(parent.Global + parent.GetPhysics().Height * Vector3.UnitZ, parent.Velocity));
            //        // or pop loot?
            //        return true;
            //    }
            //    e.Network.Despawn(objSlot.Object);
            //    this.Holding.Swap(objSlot);

            //    return true;
            //}
            this.Slots.InsertObject(objSlot, out this.HaulIndex);
            //this.Children.InsertObject(this.Parent.Net, objSlot);
            return true;
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
         //   slots = new List<GameObjectSlot>();
            foreach (var container in invComp.Containers)
                foreach (GameObjectSlot slot in container)
                    if (slot.Object != null)
                        if (filter(slot.Object))
                            slots.Enqueue(slot);
            return slots.Count > 0;
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

            //InventoryComponent inv;
            //if (!subject.TryGetComponent<InventoryComponent>("Inventory", out inv))
            //    return false;

            //foreach (var container in inv.Containers)
            //    foreach (GameObjectSlot slot in container)
            //        if (slot.Object != null)
            //            if (filter(slot.Object))
            //                return true;
            //return false;
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

        static public bool GiveObject(Net.IObjectProvider net, GameObject receiver, GameObjectSlot objSlot)
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
            GetSlots(receiver, foo => foo.ID == objSlot.Object.ID, slots);
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
                    if (obj.Exists)
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
                if (current.ID == objSlot.Object.ID)
                {
                    current.StackSize++;
                    objSlot.Object.Despawn();
                    net.DisposeObject(objSlot.Object);
                    return true;
                }

            // else
            // drop currently hauled object and pick up new one
            //hauling.Clear();
            this.Throw(Vector3.Zero, parent); //or store carried object in backpack? (if available)

            net.Despawn(objSlot.Object);
            this.HaulSlot.Object = objSlot.Object;
            //this.HaulSlot.Swap(objSlot);
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
        bool Throw(Net.IObjectProvider net, Vector3 velocity, GameObject parent, bool all)
        {
            // throws hauled object, if hauling nothing throws equipped object, make it so it only throws hauled object?
            //if (!this.Holding.HasValue)
            //    return false;
            //GameObject newobj = this.Holding.Take();

            //GameObjectSlot hauling = this.EquipmentSlots[GearType.Hauling];

            var slot = this.HaulSlot;
            GameObjectSlot hauling = slot;// this.Slot;
            if (hauling.Object == null)
                return false;
            //GameObject newobj = hauling.Take();

            GameObject newobj = all ? hauling.Object : hauling.Take();


            newobj.Global = parent.Global + new Vector3(0, 0, parent.GetPhysics().Height);
            newobj.Velocity = velocity;
            newobj.Physics.Enabled = true;
            net.Spawn(newobj);

            if (all)
                hauling.Clear();
            return true;
        }

        static List<GameObjectSlot> GetContents(GameObject actor)
        {
            var inv = actor.GetComponent<PersonalInventoryComponent>();
            var list = new List<GameObjectSlot>();
            list.Add(inv.HaulSlot);
            foreach (var c in inv.Slots.Slots)
                list.Add(c);
            return list;
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
                //var container = parent.GetComponent<PersonalInventoryComponent>().Slots.Slots;
            var container = this.GetContents();
            var parent = this.Slots.Parent;
            var net = parent.Net;
            //var reqs = this.Construction.GetReq();
            
            foreach (var item in reqs)
            {
                int amountRemaining = item.Max;
                GameObject current;
                foreach (var found in from slot in container where slot.HasValue where (int)slot.Object.ID == item.ObjectID select slot)
                {
                    current = found.Object;
                    int amountToTake = Math.Min(found.Object.StackSize, amountRemaining);
                    amountRemaining -= amountToTake;
                    //found.Object.StackSize -= amountToTake;

                    //if(found.Object.StackSize == 0)
                    if (amountToTake == found.Object.StackSize)
                    {
                        net.Despawn(found.Object);
                        net.DisposeObject(found.Object);
                        found.Clear();
                        //net.SyncDisposeObject(found);
                    }
                    else
                        found.Object.StackSize -= amountToTake;

                    if (amountRemaining == 0)
                    {
                        Net.Client.Instance.EventOccured(Message.Types.ItemLost, parent, current, item.Max);
                        break;
                    }
                }
            }
            return true;
        }

        public bool Has(List<ItemRequirement> reqs)
        {
            //var container = actor.GetComponent<PersonalInventoryComponent>().Slots.Slots;
            var container = this.GetContents();

            foreach (var item in reqs)
            {
                int amountFound = 0;
                foreach (var found in from slot in container where slot.HasValue where (int)slot.Object.ID == item.ObjectID select slot.Object)
                    amountFound += found.StackSize;
                if (amountFound < item.Max)
                    return false;
            }
            return true;
        }
        public override object Clone()
        {
            PersonalInventoryComponent comp = new PersonalInventoryComponent((byte)this.Slots.Slots.Count);
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

        public override void Write(BinaryWriter writer)
        {
            //writer.Write(this.Capacity);
            //this.Children.Write(writer);
            this.Slots.Write(writer);
            this.HaulSlot.Write(writer);
        }
        public override void Read(BinaryReader reader)
        {
            //this.Capacity = reader.ReadByte();
            //this.Children = ItemContainer.Create(this.Parent, reader);
            this.Slots.Read(reader);
            this.HaulSlot.Read(reader);

           // this.Children.Parent = this.Parent;
        }

        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();

            //data.Add(new SaveTag(SaveTag.Types.Compound, "Container", this.Children.Save()));
            //data.Add(new SaveTag(SaveTag.Types.Compound, "Container", this.Children.Save()));
            data.Add(new SaveTag(SaveTag.Types.Compound, "Inventory", this.Slots.Save()));
            var isHauling = this.HaulSlot.Object != null;
            data.Add(new SaveTag(SaveTag.Types.Bool, "IsHauling", isHauling));
            if (isHauling)
            data.Add(new SaveTag(SaveTag.Types.Compound, "Hauling", this.HaulSlot.Save()));

            return data;
        }
        internal override void Load(SaveTag data)
        {
            //this.Children = ItemContainer.Create(this.Parent, data["Container"]);
            //this.Children = ItemContainer.Create(this.Parent, data["Container"]);
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
    }
}
