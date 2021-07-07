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
        public InventorySlot(GameObjectSlot invSlot, GameObject parent, int id = 0)
        {
            Tag = invSlot;
            ID = id;
            LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) && invSlot.StackSize > 1)
                {
                    SplitStackWindow.Instance.Refresh(new TargetArgs(invSlot)).Show();
                    return;
                }
                if (Tag.HasValue)
                {
                    ToolManager.Instance.ActiveTool.SlotLeftClick(this.Tag);
                    return;
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
            if (this.Tag.Object == null)
                return;
            this.Tag.Object.GetInventoryTooltip(tooltip);
        }
    }
}
