using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Start_a_Town_.UI
{
    [Obsolete]
    public class HotBar : GroupBox
    {
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
            this.Dic = new Dictionary<Keys, HotbarSlot>();
            this.Slots = new SlotGrid<HotbarSlot>(12, 12, c =>
            {
                this.Dic[keys.Dequeue()] = c;
            });
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
                var slot = this.Slots.Controls[map.Key] as Slot;
                var found = PlayerOld.Actor.GetChildren()[map.Value].Object;
                slot.Tag.Link = found;
            }
        }

        public SaveTag Save()
        {
            SaveTag tag = new SaveTag(SaveTag.Types.List, "HotBar", SaveTag.Types.Compound);
            foreach (var slot in from slot in this.Slots.Controls where slot is Slot select slot as Slot)
            {
                if (!slot.Tag.HasValue)
                    continue;
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
            }
        }
    }
}
