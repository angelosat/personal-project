using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    public class QuickBar : Panel
    {
        public int Count = 0;

        public QuickBar()
        {
            Color = Color.Black;
            this.Name = "QuickBar";
            this.AutoSize = true;
            Game1.TextInput.KeyUp += new System.Windows.Forms.KeyEventHandler(TextInput_KeyUp);
            Game1.TextInput.KeyDown += new System.Windows.Forms.KeyEventHandler(TextInput_KeyDown);
        }

        public QuickBar Initialize()
        {
            //foreach (var ctrl in Client.Controls)
            foreach (var ctrl in Controls)
            {
                ctrl.LeftClick -= slot_Click;
            }
            //Client.Controls.Clear();
            Controls.Clear();
            if (Count == 0)
                return this;
            //  Slot slot;
            for (int i = 0; i < Count; i++)
            {
                Slot slot = new Slot(new Vector2(i * Slot.DefaultHeight, 0));
                slot.Controls.Add(new Label(((i + 1) % 10).ToString()).SetMousethrough(true));
                slot.Tag = GameObjectSlot.Empty;
                slot.ID = i;
                //slot.LeftClick += new UIEvent(slot_Click);
                //slot.RightClick += new UIEvent(slot_RightClick);
                slot.DragDropAction = (a) =>
                {
                    GameObjectSlot objSlot = a.Source as GameObjectSlot;
                    if (objSlot is null)
                        return DragDropEffects.None;
                    if (!InventoryComponent.HasObject(PlayerOld.Actor, obj => obj == objSlot.Object))
                        return DragDropEffects.None;
                    GameObjectSlot.Copy(objSlot, slot.Tag);
                    return DragDropEffects.Link;
                };
                slot.RightClickAction = () =>
                {
                    slot.Tag = GameObjectSlot.Empty;
                };
                slot.LeftClickAction = () =>
                {
                    if (!slot.Tag.HasValue)
                        return;
                    throw new NotImplementedException();
                    //GameObjectSlot found;
                    //if (InventoryComponent.HasObject(Player.Actor, foo => foo == slot.Tag.Object, out found))
                    //    GameObject.PostMessage(Player.Actor, Message.Types.Hold, null, found, found.Object);
                };
                Controls.Add(slot);
            }
            return this;
        }

        void slot_RightClick(object sender, EventArgs e)
        {
            //GameObjectSlot objSlot = (sender as Slot).Tag;
            (sender as Slot).Tag = GameObjectSlot.Empty;
        }


        void TextInput_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            e.Handled = true;
            if (e.KeyValue < 49 || e.KeyValue > 58)
                return;
            int slotIndex = e.KeyValue - 49;
            Slot slot = Controls[slotIndex] as Slot;
            slot.Pressed = true;
        }


        void TextInput_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.Handled)
                return;
            e.Handled = true;
            if (e.KeyValue < 49 || e.KeyValue > 58)
                return;
            //Controls[e.KeyChar].PerformClick();
            int slotIndex = e.KeyValue - 49;// (int)char.GetNumericValue(e.KeyChar);
            Slot slot = Controls[slotIndex] as Slot;
            slot.PerformLeftClick();
            slot.Pressed = false;
        }

        void slot_Click(object sender, EventArgs e)
        {
            Slot quickslot = sender as Slot;
            if (DragDropManager.Instance.Item != null)
            {
                GameObjectSlot slot = DragDropManager.Instance.Item as GameObjectSlot;
                if (slot == null)
                    return;

                InventoryComponent inv = PlayerOld.Actor.GetComponent<InventoryComponent>("Inventory");

                ItemContainer container;
                if (!inv.TryGetContainer("Bag1", out container))
                    return;

                if (!container.Contains(slot))
                    return;

                //quickslot.Tag = slot;
                var barSlot = quickslot.Tag as GameObjectSlot;
                GameObjectSlot.Copy(slot, barSlot);
                DragDropManager.Instance.Clear();
                return;
            }

            GameObjectSlot objSlot = quickslot.Tag;
            if (objSlot == null)
                return;
            if (objSlot.HasValue)
            {
                // search inventory for object and equip it
               // if(InventoryComponent.HasObject(Player.Actor, foo=>foo == objSlot.Object))
                GameObjectSlot found;
                if(InventoryComponent.HasObject(PlayerOld.Actor, foo=>foo == objSlot.Object, out found))
                    throw new NotImplementedException();
                    //GameObject.PostMessage(Player.Actor, Message.Types.Hold, null, found, found.Object);
            }

            //GameObject.PostMessage(Player.Actor, Message.Types.Hold, null, objSlot, objSlot.Object);
        }
    }
}
