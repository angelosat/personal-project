using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    class SlotDefault : InventorySlot
    {
        public override DragDropEffects Drop(DragEventArgs args)
        {
            var a = args as DragDropSlot;
            if (a.Effects == DragDropEffects.None)
                return DragDropEffects.None;
            Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(this.Tag), a.DraggedTarget.Slot.StackSize);

            return DragDropEffects.Move;
        }

        protected override void OnLeftClick()
        {
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) && this.Tag.StackSize > 1)
            {
                SplitStackWindow.Instance.Show(this.Tag, this.Tag.Parent);
                return;
            }
            if (Tag.HasValue)
            {
                DragDropManager.Create(new DragDropSlot(null, new TargetArgs(this.Tag), new TargetArgs(this.Tag), DragDropEffects.Move | DragDropEffects.Link));
                return;
            }
        }

        protected override void OnRightClick()
        {
            if (!Tag.HasValue)
                return;
            Client.PlayerSlotInteraction(this.Tag);
        }
    }
}
