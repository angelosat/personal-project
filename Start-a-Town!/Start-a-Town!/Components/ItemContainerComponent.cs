using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class ItemContainerComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "ItemContainer"; }
        }

        ItemContainer Children { get { return (ItemContainer)this["Children"]; } set { this["Children"] = value; } }
        public byte Capacity { get { return (byte)this["Capacity"]; } set { this["Capacity"] = value; } }


        public ItemContainerComponent Initialize(byte capacity)
        {
            //this.Children = new ItemContainer(capacity);
            this.Capacity = capacity;
            return this;
        }

        public override void OnObjectCreated(GameObject parent)
        {
            this.Children = new ItemContainer(this.Capacity, () => parent.ChildrenSequence);
            this.Children.Parent = parent;
        }
        public override void OnObjectSynced(GameObject parent)
        {
            this.Children = new ItemContainer(this.Capacity, () => parent.ChildrenSequence);
            this.Children.Parent = parent;
        }
        public override void GetChildren(List<GameObjectSlot> list)
        {
            list.AddRange(this.Children);
        }

        public ItemContainerComponent()
        {
            this.Children = new ItemContainer();
        }

        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        //{
        //    GameObjectSlot objSlot;
        //    GameObject obj;
        //    switch (e.Type)
        //    {
        //        case Message.Types.ContainerOperation:
        //            parent.Global.GetChunk(e.Network.Map).Changed = true;
        //            IObjectProvider net = e.Network;
        //            ContainerOperationArgs args = e.Data.Translate<ContainerOperationArgs>(e.Network);
        //            GameObject source = args.SourceEntity.Object;
        //            GameObject sourceObj = args.Object.Object;
        //            GameObjectSlot targetSlot = this[args.TargetContainerID][args.TargetSlotID];
        //            GameObjectSlot sourceSlot = source.GetComponent<ContainerComponent>()[args.SourceContainerID][args.SourceSlotID];
        //            int amount = args.Amount;

        //            if (sourceObj.IsNull())
        //            {
        //                // object originating from a splitstack operation probably, instantiate it on network and resend message to parent
        //                GameObject newObj  =sourceSlot.Object.Clone();
        //                e.Network.InstantiateObject(newObj);
        //                args.Object = new TargetArgs(newObj);
        //                e.Network.SyncEvent(parent, Message.Types.ContainerOperation, args.Write);
        //                return true;
        //            }
        //            if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
        //            {
        //                targetSlot.Set(sourceObj, amount);
        //                sourceSlot.StackSize -= amount;
        //                return true;
        //            }
        //            if (sourceSlot.Object.ID == targetSlot.Object.ID)
        //            {
        //                if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
        //                {
        //                    targetSlot.StackSize += sourceSlot.StackSize;
        //                    e.Network.DisposeObject(sourceSlot.Object.NetworkID);
        //                    sourceSlot.Clear();
        //                    //merge slots
        //                    return true;
        //                }
        //            }
        //            else
        //                if (amount < sourceSlot.StackSize)
        //                    return true;

        //            targetSlot.Swap(sourceSlot);
        //            return true;

                   

               

        //        case Message.Types.AddItem:
        //            obj = e.Parameters[0] as GameObject;
        //            byte containerID = (byte)e.Parameters[1];
        //            byte slotID = (byte)e.Parameters[2];
        //            amount = (byte)e.Parameters[3];

        //            this[containerID][slotID].Set(obj, amount);

        //            return true;

        //        case Message.Types.ReceiveItem:
        //            e.Data.Translate(e.Network, r =>
        //            {
        //                obj = TargetArgs.Read(e.Network, r).Object;
        //                amount = r.ReadByte();
        //                GameObjectSlot available =
        //                    (from slot in parent.GetChildren()
        //                     where !slot.HasValue
        //                     select slot).FirstOrDefault();
        //                if (available.IsNull())
        //                {
        //                    // drop if no space available
        //                    e.Network.Spawn(obj, new Position(e.Network.Map, parent.Global + parent.GetComponent<PhysicsComponent>().Height * Vector3.UnitZ));
        //                    return;
        //                }
        //                available.Set(obj, amount);
        //            });
        //            return true;




               

        //        case Message.Types.Open:
        //            //e.Sender.PostMessage(Message.Types.Interface, parent);
        //            e.Network.PostLocalEvent(e.Sender, ObjectEventArgs.Create(Message.Types.Interface, new TargetArgs(parent)));
        //            return true;
        //        //case Message.Types.ContainerClose:
        //        //    Users.Remove(e.Sender);
        //        //    return true;
        //        case Message.Types.Give:
        //            objSlot = e.Parameters[1] as GameObjectSlot;
        //            Give(parent, e.Sender, objSlot);
        //            return true;
        //        case Message.Types.DropOn:
        //            InventoryComponent inv;
        //            if (!e.Sender.TryGetComponent<InventoryComponent>("Inventory", out inv))
        //                return false;
        //            if (inv.GetProperty<GameObjectSlot>("Holding").Object == null)
        //                return false;
        //            GameObject hauling = inv.GetProperty<GameObjectSlot>("Holding").Object;
        //           // if (
        //            Give(parent, e.Sender, inv.GetProperty<GameObjectSlot>("Holding"));
        //        //)//.Object))
        //            //{
        //                //  Map.RemoveObject(hauling);
        //      //          hauling.Remove();
        //                return true;
        //            //}
        //            //return false;
        //        case Message.Types.Insert:
        //            GameObjectSlot haulSlot = e.Sender["Inventory"]["Holding"] as GameObjectSlot;

        //            obj = haulSlot.Object;
        //            if (Give(parent, e.Sender, haulSlot))
        //            {
        //                e.Network.Despawn(obj);
        //                //obj.Remove();


        //                //e.Sender.PostMessage(Message.Types.ModifyNeed, parent, "Work", 20);
        //                //e.Sender.PostMessage(Message.Types.Dropped, parent);

        //                //e.Network.PostLocalEvent(e.Sender, LocalEventArgs.Create(Message.Types.ModifyNeed, new TargetArgs(parent), w =>
        //                //{
        //                //    w.Write(Encoding.ASCII.GetBytes("Work"));
        //                //    w.Write(20);
        //                //}));

        //                e.Network.PostLocalEvent(e.Sender, ObjectEventArgs.Create(Message.Types.ModifyNeed, new TargetArgs(parent), "Work", 20));
        //                e.Network.PostLocalEvent(e.Sender, ObjectEventArgs.Create(Message.Types.Dropped));
        //            }
        //            return true;
        //        case Message.Types.Extract:
        //            //foreach (GameObjectSlot sl in this.Slots)
        //            //    if (sl.HasValue)
        //            //        if (!FilterOld.Apply(sl.Object))
        //            //        {
        //            //          //  Loot.PopLoot(parent, sl.Object);
        //            //            e.Network.PopLoot(sl.Object, parent.Global, parent.Velocity);
        //            //            sl.Clear();
        //            //            break;
        //            //        }
        //            return true;

        //        case Message.Types.Work:
        //            //haulSlot = e.Sender["Inventory"]["Holding"] as GameObjectSlot;
        //            //if (!haulSlot.HasValue)
        //            //{
        //            //    Queue<GameObjectSlot> emptySlots;
        //            //    if (TryGetEmptySlots(out emptySlots))
        //            //        //e.Sender.PostMessage(Message.Types.Work, parent, new Predicate<GameObject>(foo => FilterOld.Apply(foo)), FilterOld, emptySlots.Count);
        //            //        e.Network.PostLocalEvent(e.Sender, ObjectEventArgs.Create(Message.Types.Work, new TargetArgs(parent), new Predicate<GameObject>(foo => FilterOld.Apply(foo)), FilterOld, emptySlots.Count));
        //            //    return true;
        //            //}
        //            return true;

        //        //case Message.Types.Query:
        //        //    Query(parent, e);
        //        //    return true;
        //        case Message.Types.ManageEquipment:
        //            //e.Sender.PostMessage(Message.Types.UIOwnership, parent, new Predicate<GameObject>(foo => foo.Components.ContainsKey("Container")));
        //            e.Network.PostLocalEvent(e.Sender, ObjectEventArgs.Create(Message.Types.UIOwnership, new TargetArgs(parent), new Predicate<GameObject>(foo => foo.Components.ContainsKey("Container"))));
        //            return true;
        //        default:
        //            return true;
        //    }
        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.ArrangeChildren:
                    //parent.Global.GetChunk(e.Network.Map).Invalidate();//.Saved = true;
                    e.Network.Map.GetChunk(parent.Global).Invalidate();//.Saved = true;

                    IObjectProvider net = e.Network;
                    ArrangeChildrenArgs args = e.Data.Translate<ArrangeChildrenArgs>(e.Network);
                    GameObject source = args.SourceEntity.Object;
                    GameObjectSlot targetSlot, sourceSlot;
                    if (!parent.TryGetChild(args.TargetSlotID, out targetSlot) ||
                        !source.TryGetChild(args.SourceSlotID, out sourceSlot))
                        return true;
                    int amount = args.Amount;
                    GameObject sourceObj = args.Object.Object;

                    //if (sourceObj.IsNull())
                    //{
                    //    // object originating from a splitstack operation probably, instantiate it on network and resend message to parent
                    //    GameObject newObj  =sourceSlot.Object.Clone();
                    //    e.Network.InstantiateObject(newObj);
                    //    args.Object = new TargetArgs(newObj);
                    //    e.Network.SyncEvent(parent, Message.Types.ContainerOperation, args.Write);
                    //    return true;
                    //}
                    //if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
                    //{
                    //    targetSlot.Set(sourceObj, amount);
                    //    sourceSlot.StackSize -= amount;
                    //    return true;
                    //}
                    //if (sourceSlot.Object.ID == targetSlot.Object.ID)
                    //{
                    //    if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
                    //    {
                    //        targetSlot.StackSize += sourceSlot.StackSize;
                    //        e.Network.DisposeObject(sourceSlot.Object.NetworkID);
                    //        sourceSlot.Clear();
                    //        //merge slots
                    //        return true;
                    //    }
                    //}
                    //else
                    //    if (amount < sourceSlot.StackSize)
                    //        return true;

                    targetSlot.Swap(sourceSlot);
                    break;

                default:
                    break;
            }
            return false;
        }

        //public override void Query(GameObject parent, List<InteractionOld> list)
        //{
        //    list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Open, parent, "Open"));

         
        //}

        public override object Clone()
        {
            return new ItemContainerComponent().Initialize(this.Capacity);
        }

        public override void GetClientActions(GameObject parent, List<ContextAction> actions)
        {
            actions.Add(new ContextAction(() => "Interface", () =>
            {
                parent.GetUi().Show();
                return;// true; true;
            }));
        }

        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<ObjectEventArgs>> handlers)
        {
            ui.Controls.Add(new SlotGrid(this.Children, parent, 4, s =>
            {
                s.DragDropAction = (args) =>
                {
                    var a = args as DragDropSlot;
                    Net.Client.PostPlayerInput(parent, Message.Types.ArrangeChildren, w =>
                    {
                        ArrangeChildrenArgs.Write(w, new TargetArgs(a.Parent), new TargetArgs(a.Source.Object), new TargetArgs(a.Slot.Object), a.Slot.ID, s.Tag.ID, (byte)a.Slot.StackSize);
                    });
                    return DragDropEffects.Move;
                };
            }));
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            this.Children.Write(writer);
            //writer.Write(this.Capacity);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Children = ItemContainer.Create(reader);
            //this.Initialize(reader.ReadByte());
        }
    }
}
