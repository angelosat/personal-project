using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class BlockEntityInventorySlot : Slot
    {
        public Vector3 Global;
        public BlockEntityInventorySlot()
        {

        }
        public ObjectFilter2 Filter = new ObjectFilter2();

        public BlockEntityInventorySlot(GameObjectSlot invSlot, Vector3 blockEntityGlobal, int id = 0)
        {
            Tag = invSlot;
            ID = id;
            //LeftClickAction = () =>
            //{
            //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) && invSlot.StackSize > 1)
            //    {
            //        SplitStackWindow.Instance.Refresh(new TargetArgs(this.Global, this.Tag)).Show();
            //        return;
            //    }
            //    if (Tag.HasValue)
            //    {
            //        DragDropManager.Create(new DragDropSlot(null, new TargetArgs(this.Global, this.Tag), new TargetArgs(this.Global, this.Tag), DragDropEffects.Move | DragDropEffects.Link));
            //        return;
            //    }
            //};
            //RightClickAction = () =>
            //{
            //    if (this.Tag.HasValue)
            //        Client.PlayerSlotRightClick(new TargetArgs(this.Global), this.Tag.Object);
            //};
            //DragDropAction = (args) =>
            //{
            //    var a = args as DragDropSlot;
            //    Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(this.Global, this.Tag), a.DraggedTarget.Slot.Object.StackSize);
            //    return DragDropEffects.Move;
            //};
        }
        public override void GetTooltipInfo(Tooltip tooltip)
        {
            //base.GetTooltipInfo(tooltip);
            if (this.Tag.Object == null)
                return;
            this.Tag.Object.GetInventoryTooltip(tooltip);
        }
        protected override void OnLeftClick()
        {
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) && this.Tag.StackSize > 1)
            {
                SplitStackWindow.Instance.Refresh(new TargetArgs(this.Global, this.Tag)).Show();
                return;
            }
            if (Tag.HasValue)
            {
                DragDropManager.Create(new DragDropSlot(null, new TargetArgs(this.Global, this.Tag), new TargetArgs(this.Global, this.Tag), DragDropEffects.Move | DragDropEffects.Link));
                return;
            }
        }
        protected override void OnRightClick()
        {
            if (this.Tag.HasValue)
                Net.Client.PlayerSlotRightClick(new TargetArgs(Net.Client.Instance.Map, this.Global), this.Tag.Object);
        }
        public override DragDropEffects Drop(DragEventArgs args)
        {
            var a = args as DragDropSlot;
            Net.Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(this.Global, this.Tag), a.DraggedTarget.Slot.Object.StackSize);
            return DragDropEffects.Move;
        }
    }
}
