using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class ParentComponent : Component
    {
        public override string ComponentName
        {
            get { return "ParentComponent"; }
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.ArrangeChildren:
                    parent.Global.GetChunk(e.Network.Map).Changed = true;
                    Net.IObjectProvider net = e.Network;
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


        public override object Clone()
        {
            return new ParentComponent();
        }

        //public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<ObjectEventArgs>> handlers)
        //{
        //    //ui.Controls.Add(new SlotGrid(this.Children, parent, 4, s =>
        //    //{
        //    //    s.DragDropAction = (args) =>
        //    //    {
        //    //        var a = args as DragDropSlot;
        //    //        Net.Client.PostPlayerInput(parent, Message.Types.ArrangeChildren, w =>
        //    //        {
        //    //            ArrangeChildrenArgs.Write(w, new TargetArgs(a.Parent), new TargetArgs(a.Source.Object), new TargetArgs(a.Slot.Object), a.Slot.ID, s.Tag.ID, (byte)a.Slot.StackSize);
        //    //        });
        //    //        return DragDropEffects.Move;
        //    //    };
        //    //}));
        //}
    }
}
