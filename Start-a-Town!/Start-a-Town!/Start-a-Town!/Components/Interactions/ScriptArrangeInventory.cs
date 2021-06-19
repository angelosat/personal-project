using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Scripts
{
    class ScriptArrangeInventory : Script
    {
        public override Script.Types ID
        {
            get
            {
                return Script.Types.ArrangeInventory;
            }
        }
        public override string Name
        {
            get
            {
                return "ScriptArrangeInventory";
            }
        }

        public override void Start(ScriptArgs e)
        {
            Finish(e);
            e.Net.PostLocalEvent(e.Target.Object, ObjectEventArgs.Create(Message.Types.InsertAt, e.Args));
            return;


            Net.IObjectProvider net = e.Net;
            ArrangeChildrenArgs args = e.Args.Translate<ArrangeChildrenArgs>(e.Net);
            args.SourceEntity.Object.Global.GetChunk(e.Net.Map).Changed = true;
            e.Target.Object.Global.GetChunk(e.Net.Map).Changed = true;
            GameObject source = args.SourceEntity.Object;
            GameObjectSlot targetSlot, sourceSlot;
            if (!args.SourceEntity.Object.TryGetChild(args.TargetSlotID, out targetSlot) ||
                !e.Target.Object.TryGetChild(args.SourceSlotID, out sourceSlot))
                return;
            int amount = args.Amount;
            GameObject sourceObj = args.Object.Object;

            //if (sourceObj.IsNull())
            //{
            //    // object originating from a splitstack operation probably, instantiate it on network and resend message to parent
            //    GameObject newObj = sourceSlot.Object.Clone();
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

            /*
             * if split
             *  instanstiate and sync
             * else
             *  
             */

            if (amount < sourceSlot.StackSize)
            {
                GameObject newObj = sourceSlot.Object.Clone();
                e.Net.InstantiateObject(newObj); // instantiate a the new clone of the object
                args.Object = new TargetArgs(newObj); // replace the object in the args object to be sent over again
                e.Net.SyncEvent(e.Target.Object, Message.Types.InsertAt, args.Write); // sync call for the recepient to recieve the new object at target slot id
            }

            if (sourceSlot.Object.ID == targetSlot.Object.ID)
            {
                if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
                {
                    targetSlot.StackSize += sourceSlot.StackSize;
                    e.Net.DisposeObject(sourceSlot.Object.NetworkID);
                    sourceSlot.Clear();
                    //merge slots
                    return;
                }
            }
            else
            {
                if (amount < sourceSlot.StackSize) // if trying to to split a stack on a slot containing a different item
                    return;
                GameObject newObj = sourceSlot.Object.Clone();
                e.Net.InstantiateObject(newObj);
                //args.Object = new TargetArgs(newObj);
                e.Net.SyncEvent(e.Target.Object, Message.Types.InsertAt, w =>
                {
                    w.Write(targetSlot.ID);
                    new TargetArgs(newObj).Write(w);
                    w.Write(amount);
                });
                return;
            }
            targetSlot.Swap(sourceSlot);
        }

        public override object Clone()
        {
            return new ScriptArrangeInventory();
        }
    }
}
