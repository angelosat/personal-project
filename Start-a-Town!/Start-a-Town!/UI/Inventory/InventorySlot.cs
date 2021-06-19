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
        readonly bool DragDropEnabled = false;
        public ObjectFilter2 Filter = new();
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
                    //DragDropManager.Create(new DragDropSlot(parent, new TargetArgs(this.Tag), new TargetArgs(this.Tag), DragDropEffects.Move | DragDropEffects.Link));
                    ToolManager.Instance.ActiveTool.SlotLeftClick(this.Tag);

                    return;



                    //DragDropManager.Create(new DragDropSlot(parent, this.Tag, this.Tag, DragDropEffects.Move | DragDropEffects.Link));
                    //return;
                    //// MUST CLEAT SLOT ON SERVER TOO, or only on dropping the item somewhere else?
                    //GameObjectSlot clone = new GameObjectSlot(this.Tag.Object, this.Tag.StackSize);// Tag.Clone();

                    //Client.PostPlayerInput(Message.Types.ContainerOperation, w =>
                    //{
                    //    ArrangeInventoryEventArgs.Write(w, new TargetArgs(parent), TargetArgs.Empty, TargetArgs.Empty, invSlot.Container.ID, invSlot.ID, (byte)0);
                    //});

                    //// wait for a dragdrop from the server or start it locally?
                    //DragDropManager.Create(new DragDropSlot(parent, this.Tag, clone, DragDropEffects.Move | DragDropEffects.Link));
                }
            };
            DragDropCreateAction = () =>
            {
                if (DragDropEnabled)
                    DragDropManager.Create(new DragDropSlot(parent, new TargetArgs(this.Tag), new TargetArgs(this.Tag), DragDropEffects.Move | DragDropEffects.Link));
            };
            RightClickAction = () =>
            {
                ToolManager.Instance.ActiveTool.SlotRightClick(this.Tag);
                    //Client.PlayerSlotInteraction(this.Tag);
            };
            DragDropAction = (args) =>
            {
                var a = args as DragDropSlot;
                if (a.Effects == DragDropEffects.None)
                    return DragDropEffects.None;
                Net.Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(invSlot), a.DraggedTarget.Slot.StackSize);
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
