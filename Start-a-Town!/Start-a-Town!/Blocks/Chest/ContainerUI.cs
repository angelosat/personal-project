using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Blocks.Chest
{
    class ContainerUI : GroupBox
    {
        Vector3 Global;

        public ContainerUI()
        {

        }

        public ContainerUI Refresh(Vector3 global, BlockChest.BlockChestEntity entity)
        {
            this.Global = global;
            var grid = new SlotGrid(entity.Container.Slots, 4, this.SlotInitializer);
            
            this.Controls.Clear();
            this.Controls.Add(grid);
            return this;
        }

        void SlotInitializer(InventorySlot s)
        {
            s.LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) && s.Tag.StackSize > 1)
                {
                    SplitStackWindow.Instance.Refresh(new TargetArgs(this.Global, s.Tag)).Show();
                    return;
                }
                if (s.Tag.HasValue)
                {
                    DragDropManager.Create(new DragDropSlot(null, new TargetArgs(this.Global, s.Tag), new TargetArgs(this.Global, s.Tag), DragDropEffects.Move | DragDropEffects.Link));
                }
            };
            s.DragDropAction = (args) =>
            {
                var a = args as DragDropSlot;
                Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(this.Global, s.Tag), a.DraggedTarget.Slot.Object.StackSize);
                return DragDropEffects.Move;
            };
            s.RightClickAction = () =>
            {
                if (s.Tag.HasValue)
                    Client.PlayerSlotRightClick(new TargetArgs(this.Global), s.Tag.Object);
            };
        }
    }
}
