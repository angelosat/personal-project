using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    public enum FilterType { None, Exclude, Include }
    public class ObjectFilter : Dictionary<string, List<GameObject.Types>>
    {
        //public List<string> ObjectTypes = new List<string>();
        //public List<GameObject.Types> ObjectIDs = new List<GameObject.Types>();

        public ObjectFilter Clone()
        {
            ObjectFilter f = new ObjectFilter(Type);
            foreach (KeyValuePair<string, List<GameObject.Types>> filter in this)
                //  foreach(GameObject.Types type in filter.Value)
                f[filter.Key] = filter.Value.ToList();// new List<GameObject.Types>(filter.Value);
            return f;
        }


        public FilterType Type;
        public ObjectFilter(FilterType type = FilterType.None)
        {
            this.Type = type;
        }

        //public ObjectFilter SetObjectTypes(params string[] types)
        //{
        //    ObjectTypes.Clear();
        //    foreach (string type in types)
        //        ObjectTypes.Add(type);
        //    return this;
        //}
        //public ObjectFilter SetObjectIDs(params GameObject.Types[] types)
        //{
        //    ObjectTypes.Clear();
        //    foreach (GameObject.Types type in types)
        //        ObjectIDs.Add(type);
        //    return this;
        //}

        public virtual bool Apply(GameObject obj)
        {
            if (obj.Type != ObjectType.Material)
                return false;
            List<GameObject.Types> list;
            switch (Type)
            {
                case FilterType.Include:
                    if (!this.TryGetValue(obj.Type, out list))
                        return false;
                    if (!list.Contains(obj.ID))
                        return false;
                    break;
                case FilterType.Exclude:
                    if (this.TryGetValue(obj.Type, out list))

                        if (list.Contains(obj.ID))
                            return false;

                    //return true;

                    //break;
                    //case FilterType.None:
                    //    if (obj.Type == ObjectType.Material)
                    //        return true;
                    break;
                default:
                    break;
            }
            return true;
        }

        /// <summary>
        /// Set object filters.
        /// </summary>
        /// <param name="operation">True to add, false to remove filter.</param>
        /// <param name="objects">An param array of objects to set as filters.</param>
        public void Set(bool operation, params GameObject[] objects)
        {
            List<GameObject.Types> list;
            foreach (GameObject obj in objects)
            {
                switch (operation)
                {
                    case true:
                        if (!this.TryGetValue(obj.Type, out list))
                        {
                            this[obj.Type] = new List<GameObject.Types>() { obj.ID };
                            continue;
                        }
                        if (!list.Contains(obj.ID))
                            list.Add(obj.ID);
                        break;
                    default:
                        if (!this.TryGetValue(obj.Type, out list))
                            continue;
                        if (list.Contains(obj.ID))
                            list.Remove(obj.ID);
                        if (list.Count == 0)
                            this.Remove(obj.Type);
                        break;
                }
            }
        }

        //public bool Apply(GameObject obj)
        //{
        //    switch (Type)
        //    {
        //        case FilterType.Include:
        //            //if (!this.Contains(obj.Type))
        //            //    return false;
        //            if (!ObjectTypes.Contains(obj.Type))
        //                return false;
        //            if (!ObjectIDs.Contains(obj.ID))
        //                return false;
        //            break;
        //        case FilterType.Exclude:
        //            //if (this.Contains(obj.Type))
        //            //    return false;
        //            if (ObjectTypes.Contains(obj.Type))
        //                return false;
        //            if (ObjectIDs.Contains(obj.ID))
        //                return false;
        //            break;
        //        default:
        //            break;
        //    }
        //    return true;
        //}
    }



    class ContainerComponent : Component
    {
        public override string ComponentName
        {
            get { return "Container"; }
        }
        public byte Capacity { get { return (byte)this["Capacity"]; } set { this["Capacity"] = value; } }

      //  public ItemContainer Slots { get { return (ItemContainer)this["Materials"]; } set { this["Materials"] = value; } }

        public ObjectFilter FilterOld { get { return (ObjectFilter)this["FilterOld"]; } set { this["FilterOld"] = value; } }
        public ObjectFilter2 Filter { get { return (ObjectFilter2)this["Filter"]; } set { this["Filter"] = value; } }

        public List<ItemContainer> Containers { get { return (List<ItemContainer>)this["Containers"]; } set { this["Containers"] = value; } }

        public ItemContainer this[byte containerID]
        {
            get
            {
                return this.Containers[containerID];
            }
            set
            {
                this.Containers[containerID] = value;
            }
        }

        public byte Add(ItemContainer container)
        {
            container.ID = (byte)this.Containers.Count;
            this.Containers.Add(container);
           // container.Parent = this;
            return container.ID;
        }
        public bool Remove(ItemContainer container)
        {
            return this.Containers.Remove(container);
        }

        public ContainerComponent()
        {
            this.Capacity = 0;
          //  this["Materials"] = new List<GameObjectSlot>();
   //         this["Users"] = new List<GameObject>();
            this.Containers = new List<ItemContainer>();
            this.FilterOld = new ObjectFilter();
        }

        public ContainerComponent(byte capacity, ObjectFilter2 filter = null)
        {
            this.Capacity = capacity;
          //  this.Slots = new ItemContainer(capacity);
            FilterOld = new ObjectFilter();
            this.Filter = filter ?? new ObjectFilter2(foo => true);
        }

        public void Set(byte containerID, byte slotID, Action<GameObjectSlot> valueUpdater)
        {
            valueUpdater(this[containerID][slotID]);
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            GameObjectSlot objSlot;
            GameObject obj;
            switch (e.Type)
            {
                case Message.Types.ContainerOperation:
                    //parent.Global.GetChunk(e.Network.Map).Invalidate();//.Saved = true;
                    e.Network.Map.GetChunk(parent.Global).Invalidate();//.Saved = true;
                    Net.IObjectProvider net = e.Network;
                    ContainerOperationArgs args = e.Data.Translate<ContainerOperationArgs>(e.Network);
                    GameObject source = args.SourceEntity.Object;
                    GameObject sourceObj = args.Object.Object;
                    GameObjectSlot targetSlot = this[args.TargetContainerID][args.TargetSlotID];
                    GameObjectSlot sourceSlot = source.GetComponent<ContainerComponent>()[args.SourceContainerID][args.SourceSlotID];
                    int amount = args.Amount;

                    if (sourceObj.IsNull())
                    {
                        // object originating from a splitstack operation probably, instantiate it on network and resend message to parent
                        GameObject newObj  =sourceSlot.Object.Clone();
                        e.Network.InstantiateObject(newObj);
                        args.Object = new TargetArgs(newObj);
                        e.Network.SyncEvent(parent, Message.Types.ContainerOperation, args.Write);
                        return true;
                    }
                    if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
                    {
                        targetSlot.Set(sourceObj, amount);
                        sourceSlot.StackSize -= amount;
                        return true;
                    }
                    if (sourceSlot.Object.ID == targetSlot.Object.ID)
                    {
                        if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
                        {
                            targetSlot.StackSize += sourceSlot.StackSize;
                            e.Network.DisposeObject(sourceSlot.Object.Network.ID);
                            sourceSlot.Clear();
                            //merge slots
                            return true;
                        }
                    }
                    else
                        if (amount < sourceSlot.StackSize)
                            return true;

                    targetSlot.Swap(sourceSlot);
                    return true;

                   

                //    /// new iteration where slot clears on start of dragdrop operation, and the object transferred exists only on the dragdrop class
                //case Message.Types.ContainerOperation:
                //    parent.Global.GetChunk(e.Network.Map).Changed = true;
                //    Net.IObjectProvider net = e.Network;
                //    ArrangeInventoryEventArgs args = e.Data.Translate<ArrangeInventoryEventArgs>(e.Network);
                //    GameObjectSlot targetSlot = this[args.TargetContainerID][args.TargetSlotID];
                //    int amount = args.Amount;
                //    GameObject source = args.SourceObject.Object;
                    
                //    obj = args.Object.Object;

                //    // TODO: HANDLE SLOT OVERFLOW
                //    if (!obj.IsNull())
                //        if (targetSlot.HasValue)
                //            if (obj.ID == targetSlot.Object.ID)
                //            {
                //                // combine stacks and dispose added object
                //                //(DateTime.Now.ToString() + "combine").ToConsole();
                //                targetSlot.StackSize += amount;
                //                net.DestroyObject(obj);
                //                return true;
                //            }

                //    targetSlot.Set(obj, amount);
                //    return true;

                //    if (targetSlot.HasValue)
                //    {
                //        // if objects have the same id, DESTROY source object from the network if source stacksize == 0
                //        if (obj.ID == targetSlot.Object.ID)
                //        {
                //            int distFromMax = Math.Max(0, targetSlot.StackSize + amount - targetSlot.StackMax);
                //            int amountTransferred = amount - distFromMax;
                //            targetSlot.StackSize += amountTransferred;
                //            //   sourceSlot.StackSize -= amountTransferred;

                //            // if source stack == 0, dispose object from network
                //            //e.Network.DisposeObject(args.Object.Object);
                //            net.DestroyObject(args.Object.Object);
                //            return true;
                //        }
                //    }
                //    else
                //    {
                //        // if object and source have the same network id, request new instantiated object from server
                //        if (source != null)
                //            if (obj.NetworkID == source.NetworkID)
                //            {
                //                net.InstantiateInContainer(obj, parent, targetSlot.Parent.ID, targetSlot.ID, (byte)amount);
                //                return true;
                //            }
                //        targetSlot.Set(obj, amount);
                //    }
                //    return true;

                    /*
                     */

                //case Message.Types.ContainerOperation:
                //    ArrangeInventoryEventArgs args = e.Data.Translate<ArrangeInventoryEventArgs>(e.Network);
                //    GameObjectSlot sourceSlot =
                //        (from slot in parent.GetChildren()
                //         where slot.HasValue
                //         where slot.Object == args.Object.Object
                //         select slot).SingleOrDefault();
                //    if (sourceSlot.IsNull())
                //        throw new ArgumentException();

                //    GameObjectSlot targetSlot = this[args.TargetContainerID][args.TargetSlotID];

                //    if (!sourceSlot.HasValue)
                //        return true;

                //    int amount = args.Amount;

                //    if(targetSlot.HasValue)
                //    // if objects have the same id, DESTROY source object from the network if source stacksize == 0
                //        if (sourceSlot.Object.ID == targetSlot.Object.ID)
                //        {
                //            int distFromMax = Math.Max(0, targetSlot.StackSize + amount - targetSlot.StackMax);
                //            int amountTransferred = amount - distFromMax;
                //            targetSlot.StackSize += amountTransferred;
                //            sourceSlot.StackSize -= amountTransferred;

                //            // if source stack == 0, dispose object from network
                //            //e.Network.DisposeObject(args.Object.Object);
                //            e.Network.DestroyObject(args.Object.Object.NetworkID);
                //            return true;
                //        }
                //    // if moving the whole stack, swap slots
                //    if (amount == sourceSlot.StackSize)
                //    {
                //        sourceSlot.Swap(targetSlot);
                //        return true;
                //    }
                //    //if (targetSlot.HasValue)
                //    //    return true;

                //    // if moving part of a stack to a new slot, network instantiate a new object of the same type
                //    sourceSlot.StackSize -= amount;
                //    //targetSlot.Object = sourceSlot.Object;

                //    // WARNING!!! must probably create it only on server and send to the clients
                //    //GameObject o = e.Network.InstantiateObject(sourceSlot.Object.Clone());
                //    e.Network.InstantiateInContainer(sourceSlot.Object.Clone(), parent, targetSlot.Parent.ID, targetSlot.ID, (byte)amount);
                //    //targetSlot.Object = o;
                //    //targetSlot.StackSize = amount;
                //    return true;

                case Message.Types.AddItem:
                    obj = e.Parameters[0] as GameObject;
                    byte containerID = (byte)e.Parameters[1];
                    byte slotID = (byte)e.Parameters[2];
                    amount = (byte)e.Parameters[3];

                    this[containerID][slotID].Set(obj, amount);

                    return true;

                case Message.Types.ReceiveItem:
                    e.Data.Translate(e.Network, r =>
                    {
                        obj = TargetArgs.Read(e.Network, r).Object;
                        amount = r.ReadByte();
                        GameObjectSlot available =
                            (from slot in parent.GetChildren()
                             where !slot.HasValue
                             select slot).FirstOrDefault();
                        if (available.IsNull())
                        {
                            // drop if no space available
                            //e.Network.Spawn(obj, new Position(e.Network.Map, parent.Global + parent.GetComponent<PhysicsComponent>().Height * Vector3.UnitZ));
                            e.Network.Spawn(obj, new WorldPosition(e.Network.Map, parent.Global + parent.GetComponent<PhysicsComponent>().Height * Vector3.UnitZ));

                            return;
                        }
                        available.Set(obj, amount);
                    });
                    return true;

                //case Message.Types.ContainerOperation:
                //    ArrangeInventoryEventArgs args = e.Data.Translate<ArrangeInventoryEventArgs>(e.Network);
                //    //GameObjectSlot sourceSlot = args.SourceEntity.Object.GetComponent<InventoryComponent>().Containers[args.SourceContainerID][args.SourceSlotID];
                //    GameObjectSlot sourceSlot = args.SourceEntity.Object.GetComponent<ContainerComponent>()[args.SourceContainerID][args.SourceSlotID];
                //    GameObjectSlot targetSlot = this[args.TargetContainerID][args.TargetSlotID];

                //    if (!sourceSlot.HasValue)
                //        return true;

                //    int amount = args.Amount;
                //    if (sourceSlot.Object == targetSlot.Object)
                //    {
                //        int d = Math.Max(0, targetSlot.StackSize + amount - targetSlot.StackMax);
                //        int a = amount - d;
                //        targetSlot.StackSize += a;
                //        sourceSlot.StackSize -= a;
                //        return true;
                //    }
                //    if (amount == sourceSlot.StackSize)
                //    {
                //        sourceSlot.Swap(targetSlot);
                //        return true;
                //    }
                //    if (targetSlot.HasValue)
                //        return true;
                //    sourceSlot.StackSize -= amount;
                //    targetSlot.Object = sourceSlot.Object;
                //    targetSlot.StackSize = amount;
                //    return true;




                //case Message.Types.ArrangeInventory:
                //    GameObjectSlot source = e.Parameters[0] as GameObjectSlot;

                //    //int index = (int)e.Parameters[2];
                //    //Give(parent, source, target, index);
                //    if (!source.HasValue)
                //        return true;
                //    GameObjectSlot target = e.Parameters[1] as GameObjectSlot;
                //    int amount = (int)e.Parameters[2];
                //    if (source.Object == target.Object)
                //    {
                //        int d = Math.Max(0, target.StackSize + amount - target.StackMax);
                //        int a = amount - d;
                //        target.StackSize += a;
                //        source.StackSize -= a;
                //        return true;
                //    }
                //    if (amount == source.StackSize)
                //    {
                //        source.Swap(target);
                //        return true;
                //    }
                //    if (target.HasValue)
                //        return true;
                //    source.StackSize -= amount;
                //    target.Object = source.Object;
                //    target.StackSize = amount;
                //    return true;

                case Message.Types.Open:
                    //e.Sender.PostMessage(Message.Types.Interface, parent);
                    e.Network.PostLocalEvent(e.Sender, ObjectEventArgs.Create(Message.Types.Interface, new TargetArgs(parent)));
                    return true;
                //case Message.Types.ContainerClose:
                //    Users.Remove(e.Sender);
                //    return true;
                case Message.Types.Give:
                    objSlot = e.Parameters[1] as GameObjectSlot;
                    Give(parent, e.Sender, objSlot);
                    return true;
                case Message.Types.DropOn:
                    InventoryComponent inv;
                    if (!e.Sender.TryGetComponent<InventoryComponent>("Inventory", out inv))
                        return false;
                    if (inv.GetProperty<GameObjectSlot>("Holding").Object == null)
                        return false;
                    GameObject hauling = inv.GetProperty<GameObjectSlot>("Holding").Object;
                   // if (
                    Give(parent, e.Sender, inv.GetProperty<GameObjectSlot>("Holding"));
                //)//.Object))
                    //{
                        //  Map.RemoveObject(hauling);
              //          hauling.Remove();
                        return true;
                    //}
                    //return false;
                case Message.Types.Insert:
                    GameObjectSlot haulSlot = e.Sender["Inventory"]["Holding"] as GameObjectSlot;

                    obj = haulSlot.Object;
                    if (Give(parent, e.Sender, haulSlot))
                    {
                        e.Network.Despawn(obj);
                        //obj.Remove();


                        //e.Sender.PostMessage(Message.Types.ModifyNeed, parent, "Work", 20);
                        //e.Sender.PostMessage(Message.Types.Dropped, parent);

                        //e.Network.PostLocalEvent(e.Sender, LocalEventArgs.Create(Message.Types.ModifyNeed, new TargetArgs(parent), w =>
                        //{
                        //    w.Write(Encoding.ASCII.GetBytes("Work"));
                        //    w.Write(20);
                        //}));

                        e.Network.PostLocalEvent(e.Sender, ObjectEventArgs.Create(Message.Types.ModifyNeed, new TargetArgs(parent), "Work", 20));
                        e.Network.PostLocalEvent(e.Sender, ObjectEventArgs.Create(Message.Types.Dropped));
                    }
                    return true;
                case Message.Types.Extract:
                    //foreach (GameObjectSlot sl in this.Slots)
                    //    if (sl.HasValue)
                    //        if (!FilterOld.Apply(sl.Object))
                    //        {
                    //          //  Loot.PopLoot(parent, sl.Object);
                    //            e.Network.PopLoot(sl.Object, parent.Global, parent.Velocity);
                    //            sl.Clear();
                    //            break;
                    //        }
                    return true;

                case Message.Types.Work:
                    //haulSlot = e.Sender["Inventory"]["Holding"] as GameObjectSlot;
                    //if (!haulSlot.HasValue)
                    //{
                    //    Queue<GameObjectSlot> emptySlots;
                    //    if (TryGetEmptySlots(out emptySlots))
                    //        //e.Sender.PostMessage(Message.Types.Work, parent, new Predicate<GameObject>(foo => FilterOld.Apply(foo)), FilterOld, emptySlots.Count);
                    //        e.Network.PostLocalEvent(e.Sender, ObjectEventArgs.Create(Message.Types.Work, new TargetArgs(parent), new Predicate<GameObject>(foo => FilterOld.Apply(foo)), FilterOld, emptySlots.Count));
                    //    return true;
                    //}
                    return true;

                //case Message.Types.Query:
                //    Query(parent, e);
                //    return true;
                case Message.Types.ManageEquipment:
                    //e.Sender.PostMessage(Message.Types.UIOwnership, parent, new Predicate<GameObject>(foo => foo.Components.ContainsKey("Container")));
                    e.Network.PostLocalEvent(e.Sender, ObjectEventArgs.Create(Message.Types.UIOwnership, new TargetArgs(parent), new Predicate<GameObject>(foo => foo.Components.ContainsKey("Container"))));
                    return true;
                default:
                    return true;
            }
        }

        public override void Query(GameObject parent, List<InteractionOld> list)
        {
            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Open, parent, "Open"));

            //list.Add(new Interaction(new TimeSpan(0, 0, 0, 0, 500), Message.Types.Insert, name: "Insert", source: parent,// need: new Need() { Name = "Work", Value = 20 },
            //    //cond: new InteractionCondition("Holding", true, AI.PlanType.FindNearest, precondition: Filter.Apply, condition: foo => InventoryComponent.IsHauling(foo, Filter.Apply)),
            //    //cond: new InteractionCondition("Holding", true, AI.PlanType.FindNearest,
            //    //    precondition: foo => Filter.Apply(foo) || ProductionComponent.CanProduce(foo, Filter.Apply),
            //    //    finalCondition: agent => InventoryComponent.IsHauling(agent, Filter.Apply)),
            //    cond:
            //    new InteractionConditionCollection(
            //        new InteractionCondition((actor, target) => HasEmptySlots(), "No empty slots"),
            //    //actorCond:
            //    //new InteractionConditionCollection(
            //        new InteractionCondition((actor, target) => InventoryComponent.IsHauling(actor, FilterOld.Apply), "Invalid carried object",
            //            new Precondition("Holding", i => FilterOld.Apply(i.Source), AI.PlanType.FindNearest),
            //            new Precondition("Production", i => ProductionComponent.CanProduce(i.Source, FilterOld.Apply), AI.PlanType.FindNearest)
            //        ))//,
            //    //   effect: new InteractionEffect("Work")
            // ));




            //if (HasEmptySlots())
            //    //list.Add(new Interaction(TimeSpan.Zero, Message.Types.Insert, "Insert", need: Filter.Type != FilterType.None ? new Need() { Name = "Work", Value = 20 } : null, req: new InteractionRequirement(Message.Types.NeedItem, agent => InventoryComponent.IsHauling(agent, foo => Filter.Apply(foo)), new Predicate<GameObject>(foo => Filter.Apply(foo)))));// "Work" : ""));
            //    list.Add(new Interaction(TimeSpan.Zero, Message.Types.Insert, "Insert", need: new Need() { Name = "Work", Value = 20 }, req: new InteractionRequirement(Message.Types.NeedItem, agent => InventoryComponent.IsHauling(agent, foo => Filter.Apply(foo)), new Predicate<GameObject>(foo => Filter.Apply(foo)))));// "Work" : ""));


            //if there are items that don't belong, advertise an extract interaction
            //foreach (GameObjectSlot slot in this.Slots)
            //{
            //    if (slot.HasValue)
            //        if (!FilterOld.Apply(slot.Object))
            //        {
            //            list.Add(new Interaction(new TimeSpan(0, 0, 0, 0, 500), Message.Types.Extract, name: "Extract", source: parent,
            //                //effect: new InteractionEffect[] { new InteractionEffect("Work") })
            //                effect: new NeedEffectCollection() { new NeedEffect("Work", 20) }// InteractionEffect("Work"))
            //                ));// need: new Need() { Name = "Work", Value = 21 }));
            //            break;
            //        }
            //}


            //GameObjectSlot objSlot = e.Parameters.ElementAtOrDefault(4) as GameObjectSlot;//[1] as GameObjectSlot;
            //if (objSlot == null)
            //    return;
            //if (!objSlot.HasValue)
            //    return;
            //if (!Filter.Apply(objSlot.Object))
            //    return;

            //list.Add(new Interaction(TimeSpan.Zero, Message.Types.Give, parent, "Deposit"));

        }

        public override object Clone()
        {
            //ContainerComponent comp = new ContainerComponent(Capacity, Filter.Clone());// { CanChangeFilter = this.CanChangeFilter };
            //for (int i = 0; i < Capacity; i++)
            //    comp.Slots[i] = this.Slots[i].Clone();
            //return comp;
            return new ContainerComponent();
        }


    }
}
