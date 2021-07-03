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
