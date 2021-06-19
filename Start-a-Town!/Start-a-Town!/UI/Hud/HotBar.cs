using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class HotBar : GroupBox
    {
        //SlotGrid Slots { get; set; }
        //Dictionary<Keys, Slot> Dic { get; set; }
        SlotGrid<HotbarSlot> Slots { get; set; }
        Dictionary<Keys, HotbarSlot> Dic { get; set; }
        public HotBar()
        {
            this.Mapping = new Dictionary<byte, byte>();
            Queue<Keys> keys = new Queue<Keys>(new List<Keys>()
            {
                Keys.D1,
                Keys.D2,
                Keys.D3,
                Keys.D4,
                Keys.D5,
                Keys.D6,
                Keys.D7,
                Keys.D8,
                Keys.D9,
                Keys.D0,
                Keys.OemMinus,
                Keys.Oemplus,
            });
            //this.Dic = new Dictionary<Keys, Slot>();
            this.Dic = new Dictionary<Keys, HotbarSlot>();
            this.Slots = new SlotGrid<HotbarSlot>(12, 12, c =>
            {
                //c.Tag = GameObjectSlot.Empty;
                //c.Tag.Parent = Player.Actor;
                //c.Tag = new GameObjectSlot(container);
                this.Dic[keys.Dequeue()] = c;
            });
            //this.Slots = new SlotGrid(12, 12, c =>
            //{
            //    this.Dic[keys.Dequeue()] = c;
            //    c.DragDropAction = (a) =>
            //    {
            //        DragDropSlot drag = a as DragDropSlot;
            //        if (drag.IsNull())
            //            return DragDropEffects.None;
            //        GameObjectSlot.Copy(drag.Slot, c.Tag);
            //        return DragDropEffects.Link;
            //    };
            //    // TODO: add dragdrop functionality for hotbar slots to move them around
            //    c.LeftClickAction = () =>
            //    {
            //        if (c.Tag.HasValue)
            //        {
            //            //var linkedSlot = Player.Actor.GetChildren().FirstOrDefault(s => s.Object == c.Tag.Object);
            //            //if (linkedSlot == null)
            //            //    throw new Exception();
            //            //Client.PlayerSlotInteraction(linkedSlot);
            //            // just use the code as if the player is rightclicking the corresponding slot in the inventory interface
            //            Client.PostPlayerInput(Components.Message.Types.UseInventoryItem, w => TargetArgs.Write(w, c.Tag.Object));
            //        }
            //    };
            //    c.RightClickAction = () =>
            //    {
            //        c.Tag.Clear();
            //    };
            //});
            this.Controls.Add(this.Slots);
        }

        public override void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Handled)
                return;
            HotbarSlot slot;
            if (!this.Dic.TryGetValue(e.KeyCode, out slot))
                return;
            e.Handled = true;
            slot.Pressed = true;
        }

        public override void HandleKeyUp(KeyEventArgs e)
        {
            if (e.Handled)
                return;
            HotbarSlot slot;
            if (!this.Dic.TryGetValue(e.KeyCode, out slot))
                return;
            if (!slot.Pressed)
                return;
            e.Handled = true;
            slot.PerformLeftClick();
        }

        Dictionary<byte, byte> Mapping;
        public void Initialize(GameObject character)
        {
            foreach (var map in this.Mapping)
            {
                //var objSlot = character.GetComponent<InventoryComponent>().Containers.First()[map.Value];
                //GameObjectSlot.Copy(objSlot, (this.Slots.Controls[map.Key] as Slot).Tag);

                var slot = this.Slots.Controls[map.Key] as Slot;
                var found = PlayerOld.Actor.GetChildren()[map.Value].Object;
                slot.Tag.Link = found;
            }
        }

        //public List<SaveTag> Save()
        public SaveTag Save()
        {
           // List<SaveTag> data = new List<SaveTag>();
            SaveTag tag = new SaveTag(SaveTag.Types.List, "HotBar", SaveTag.Types.Compound);
            foreach (var slot in from slot in this.Slots.Controls where slot is Slot select slot as Slot)
            {
                if (!slot.Tag.HasValue)
                    continue;
                //var found = Player.Actor.GetComponent<InventoryComponent>().Containers.First().FirstOrDefault(sl => sl.Object == slot.Tag.Object);
                var found = PlayerOld.Actor.GetChildren().FirstOrDefault(s => s.Object == slot.Tag.Object);
                if (found == null)
                    continue;
                SaveTag foundTag = new SaveTag(SaveTag.Types.Compound, "Slot");
                foundTag.Add(new SaveTag(SaveTag.Types.Byte, "HotBar_Index", (byte)slot.ID));
                foundTag.Add(new SaveTag(SaveTag.Types.Byte, "Inventory_Index", (byte)found.ID));
                tag.Add(foundTag);
            }
            return tag;
        }
        public void Load(SaveTag tag, GameObject character)
        {
            this.Mapping.Clear();
            foreach(var slot in (List<SaveTag>)tag.Value)
            {
                var hbindex = slot.GetValue<byte>("HotBar_Index");
                var invindex = slot.GetValue<byte>("Inventory_Index");
                this.Mapping.Add(hbindex, invindex);
                //var objSlot = character.GetComponent<InventoryComponent>().Containers.First()[invindex];
                //GameObjectSlot.Copy(objSlot, (this.Slots.Controls[hbindex] as Slot).Tag);
            }
        }
    }
}
