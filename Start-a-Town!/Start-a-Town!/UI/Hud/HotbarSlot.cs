using System;
using System.Linq;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    [Obsolete]
    class HotbarSlot : Slot
    {
        public HotbarSlot()
        {
            this.DragDropAction = this._OnDragDrop;
            this.LeftClickAction = this.Interact;
            this.RightClickAction = this.Clear;
        }
        DragDropEffects _OnDragDrop(DragEventArgs a)
        {
            DragDropSlot drag = a as DragDropSlot;
            if (drag is null)
                return DragDropEffects.None;

            this.Tag.Link = drag.SourceTarget.Object;

            return DragDropEffects.Link;
        }
        void Interact()
        {
            if (this.Tag.HasValue)
            {
                var linkedSlot = PlayerOld.Actor.GetChildren().FirstOrDefault(s => s.Object == this.Tag.Object);
                if (linkedSlot == null)
                    throw new Exception();
                // change it to equip item instead of slot interact
                Client.PlayerSlotInteraction(linkedSlot);
                // just use the code as if the player is rightclicking the corresponding slot in the inventory interface
            }
        }
        public override void Clear()
        {
            this.Tag.Link = null;
        }
    }
}
