using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class InventorySlot : Slot
    {
        public InventorySlot()
        {

        }
        public ObjectFilter2 Filter = new ObjectFilter2();
        //public Func<GameObject, bool> DragDropCondition = o => true;
        //new GameObjectSlot Tag;
        public InventorySlot(GameObjectSlot invSlot, GameObject parent, int id = 0)
        {
            Tag = invSlot;
            ID = id;
            LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) && invSlot.StackSize > 1)
                {
                    //SplitStackWindow.Instance.Show(invSlot, parent);
                    SplitStackWindow.Instance.Refresh(new TargetArgs(invSlot)).Show();
                    return;
                }
                if (Tag.HasValue)
                {
                    //DragDropManager.Create(new DragDropSlot(parent, this.Tag, this.Tag, DragDropEffects.Move | DragDropEffects.Link));
                    DragDropManager.Create(new DragDropSlot(parent, new TargetArgs(this.Tag), new TargetArgs(this.Tag), DragDropEffects.Move | DragDropEffects.Link));
                    return;
                    //// MUST CLEAT SLOT ON SERVER TOO, or only on dropping the item somewhere else?
                    //GameObjectSlot clone = new GameObjectSlot(this.Tag.Object, this.Tag.StackSize);// Tag.Clone();

                    //Net.Client.PostPlayerInput(Message.Types.ContainerOperation, w =>
                    //{
                    //    ArrangeInventoryEventArgs.Write(w, new TargetArgs(parent), TargetArgs.Empty, TargetArgs.Empty, invSlot.Container.ID, invSlot.ID, (byte)0);
                    //});

                    //// wait for a dragdrop from the server or start it locally?
                    //DragDropManager.Create(new DragDropSlot(parent, this.Tag, clone, DragDropEffects.Move | DragDropEffects.Link));
                }
            };
            RightClickAction = () =>
            {
                //if (this.Tag.HasValue)
                    Net.Client.PlayerSlotInteraction(this.Tag);
                    //Net.Client.PostPlayerInput(Message.Types.UseInventoryItem, w => TargetArgs.Write(w, this.Tag.Object));
            };
            DragDropAction = (args) =>
            {
                var a = args as DragDropSlot;
                if (a.Effects == DragDropEffects.None)
                    return DragDropEffects.None;
                Net.Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(invSlot), a.DraggedTarget.Slot.StackSize);
                return DragDropEffects.Move;

                // OLD BELOW

                if (args.Effects == DragDropEffects.None)
                    return DragDropEffects.None;
                GameObjectSlot itemSlot = args.Item as GameObjectSlot;
                GameObjectSlot sourceSlot = args.Source as GameObjectSlot;
                if (itemSlot.IsNull())
                {
                    Log.Enqueue(Log.EntryTypes.System, "Null object!");
                    return DragDropEffects.None;
                }

                // TODO: maybe apply the filter in the object's message handling?
                if (!Filter.Apply(itemSlot.Object))
                    return DragDropEffects.None;
                if (!DragDropCondition(itemSlot.Object))
                    return DragDropEffects.None;

                if (args.Effects == DragDropEffects.Copy)
                {
                    // TODO: or maybe don't change the stacksize?
                    itemSlot = itemSlot.Clone();
                    sourceSlot = sourceSlot.Clone();
                }
                //Net.Client.PostPlayerInput(parent,  Message.Types.ArrangeInventory, w =>
                //{
                //    ArrangeInventoryEventArgs.Write(w, new TargetArgs(Player.Actor), Tag.ID, sourceSlot.ID, (byte)itemSlot.StackSize);
                //});

                //Net.Client.PostPlayerInput(parent, Message.Types.ArrangeInventory, w =>
                //{
                //    //w.Write(Tag.ID);
                //    //w.Write(sourceSlot.ID);
                //    //w.Write(itemSlot.StackSize);
                //    ArrangeInventoryEventArgs.Write(w, Tag.ID, sourceSlot.ID, (byte)itemSlot.StackSize);
                //});
                return DragDropEffects.Move;
            };


        }
        public override void GetTooltipInfo(Tooltip tooltip)
        {
            //base.GetTooltipInfo(tooltip);
            if (this.Tag.Object == null)
                return;
            this.Tag.Object.GetInventoryTooltip(tooltip);
        }
    }
}
