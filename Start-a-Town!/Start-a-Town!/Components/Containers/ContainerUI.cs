using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components.Containers
{
    class ContainerUI : GroupBox
    {
        //GameObject ContainerEntity;

        public ContainerUI()
        {

        }

        //internal override void OnGameEvent(GameEvent e)
        //{
        //    switch(e.Type)
        //    {
        //        case Message.Types.EntityMovedCell:
        //            if(this.ContainerEntity == null)
        //                break;
        //            var entity = e.Parameters[0] as GameObject;
        //            if(entity != Player.Actor)
        //                break;
        //            var distance = Vector3.DistanceSquared(Player.Actor.Global, this.ContainerEntity.Global);
        //            if (distance > 2)
        //                this.GetWindow().Hide();
        //            break;

        //        default:
        //            break;
        //    }
        //}

        public ContainerUI Refresh(GameObject parent)
        {
            //this.ContainerEntity = parent;
            StorageComponent comp = parent.GetComponent<StorageComponent>();
            var grid = new SlotGrid(comp.Container.Slots, parent, 4, s =>
            {
                s.DragDropAction = (args) =>
                {
                    var a = args as DragDropSlot;
                    Client.PlayerInventoryOperationNew(a.Source, s.Tag, a.Slot.Object.StackSize);
                    return DragDropEffects.Move;
                };
                s.RightClickAction = () =>
                {
                    if (s.Tag.HasValue)
                        Client.PlayerSlotInteraction(s.Tag);
                };
            });
            this.Controls.Clear();
            this.Controls.Add(grid);
            return this;
        }

        
    }
}
