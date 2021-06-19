using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Start_a_Town_.Items;

namespace Start_a_Town_.UI
{
    class LootWindow : Window
    {
        Rectangle DragBoxFromMouseDown = Rectangle.Empty;

        Panel panel;
        Dictionary<Slot, Label> SlotLabelMap;

        LootCollection _Loot;
        public LootCollection Loot
        {
            get { return _Loot; }
            set
            {
                if (value != null) //_Loot)
                {
                    _Loot = value;
                    _Loot.FullyLooted += new EventHandler<EventArgs>(Loot_FullyLooted);
                    //Console.WriteLine("tes");
                    SlotLabelMap = new Dictionary<Slot, UI.Label>();

                    panel = new Panel();
                    panel.ClientSize = new Rectangle(0, 0, 150, Loot.LootItems.Count * UIManager.SlotSprite.Height);
                    Controls.Add(panel);
                    SizeToControl(panel);

                    for (int i = 0; i < Loot.LootItems.Count; i++)
                    {
                        Slot slot = new Slot(new Vector2(0, i * UIManager.SlotSprite.Height));
                        slot.Tag = Loot.LootItems[i];
                        slot.Click += new UIEvent(slot_Click);
                        panel.Controls.Add(slot);
                        Label slotLabel = new Label(new Vector2(slot.Right, slot.Center.Y), Loot.LootItems[i].Data.Name, TextAlignment.Left, TextAlignment.Center);
                        panel.Controls.Add(slotLabel);
                        SlotLabelMap.Add(slot, slotLabel);
                    }
                }
            }
        }

        void Loot_FullyLooted(object sender, EventArgs e)
        {
            Loot.FullyLooted -= Loot_FullyLooted;
            Close();
        }

        public LootWindow()
        {
            Title = "Loot";
            ClientSize = new Rectangle(0, 0, 100, 100);
            Movable = true;
            Location = Controller.Instance.MouseLocation;
            Update();
        }

        void slot_Click(object sender, EventArgs e)
        {
            Slot slot = sender as Slot;
            if (slot.Tag != null)
            {
                ItemSlot loot = Loot.Take(slot.Tag as ItemSlot);
                if (Player.Actor.GetComponent<Components.InventoryComponent>("Inventory").GiveItem(loot))
                {
                    slot.Tag = null;
                    SlotLabelMap[slot].Text = "";
                }
            }
        }
    }
}
