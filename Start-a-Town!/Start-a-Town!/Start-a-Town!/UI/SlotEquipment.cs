using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;
namespace Start_a_Town_.UI
{
    class SlotEquipment : Slot
    {
        public SlotEquipment()
        {

        }
        public SlotEquipment(Vector2 location)
            : base(location)
        {

        }

        public SlotEquipment(GameObjectSlot invSlot, GameObject parent, int id = 0)
        {
            Tag = invSlot;
            ID = id;
            LeftClickAction = () =>
            {
                // no need to split stack cause generally equipment isn't stackable
                //if (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) && invSlot.StackSize > 1)
                //{
                //    //SplitStackWindow.Instance.Show(invSlot, parent);
                //    SplitStackWindow.Instance.Refresh(new TargetArgs(invSlot)).Show();
                //    return;
                //}
                if (Tag.HasValue)
                {
                    //DragDropManager.Create(new DragDropSlot(parent, this.Tag, this.Tag, DragDropEffects.Move | DragDropEffects.Link));
                    DragDropManager.Create(new DragDropSlot(parent, new TargetArgs(this.Tag), new TargetArgs(this.Tag), DragDropEffects.Move | DragDropEffects.Link));
                    return;

                }
            };
            RightClickAction = () =>
            {
                if (this.Tag.HasValue)
                    Net.Client.PlayerUnequip(new TargetArgs(invSlot));
                    //Net.Client.PlayerSlotInteraction(this.Tag);
            };
            DragDropAction = (args) =>
            {
                var a = args as DragDropSlot;
                if (a.Effects == DragDropEffects.None)
                    return DragDropEffects.None;

                var equipComponent = a.SourceTarget.Object.GetComponent<EquipComponent>();
                if (equipComponent == null)
                    return DragDropEffects.None;
                if ((byte)equipComponent.Type.ID != this.Tag.ID)
                    return DragDropEffects.None;

                Net.Client.PlayerInventoryOperationNew(a.SourceTarget, new TargetArgs(invSlot), a.DraggedTarget.Slot.StackSize);
                return DragDropEffects.Move;

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

                if (!DragDropCondition(itemSlot.Object))
                    return DragDropEffects.None;

                if (args.Effects == DragDropEffects.Copy)
                {
                    // TODO: or maybe don't change the stacksize?
                    itemSlot = itemSlot.Clone();
                    sourceSlot = sourceSlot.Clone();
                }

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
