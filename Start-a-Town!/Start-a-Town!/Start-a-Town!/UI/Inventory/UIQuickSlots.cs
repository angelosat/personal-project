using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class UIQuickSlots : GroupBox
    {
        SlotGrid<InventorySlot> Slots { get; set; }
        //List<InventorySlot> Slots = new List<InventorySlot>();
        Dictionary<Keys, InventorySlot> Dic { get; set; }
        public UIQuickSlots(GameObject parent)
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
                Keys.D8
                //,
                //Keys.D9,
                //Keys.D0,
                //Keys.OemMinus,
                //Keys.Oemplus,
            });
            this.Dic = new Dictionary<Keys, InventorySlot>();
            var inventory = parent.GetComponent<PersonalInventoryComponent>();
            var slots = inventory.Slots.Slots.Take(8).ToList();

            //foreach(var s in slots)
            //{
            //    var slot = new InventorySlot(s, parent, s.ID);
            //    this.Dic[keys.Dequeue()] = slot;
            //    this.Slots.Add(slot);
            //    this.Controls.Add(slot);
            //}

            //this.Slots = new SlotGrid<InventorySlot>(slots, 12, c =>
            //{
            //    this.Dic[keys.Dequeue()] = c;
            //});
            var i = 1;
            this.Slots = new SlotGrid<InventorySlot>(slots, 12, c =>
            {
                var slot = new InventorySlot(c, parent, c.ID);
                var key = keys.Dequeue();
                var keyLbl = new Label(i++.ToString());//key.ToString());
                slot.Controls.Add(keyLbl);
                this.Dic[key] = slot;
                return slot;
            });
            this.Controls.Add(this.Slots);
        }

        public override void HandleKeyDown(KeyEventArgs e)
        {
            if (e.Handled)
                return;
            InventorySlot slot;
            if (!this.Dic.TryGetValue(e.KeyCode, out slot))
                return;
            e.Handled = true;
            slot.Pressed = true;
        }

        public override void HandleKeyUp(KeyEventArgs e)
        {
            
            if (e.Handled)
                return;
            InventorySlot slot;
            if (!this.Dic.TryGetValue(e.KeyCode, out slot))
                return;
            if (!slot.Pressed)
                return;
            e.Handled = true;
            //slot.PerformLeftClick();
            slot.PerformRightClick();
        }

        Dictionary<byte, byte> Mapping;
        //public void Initialize(GameObject character)
        //{
        //    foreach (var map in this.Mapping)
        //    {
        //        //var objSlot = character.GetComponent<InventoryComponent>().Containers.First()[map.Value];
        //        //GameObjectSlot.Copy(objSlot, (this.Slots.Controls[map.Key] as Slot).Tag);

        //        var slot = this.Slots.Controls[map.Key] as Slot;
        //        var found = Player.Actor.GetChildren()[map.Value].Object;
        //        slot.Tag.Link = found;
        //    }
        //}

        //public SaveTag Save()
        //{
        //    // List<SaveTag> data = new List<SaveTag>();
        //    SaveTag tag = new SaveTag(SaveTag.Types.List, "HotBar", SaveTag.Types.Compound);
        //    foreach (var slot in from slot in this.Slots.Controls where slot is Slot select slot as Slot)
        //    {
        //        if (!slot.Tag.HasValue)
        //            continue;
        //        //var found = Player.Actor.GetComponent<InventoryComponent>().Containers.First().FirstOrDefault(sl => sl.Object == slot.Tag.Object);
        //        var found = Player.Actor.GetChildren().FirstOrDefault(s => s.Object == slot.Tag.Object);
        //        if (found == null)
        //            continue;
        //        SaveTag foundTag = new SaveTag(SaveTag.Types.Compound, "Slot");
        //        foundTag.Add(new SaveTag(SaveTag.Types.Byte, "HotBar_Index", (byte)slot.ID));
        //        foundTag.Add(new SaveTag(SaveTag.Types.Byte, "Inventory_Index", (byte)found.ID));
        //        tag.Add(foundTag);
        //    }
        //    return tag;
        //}
        //public void Load(SaveTag tag, GameObject character)
        //{
        //    this.Mapping.Clear();
        //    foreach (var slot in (List<SaveTag>)tag.Value)
        //    {
        //        var hbindex = slot.GetValue<byte>("HotBar_Index");
        //        var invindex = slot.GetValue<byte>("Inventory_Index");
        //        this.Mapping.Add(hbindex, invindex);
        //        //var objSlot = character.GetComponent<InventoryComponent>().Containers.First()[invindex];
        //        //GameObjectSlot.Copy(objSlot, (this.Slots.Controls[hbindex] as Slot).Tag);
        //    }
        //}
    }
}
