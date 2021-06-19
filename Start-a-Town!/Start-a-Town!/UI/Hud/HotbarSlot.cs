using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
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
            if (drag.IsNull())
                return DragDropEffects.None;

            //this.Tag.Link = drag.Slot.Object;
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
                //Client.PostPlayerInput(Components.Message.Types.UseInventoryItem, w => TargetArgs.Write(w, c.Tag.Object));
            }
        }
        public override void Clear()
        {
            //this.Tag = GameObjectSlot.Empty;
            //this.Tag.Clear();
            this.Tag.Link = null;
        }
        //c.DragDropAction = (a) =>
        //        {
        //            DragDropSlot drag = a as DragDropSlot;
        //            if (drag.IsNull())
        //                return DragDropEffects.None;
        //            GameObjectSlot.Copy(drag.Slot, c.Tag);
        //            return DragDropEffects.Link;
        //        };
        //        // TODO: add dragdrop functionality for hotbar slots to move them around
        //        c.LeftClickAction = () =>
        //        {
        //            if (c.Tag.HasValue)
        //            {
        //                //var linkedSlot = Player.Actor.GetChildren().FirstOrDefault(s => s.Object == c.Tag.Object);
        //                //if (linkedSlot == null)
        //                //    throw new Exception();
        //                //Client.PlayerSlotInteraction(linkedSlot);
        //                // just use the code as if the player is rightclicking the corresponding slot in the inventory interface
        //                Client.PostPlayerInput(Components.Message.Types.UseInventoryItem, w => TargetArgs.Write(w, c.Tag.Object));
        //            }
        //        };
        //        c.RightClickAction = () =>
        //        {
        //            c.Tag.Clear();
        //        };
    }
}
