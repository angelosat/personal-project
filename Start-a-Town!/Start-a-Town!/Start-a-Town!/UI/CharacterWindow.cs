using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class CharacterWindow : Window
    {
        static CharacterWindow _Instance;
        public static CharacterWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new CharacterWindow();
                return _Instance;
            }
        }


        Panel EquipmentPanel, NeedsPanel, BuffsPanel, Panel_RBs;
        GameObject _Actor;
        public GameObject Actor
        {
            get { return _Actor; }
            set
            {
                //_Actor = value;
                //EquipmentContainer equipment = Actor.equipment;

                //Slot slot;

                //int eqcount = Enum.GetValues(typeof(EquipmentContainer.EquipmentTypes)).Length - 1;

                //EquipmentPanel.Controls.Clear();
                //EquipmentPanel.AutoSize = true;
                //for (byte n = 0; n < equipment.Size; n++)
                //{
                //    slot = new Slot(new Vector2(0, (n) * Slot.DefaultHeight));
                //    Label label = new Label(new Vector2(slot.Right, slot.Top + slot.Height / 2), equipment[n].Label, TextAlignment.Left, TextAlignment.Center);
                //    EquipmentPanel.Controls.Add(slot);
                //    EquipmentPanel.Controls.Add(label);

                //    slot.Item = equipment[n];

                //}


                //Bar bar;
                //_Actor = value;
                //int i = 0;
                //NeedCollection needs = Actor.GetComponent<NeedsComponent>("Needs").Needs;
                //Dictionary<int, Need>.KeyCollection keys = needs.Keys;
                //foreach (int needID in keys)
                //{
                //    bar = new Bar(new Vector2(0, i * UIManager.ProgressBarBorder.Height));
                //    bar.Text = needs[needID].Name;
                //    bar.Percentage = needs[needID].Value / (double)needs[needID].Max;
                //    NeedsPanel.Controls.Add(bar);
                //    i++;
                //}
                //needs.Updated += new EventHandler<EventArgs>(Needs_Updated);

                InventoryComponent inventory;
                if(Actor.TryGetComponent<InventoryComponent>("Inventory", out inventory))
                {
                    //Dictionary<string, ItemSlot> equipment = inventory.Equipment;
                    Dictionary<string, GameObjectSlot> equipment = inventory.GetProperty<Dictionary<string, GameObjectSlot>>("Equipment");
                    int n = 0;
                    //foreach(KeyValuePair<string, ItemSlot> pair in equipment)
                    foreach (KeyValuePair<string, GameObjectSlot> pair in equipment)
                    {
                        Slot slot = new Slot(new Vector2(0, (n++) * Slot.DefaultHeight));
                        Label label = new Label(new Vector2(slot.Right, slot.Top + slot.Height / 2), pair.Key, HorizontalAlignment.Left, VerticalAlignment.Center);
                        //if (pair.Value.Object != null)//.IsEmpty)
                        ///{
                            //slot.IconIndex = pair.Value.IconIndex;
                            slot.Tag = pair.Value;
                        //}
                        EquipmentPanel.Controls.Add(slot, label);
                    }
                }

                //for (int k = 0; k < Actor.Buffs.Count; k++)
                //{
                //    slot = new BuffSlot(new Vector2(0));

                //    slot.Item = Actor.Buffs[k];
                //    BuffsPanel.Controls.Add(slot);

                //}
                //Actor.Buffs.Changed += new EventHandler<EventArgs>(Buffs_Changed);

            }
        }

        void Buffs_Changed(object sender, EventArgs e)
        {
            //BuffsPanel.Controls.Clear();
            //Slot slot;
            //for (int k = 0; k < Player.Actor.Buffs.Count; k++)
            //{
            //    slot = new BuffSlot(new Vector2(0)); //(k % 3) * UIManager.containerSprite.Width, (k / 3) * UIManager.containerSprite.Height));
            //    //slot.Item = new ItemSlot(Player.Instance);
            //    //slot.Item.Item = Player.Instance.Buffs[k];
            //    slot.Item = Player.Actor.Buffs[k];
            //    BuffsPanel.Controls.Add(slot);
            //}
        }

        //void Needs_Updated(object sender, EventArgs e)
        //{
        //    NeedCollection needs = Player.Actor.GetComponent<NeedsComponent>("Needs").Needs;
        //    Dictionary<int, Need>.KeyCollection keys = needs.Keys;
        //    int i = 0;
        //    foreach (int needID in keys)
        //        (NeedsPanel.Controls[i++] as Bar).Percentage = needs[needID].Value / (double)needs[needID].Max;
        //}

        //public override bool Close()
        //{
        //    NeedCollection needs = Player.Actor.GetComponent<NeedsComponent>("Needs").Needs;
        //    needs.Updated -= Needs_Updated;
        //    return base.Close();
        //}


        //void slot_Click(object sender, EventArgs e)
        //{
        //    ItemSlot slot = (sender as Slot).Item as ItemSlot;
        //    if (slot != null)
        //    {
        //        InventoryItem item = slot.Item;
        //        //if (item != null)
        //        //{
        //        slot.Owner.GiveItem(slot);
        //        slot.Item = null;
        //        //}
        //    }
        //}

        private void OnEquipmentChanged()
        {
            Console.WriteLine("eq changed");
        }

        public CharacterWindow()
        {
            Location = Vector2.Zero;
            AutoSize = true;

            Panel_RBs = new Panel();
            Panel_RBs.AutoSize = true;
            RadioButton RB_Needs = new RadioButton("Needs", Vector2.Zero);
            RB_Needs.Checked = true;
            RadioButton RB_Equipment = new RadioButton("Equipment", new Vector2(0, RB_Needs.Bottom));
            RadioButton RB_Effects = new RadioButton("Effects", new Vector2(0, RB_Equipment.Bottom));
            Panel_RBs.Controls.Add(RB_Needs, RB_Equipment, RB_Effects);

            RB_Needs.CheckedChanged += new EventHandler<EventArgs>(RB_Needs_CheckedChanged);
            RB_Equipment.CheckedChanged += new EventHandler<EventArgs>(RB_Equipment_CheckedChanged);
            RB_Effects.CheckedChanged += new EventHandler<EventArgs>(RB_Effects_CheckedChanged);

            EquipmentPanel = new Panel(new Vector2(Panel_RBs.Right, 0));
            EquipmentPanel.AutoSize = true;
            //Slot slot;
            //Label label;
            //for (byte n = 0; n < EquipmentContainer.Count; n++)
            //{
            //    slot = new Slot(new Vector2(0, (n) * Slot.DefaultHeight));
            //    label = new Label(slot.Location + new Vector2(slot.Width, slot.Height / 2), EquipmentContainer.SlotNames[n], TextAlignment.Left, TextAlignment.Center);
            //    EquipmentPanel.Controls.Add(slot);
            //    EquipmentPanel.Controls.Add(label);
            //}

            NeedsPanel = new Panel(new Vector2(Panel_RBs.Right, 0));
            NeedsPanel.ClientSize = new Rectangle(0, 0, UIManager.ProgressBarBorder.Width, UIManager.ProgressBarBorder.Height * 2);

            BuffsPanel = new Panel(new Vector2(Panel_RBs.Right, 0));
            //BuffsPanel.ClientSize = new Rectangle(0, 0, UIManager.containerSprite.Width * 3, UIManager.containerSprite.Height * 2); 
            BuffsPanel.ClientSize = new Rectangle(0, 0, 100, UIManager.SlotSprite.Height * 4); 

            //Controls.Add(EquipmentPanel);
            Controls.Add(Panel_RBs);
            Controls.Add(NeedsPanel, EquipmentPanel, BuffsPanel);
            AutoSize = false;
            Controls.Remove(EquipmentPanel);
            Controls.Remove(BuffsPanel);
            //Controls.Add(BuffsPanel);
            //SizeToControl(EquipmentPanel);
            Title = "Character";
            Movable = true;
            Location = new Vector2(0, Hud.DefaultHeight);
        }

        void RB_Effects_CheckedChanged(object sender, EventArgs e)
        {
            //Controls.Clear();
            if ((sender as RadioButton).Checked)
                Controls.Add(BuffsPanel);
            else
                Controls.Remove(BuffsPanel);
        }

        void RB_Equipment_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                Controls.Add(EquipmentPanel);
            else
                Controls.Remove(EquipmentPanel);
        }

        void RB_Needs_CheckedChanged(object sender, EventArgs e)
        {
            if ((sender as RadioButton).Checked)
                Controls.Add(NeedsPanel);
            else
                Controls.Remove(NeedsPanel);
        }
    }
}
