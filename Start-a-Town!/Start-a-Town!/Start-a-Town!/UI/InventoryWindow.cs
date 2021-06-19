using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class InventoryWindow : Window
    {
        Panel Panel_Slots;
        Rectangle DragBoxFromMouseDown = Rectangle.Empty;
        InventoryComponent Inventory;
        public static int LineMax = 4;
        ListView SlotList;
        GameObject _Actor;
        public GameObject Actor
        {
            get { return _Actor; }
            set
            {
                if (_Actor != null)
                {
                    foreach (Slot slot in Panel_Slots.Controls)
                    {
                        slot.DragDrop -= slot_DragDrop;
                        slot.KeyPress -= slot_KeyPress;
                        //slot.MouseMove -= slot_MouseMove;
                        slot.MouseLeftPress -= slot_MouseLeftPress;
                        slot.MouseRightPress -= slot_MouseRightPress;
                        slot.DrawTooltip -= slot_DrawTooltip;
                        slot.MouseRightUp -= slot_MouseRightUp;
                    }
                }
                _Actor = value;
                if (_Actor != null)
                {
                    //Inventory inventory = Actor.GetComponent<InventoryComponent>("Inventory").Inventory;
                    //Controls.Remove(Panel_Slots);
                    //Panel_Slots.Controls.Clear();
                    //SlotList = new ListView(Vector2.Zero, new Rectangle(0, 0, 300, 300));
                    ////listView.ItemSize = UIManager.SlotSprite.Bounds;
                    //SlotList.ItemBackground = UIManager.SlotSprite;
                    //SlotList.Width = 4 * UIManager.SlotSprite.Width;
                    //SlotList.Height = SlotList.Width;
                    ////listView.OwnerDraw = true;
                    //SlotList.DrawItem += new EventHandler<DrawListViewItemEventArgs>(listView_DrawItem);
                    //for (int i = 0; i < inventory.Size; i++)
                    //{
                    //    ListViewItem listItem = new ListViewItem();
                    //    listItem.Index = i;
                    //    SlotList.Items.Add(listItem);
                    //    ItemSlot invItem = inventory[i];
                    //    listItem.Tag = invItem;
                    //    if (invItem == null)
                    //        continue;
                    //    //listItem.IconIndex = invItem.Data != null ? invItem.Data.IconIndex : -1;
                    //    listItem.IconIndex = invItem.Data.IconIndex;
                    //    listItem.Name = invItem.Data.Name;
                    //}
                    //Panel_Slots.AutoSize = true;
                    //Panel_Slots.Controls.Add(SlotList);
                    //Controls.Add(Panel_Slots);
                    //Location = BottomRightScreen - new Vector2(Width / 2, Height / 2);


                    Inventory = Actor.GetComponent<InventoryComponent>("Inventory");
                    //Inventory.Updated += new EventHandler<InventoryEventArgs>(inventory_Changed);
                    ItemContainer container;
                    if (!Inventory.TryGetContainer("Bag1", out container))
                        return;
                    Controls.Remove(Panel_Slots);
                    Panel_Slots.Controls.Clear();
                    Slot slot;
                    for (int i = 0; i < container.Count; i++)
                    {
                        GameObjectSlot itemSlot = container[i];
                        //GameObject obj = container[i];
                        slot = new Slot(new Vector2((i % LineMax) * UIManager.SlotSprite.Width, (i / LineMax) * UIManager.SlotSprite.Height));
                        slot.ID = i;
                        slot.CustomTooltip = true;
                        //slot.Tag = obj;
                        if (itemSlot != null)
                        {
                            slot.Tag = itemSlot;
                        }

                        slot.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(slot_MouseLeftPress);
                        slot.MouseRightPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(slot_MouseRightPress);
                        slot.MouseRightUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(slot_MouseRightUp);
                        slot.KeyPress += new EventHandler<KeyPressEventArgs2>(slot_KeyPress);
                        slot.DragDrop += new EventHandler<DragEventArgs>(slot_DragDrop);
                        slot.DrawTooltip += new EventHandler<TooltipArgs>(slot_DrawTooltip);
                        Panel_Slots.Controls.Add(slot);
                    }
                    Controls.Add(Panel_Slots);
                    Location = BottomRightScreen - new Vector2(Width / 2, Height / 2);
                }
            }
        }

        void slot_MouseRightUp(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            Slot uiSlot = (sender as Slot);
            uiSlot.Alpha = Color.White;
            GameObjectSlot slot = (sender as Slot).Tag as GameObjectSlot;
            if (slot == null)
                return;
            if (slot.Object == null)
                return;
            if (slot.Object.Components.ContainsKey("Equip"))
            {
                slot.Object.GetComponent<EquipComponent>("Equip").Equip(Player.Actor, slot.Object);
                //Interaction equip = InteractionManager.GetInteraction(3);
                //ShortcutMenu.Add(equip.Name, delegate() { Player.Actor.GetComponent<TasksComponent>("Tasks").TaskAssign(new Task(Player.Actor, slot.Object, 3)); }, 3);
            }
            //InventorySlot slot = (sender as Slot).Tag as InventorySlot;
            //if (slot == null)
            //    return;
            //if (slot.Object == null)
            //    return;
            //if (slot.Object.Components.ContainsKey("Equip"))
            //{
            //    slot.Object.GetComponent<EquipComponent>("Equip").Equip(Player.Actor, slot.Object);
            //    //Interaction equip = InteractionManager.GetInteraction(3);
            //    //ShortcutMenu.Add(equip.Name, delegate() { Player.Actor.GetComponent<TasksComponent>("Tasks").TaskAssign(new Task(Player.Actor, slot.Object, 3)); }, 3);
            //}
        }

        void slot_DrawTooltip(object sender, TooltipArgs e)
        {
            GameObjectSlot slot = (e.Tooltip.Tag as Slot).Tag as GameObjectSlot;
            GameObject obj = slot.Object as GameObject;
            if (obj == null)
                return;
            //e.Tooltip.Controls.AddRange(obj.TooltipGroups);
            obj.GetTooltipInfo(e.Tooltip);
            if (obj.Components.ContainsKey("Equip"))
            {
                Label label = new Label("Right click: Equip");
                GroupBox box = new GroupBox();
                box.Controls.Add(label);
                e.Tooltip.Controls.Add(box);
            }
        }

        void slot_DragDrop(object sender, DragEventArgs e)
        {
            Slot target = sender as Slot;
            Slot source = DragDropManager.Instance.Source as Slot;
            object tag = target.Tag;
            GameObjectSlot targetInvSlot = target.Tag as GameObjectSlot;
            GameObjectSlot sourceInvSlot = source.Tag as GameObjectSlot;
            sourceInvSlot.Swap(targetInvSlot);
        }


        void listView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            ////ListViewItem listItem = (ListViewItem)sender;
            //e.SpriteBatch.Draw(UIManager.SlotSprite, e.Item.Bounds, Color.White);
            //ItemSlot itemslot = (ItemSlot)e.Item.Tag;
            //if (itemslot != null)
            //    //e.SpriteBatch.Draw(ItemManager.ItemSheet, e.Item.Index * ((ListView)sender).ItemSize, ItemManager.Icons[itemslot.Data.IconIndex], Color.White);
            //    e.SpriteBatch.Draw(ItemManager.Instance.ItemSheet, e.Bounds, ItemManager.Instance.Icons[itemslot.Data.IconIndex], Color.White);

            e.SpriteBatch.Draw(UIManager.SlotSprite, e.Item.Bounds, Color.White);
            GameObjectSlot slot = (GameObjectSlot)e.Item.Tag;
            if (slot != null)
            {
                GuiComponent gui;
                if(slot.Object.TryGetComponent<GuiComponent>("Gui", out gui))
                    e.SpriteBatch.Draw(gui.GetProperty<Icon>("Icon").SpriteSheet, e.Bounds, gui.GetProperty<Icon>("Icon").SourceRect, Color.White);
            }
                //e.SpriteBatch.Draw(ItemManager.Instance.ItemSheet, e.Bounds, ItemManager.Instance.Icons[slot.Data.IconIndex], Color.White);
        }

        //void slot_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    Slot slot = (Slot)sender;
        //    e.SpriteBatch.Draw(UIManager.SlotSprite, e.Bounds, Color.White);
        //    ItemSlot itemslot = (ItemSlot)slot.Item;
        //    if (itemslot != null)
        //        e.SpriteBatch.Draw(ItemManager.ItemSheet, e.Bounds, ItemManager.Icons[itemslot.Data.IconIndex], Color.White);
        //}

        void slot_KeyPress(object sender, KeyPressEventArgs2 e)
        {
            //Slot slot = sender as Slot;
            //char key = (char)((int)e.Key);
            //int n = (int)Char.GetNumericValue(key);
            ////Console.WriteLine(n + " "  + list.HoverIndex);
            //if (n >= 0 && n <= 9)
            //{
            //    Slot quickslot = QuickBar.Instance.Controls[n > 0 ? n - 1 : 10] as Slot;
            //    //quickslot.Item =  list.Items[list.HoverIndex] as Task;// as Task).Interaction;

            //    //quickslot.Item = slot.Item;
            //    quickslot.Tag = slot.Tag;
            //    e.Input.Handled = true;
            //}

            Slot slot = sender as Slot;
            if (e.Key == GlobalVars.KeyBindings.QuickSlot1)
            {
                Player.Instance.Tool.Hotbar[0] = slot.Tag as GameObjectSlot;
                //Player.Instance.Tool.Hotbar[0].Object = (slot.Tag as InventorySlot).Object;
            }
            else if (e.Key == GlobalVars.KeyBindings.QuickSlot2)
            {
                Player.Instance.Tool.Hotbar[1] = slot.Tag as GameObjectSlot;
                //Player.Instance.Tool.Hotbar[1].Object = (slot.Tag as InventorySlot).Object;
            }
        }

        public override void Dispose()
        {
            foreach (Slot slot in Panel_Slots.Controls)
            {
                ////slot.DragDrop -= slot_DragDrop;
                ////slot.DrawItem -= slot_DrawItem;
                //slot.MouseMove -= slot_MouseMove;
                //slot.MouseDown -= slot_MouseLeftPress;
                //slot.MouseRightPress -= slot_MouseRightPress;
                slot.DragDrop -= slot_DragDrop;
                slot.KeyPress -= slot_KeyPress;
                slot.MouseLeftPress -= slot_MouseLeftPress;
                slot.MouseRightPress -= slot_MouseRightPress;
                slot.DrawTooltip -= slot_DrawTooltip;
                slot.MouseRightUp -= slot_MouseRightUp;
            }
            //Inventory.Updated -= inventory_Changed;
            base.Dispose();
        }
        
        void slot_MouseRightPress(object sender, EventArgs e)
        {
            Slot uiSlot = (sender as Slot);
            uiSlot.Alpha = Color.Gold;

            GameObjectSlot slot = (sender as Slot).Tag as GameObjectSlot;
            if (slot == null)
                return;
            if (slot.Object == null)
                return;
            //Interaction drop = InteractionManager.GetInteraction(6);
            
        }

        void slot_MouseLeftPress(object sender, EventArgs e)
        {
            DragBoxFromMouseDown = new Rectangle(Controller.Instance.msCurrent.X - 4, Controller.Instance.msCurrent.Y - 4, 8, 8);
            Slot slot = sender as Slot;
            if (DragDropManager.Instance.Item != null)
            {
                slot.Drop(DragDropManager.Args);//.Instance.Item);
                DragDropManager.Instance.Item = null;
                return;
            }
            if (slot.Tag != null)
            {
                //StaticObject obj = slot.Tag as StaticObject;
                //if (obj != null)
                //    slot.DoDragDrop(obj, DragDropEffects.Move | DragDropEffects.Link);

                GameObjectSlot invSlot = slot.Tag as GameObjectSlot;
                if (invSlot.Object != null)
                    //slot.DoDragDrop(invSlot.Object, DragDropEffects.Move | DragDropEffects.Link);
                    DragDropManager.Create(invSlot.Object, invSlot, DragDropEffects.Move | DragDropEffects.Link);
            }
            //Console.WriteLine("wtf gamw down");
        }

        void slot_MouseMove(object sender, EventArgs e)
        {
            //Console.WriteLine("wtf gamw move");
            if (Controller.Instance.msCurrent.LeftButton == ButtonState.Pressed)
                if (DragBoxFromMouseDown != Rectangle.Empty && !DragBoxFromMouseDown.Contains(new Rectangle(Controller.Instance.msCurrent.X, Controller.Instance.msCurrent.Y, 1, 1)))
                {
                    Slot slot = sender as Slot;
                    //slot.DoDragDrop(slot.Tag, DragDropEffects.Move | DragDropEffects.Link);
                    DragDropManager.Create(slot.Tag, slot, DragDropEffects.Move | DragDropEffects.Link);
                    DragBoxFromMouseDown = Rectangle.Empty;
                }
        }

        //public override List<GroupBox> TooltipGroups
        //{
        //    get
        //    {
        //        //Console.WriteLine(SlotList.MouseHoverItem == null ? "null" : SlotList.MouseHoverItem.ToString());
        //        //int a = delegate(ListViewItem item) { item.Index; };  
        //        if (SlotList.MouseHoverItem != null)
        //            return ((ItemSlot)SlotList.MouseHoverItem.Tag).TooltipGroups;
        //        else
        //            return null;
        //    }
        //}

        //void slot_DragDrop(object sender, DragDropArgs e)
        //{
        //    Slot slot = sender as Slot;
        //    //
        //    if ((e.Data as ItemSlot).Item is ItemBase)
        //    {
        //        //SlottableItem currentItem = slot.Item.Item;
        //        ISlottable currentItem = slot.Item;

        //        if (((int)DragDropManager.Instance.Effects & (int)DragDropEffects.Move) == (int)DragDropEffects.Move)
        //        {
        //            //slot.Item.Item = (e.Data as ItemSlot).Item;
        //            //DragDropManager.Instance.Source.Item.Item = currentItem;
        //            slot.Item = (e.Data as ItemSlot).Item;
        //            DragDropManager.Instance.Source.Item = currentItem;
        //        }
        //    }
        //}

        //void slot_Click(object sender, EventArgs e)
        //{
        //    Slot slot = sender as Slot;
        //    if (slot.Item != null)
        //    {
        //        //InventoryItem item = slot.Item as InventoryItem;
        //        ItemSlot item = slot.Item as ItemSlot;
        //        if (item.Item != null)
        //        {
        //            //if (item.Item.IsEquippable)
        //            //    item.Owner.Equip(item);
        //            //else
        //            //if(item.Item
        //                item.Owner.TaskAssign(new Tasks.Task(item.Owner, item, item.Item.Interaction));
        //        }
        //    }
        //}

        public InventoryWindow()  
            : base()
        {
            Title = "Inventory";
            Movable = true;
            AutoSize = true;
            //panel = new Panel();
            //panel.AutoSize = true;
            //panel.ClientSize = new Rectangle(0, 0, 100, 100);
            //Controls.Add(panel);
            //ScreenLocation = CenterScreen;
            Location = CenterScreen;
            Panel_Slots = new Panel();
            Panel_Slots.AutoSize = true;
        }
    }
}
