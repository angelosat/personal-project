using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    [Obsolete]
    class InventoryComponent : EntityComponent
    {
        public GameObjectSlot Holding { get { return (GameObjectSlot)this["Holding"]; } set { this["Holding"] = value; } }
        public List<ItemContainer> Containers = new();

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
                
                default:
                    return false;
            }
        }

        bool Hold(IObjectProvider net, GameObject parent, GameObjectSlot objSlot)
        {
            throw new Exception();
            
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

        static public void UseHeldObject(GameObject actor, TargetArgs target)
        {
            GameObjectSlot obj;
            if (!InventoryComponent.TryGetHeldObject(actor, out obj))
                return;
            throw new NotImplementedException();
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

                    }
            return false;
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
            if (invComp is null)
                return;
            invComp.Holding.StackSize -= amount;
            throw new NotImplementedException();
            //parent.PostMessage(Message.Types.Dropped, parent);
        }
        static public bool ConsumeEquipped(GameObject parent, Func<GameObjectSlot, bool> check, int amount = 1)
        {
            InventoryComponent invComp = parent.Components.Values.FirstOrDefault(foo => foo is InventoryComponent) as InventoryComponent;
            if (invComp is null)
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


        static public void PollStats(GameObject obj, StatCollection list)
        {
            //InventoryComponent inv;
            //if (!obj.TryGetComponent<InventoryComponent>("Inventory", out inv))
            //    return;
            EquipComponent.GetStats(InventoryComponent.GetHeldObject(obj), list);
        }

        
    }
}
