﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class SlotDefault : InventorySlot
    {
        public SlotDefault()
        {

        }
        public SlotDefault(GameObjectSlot slot)
            : base(slot, slot.Parent)
        {
            this.Tag = slot;
        }

        public override DragDropEffects Drop(DragEventArgs args)
        {
            var a = args as DragDropSlot;
            if (!this.Tag.Filter(a.Source.Object))
                return DragDropEffects.None;
            Net.Client.PlayerInventoryOperation(Player.Actor, a.Source, this.Tag, a.Slot.StackSize);

            //Net.Client.PlayerInventoryOperationOld(this.Tag.Parent, w =>
            //{
            //    ArrangeChildrenArgs.Write(w, new TargetArgs(a.Parent), new TargetArgs(a.Source.Object), new TargetArgs(a.Slot.Object), a.Source.ID, this.Tag.ID, (byte)a.Slot.StackSize);
            //});
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
                DragDropManager.Create(new DragDropSlot(this.Tag.Parent, this.Tag, this.Tag, DragDropEffects.Move | DragDropEffects.Link));
                return;
            }
        }

        protected override void OnRightClick()
        {
            if (!Tag.HasValue)
                return;
            Net.Client.PlayerSlotInteraction(this.Tag);
            //Net.Client.PostPlayerInput(Message.Types.StartScript, w =>
            //{
            //    Ability.Write(w, Script.Types.PickUpSlot, new TargetArgs(this.Tag));//, ww => TargetArgs.Write(ww, Player.Actor));
            //});
        }
    }
}
