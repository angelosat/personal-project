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
    class ContainerWindow : Window
    {
        Panel Panel_Slots, Panel_Settings, Panel_ObjectTypes;
        RadioButton RB_FilterNone, RB_FilterIncl, RB_FilterExcl;
        CheckBox CHB_ObjectTypes;
        Rectangle DragBoxFromMouseDown = Rectangle.Empty;
        ContainerComponent Container;
        public static int LineMax = 4;
        public GameObject Object;
        ObjectFilter Filter;
        public void InitSlots(GameObject obj)
        {
            Object = obj;
            if (Object == null)
            {
                Close();
                return;
            }
            Title = Object.Name;



            InitInvSlots();




            Client.Controls.Add(Panel_Slots);
            Location = BottomRightScreen;// -new Vector2(Width / 2, Height / 2);

        }

        private void InitInvSlots()
        {
            foreach (Slot slot in Panel_Slots.Controls)
            {
                slot.DragDrop -= slot_DragDrop;
              //  slot.KeyPress -= slot_KeyPress;
                //slot.MouseMove -= slot_MouseMove;
                slot.MouseLeftPress -= slot_MouseLeftPress;
                slot.MouseRightPress -= slot_MouseRightPress;
                slot.DrawTooltip -= slot_DrawTooltip;
                slot.MouseRightUp -= slot_MouseRightUp;
            }

            Container = Object.GetComponent<ContainerComponent>("Container");

            List<GameObjectSlot> objSlots = Container.Slots;

                Controls.Remove(Panel_Slots);
                Panel_Slots.Controls.Clear();

                for (int i = 0; i < objSlots.Count; i++)
                {
                    GameObjectSlot invSlot = objSlots[i];
                    Slot slot;
                    slot = new Slot(new Vector2((i % LineMax) * UIManager.SlotSprite.Width, (i / LineMax) * UIManager.SlotSprite.Height));
                    slot.ID = i;
                    slot.CustomTooltip = true;

                    if (invSlot != null)
                    {
                        slot.Tag = invSlot;
                    }
                    Panel_Slots.Controls.Add(slot);
                    slot.DrawTooltip += new EventHandler<TooltipArgs>(slot_DrawTooltip);
                    if (Player.Actor == null)
                        continue;

                    slot.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(slot_MouseLeftPress);
                    slot.MouseRightPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(slot_MouseRightPress);
                    slot.MouseRightUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(slot_MouseRightUp);
                 //   slot.KeyPress += new EventHandler<KeyPressEventArgs2>(slot_KeyPress);
                    slot.DragDrop += new EventHandler<DragDropArgs>(slot_DragDrop);


                }
            
        }









        void slot_MouseRightUp(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            Slot uiSlot = (sender as Slot);
            if (!uiSlot.Pressed)
                return;
            uiSlot.Alpha = Color.White;
            GameObjectSlot slot = (sender as Slot).Tag as GameObjectSlot;
            if (slot == null)
                return;
            if (slot.Object == null)
                return;
            
            //if(Player.Actor.Give(Object, slot.Object))
            if (InventoryComponent.GiveObject(Player.Actor, slot.Object, slot.StackSize))
            {
                slot.Object = null;
                slot.StackSize = 0;
            }
        }

        void slot_DrawTooltip(object sender, TooltipArgs e)
        {
            GameObjectSlot slot = (sender as Slot).Tag as GameObjectSlot;
            GameObject obj = slot.Object as GameObject;
            if (obj == null)
                return;

            //obj.GetTooltipInfo(e.Tooltip);
            slot.GetTooltipInfo(e.Tooltip);
            float bottom = 0;
            foreach (Control control in e.Tooltip.Controls)
            {
                bottom = Math.Max(bottom, control.Bottom);
            }
            if (Player.Actor == null)
                return;

            foreach (Component comp in obj.Components.Values)
            {
                string invText = comp.GetInventoryText(obj, Player.Actor);
                if (invText.Length == 0)
                    continue;
                Label label = new Label(new Vector2(0, e.Tooltip.ClientSize.Bottom), invText);
                GroupBox box = new GroupBox();
                box.Controls.Add(label);
                e.Tooltip.Controls.Add(box);
            }

            
        }
        void slot_DragDrop(object sender, DragDropArgs e)
        {
            GameObjectSlot source = DragDropManager.Instance.Source as GameObjectSlot;
            GameObjectSlot target = (sender as Slot).Tag;
            GameObjectSlot drag = DragDropManager.Instance.Item as GameObjectSlot;
            if (target.Object == drag.Object)
            {
                target.StackSize += drag.StackSize;
                source.StackSize -= drag.StackSize;
                return;
            }
            if (target.Object == null)
            {
                target.Object = source.Object;
                target.StackSize = drag.StackSize;
                source.StackSize -= drag.StackSize;
                return;
            }
            if (source.Object != target.Object)
            {
                if (source.Object == DragDropManager.Instance.Item)
                    target.Swap(source);
                else
                    target.Swap(drag);
                return;
            }

            //Slot target = sender as Slot;
            //Slot source = DragDropManager.Instance.Source as Slot;
            //object tag = target.Tag;
            //GameObjectSlot targetInvSlot = target.Tag as GameObjectSlot;
            //GameObjectSlot sourceInvSlot = source.Tag as GameObjectSlot;
            //sourceInvSlot.Swap(targetInvSlot);
        }
        //void slot_DragDrop(object sender, DragDropArgs e)
        //{
        //    Slot target = sender as Slot;
        //    Slot source = DragDropManager.Instance.Source as Slot;
        //    object tag = target.Tag;
        //    GameObjectSlot targetInvSlot = target.Tag as GameObjectSlot;
        //    GameObjectSlot sourceInvSlot = source.Tag as GameObjectSlot;
        //    sourceInvSlot.Swap(targetInvSlot);
        //}


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
               // slot.KeyPress -= slot_KeyPress;
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

          //  Interaction drop = InteractionManager.GetInteraction(6);
            
        }

        void slot_MouseLeftPress(object sender, EventArgs e)
        {
            //DragBoxFromMouseDown = new Rectangle(Controller.Instance.msCurrent.X - 4, Controller.Instance.msCurrent.Y - 4, 8, 8);
            DragBoxFromMouseDown = new Rectangle((int)((Controller.Instance.msCurrent.X - 4) / UIManager.Scale), (int)((Controller.Instance.msCurrent.Y - 4) / UIManager.Scale), (int)(8 * UIManager.Scale), (int)(8 * UIManager.Scale));
            Slot slot = sender as Slot;
            if (DragDropManager.Instance.Item != null)
            {
                slot.Drop(DragDropManager.Instance.Item);
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
                    slot.DoDragDrop(invSlot, DragDropEffects.Move | DragDropEffects.Link);
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
                    slot.DoDragDrop(slot.Tag, DragDropEffects.Move | DragDropEffects.Link);
                    DragBoxFromMouseDown = Rectangle.Empty;
                }
        }


        public ContainerWindow(GameObject container)  
            : base()
        {
            Tag = container;
            Title = "Inventory";
            Movable = true;
            AutoSize = true;
            Anchor = new Vector2(1, 1);

         //   Location = CenterScreen;

            Location = Vector2.Zero;
            Panel_Slots = new Panel();
            Panel_Slots.AutoSize = true;
            Panel_Slots.ClientSize = new Rectangle(0, 0, 4 * UIManager.SlotSprite.Width, 4 * UIManager.SlotSprite.Width);
            InitSlots(container);

            Panel_Settings = new Panel(Panel_Slots.TopRight);
            Filter = (container["Container"]["Filter"] as ObjectFilter);
            RB_FilterNone = new RadioButton("None", Vector2.Zero, Filter.Type == FilterType.None) {Tag = FilterType.None};//.SetTag(FilterType.None);
            RB_FilterIncl = new RadioButton("Include", RB_FilterNone.BottomLeft, Filter.Type == FilterType.Include) { Tag = FilterType.Include };//.SetTag(FilterType.Include);
            RB_FilterExcl = new RadioButton("Exclude", RB_FilterIncl.BottomLeft, Filter.Type == FilterType.Exclude) { Tag = FilterType.Exclude };
            Controls.Add(new GroupBox(Panel_Slots.TopRight).SetControls(RB_FilterNone, RB_FilterIncl, RB_FilterExcl));

            RB_FilterExcl.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(RB_FilterExcl_MouseLeftPress);
            RB_FilterNone.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(RB_FilterNone_MouseLeftPress);
            RB_FilterIncl.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(RB_FilterIncl_MouseLeftPress);

            Panel_ObjectTypes = new Panel(Controls.Last().BottomLeft);//RB_FilterExcl.BottomLeft);
            Panel_ObjectTypes.AutoSize = true;

            List<GameObject> objects = GameObject.Objects.Values.ToList().FindAll(foo => foo.Type == ObjectType.Material || foo.Type == ObjectType.Consumable);
         //   objects.AddRange(GameObject.Objects.Values.ToList().FindAll(foo => foo.Type == ObjectType.Consumable));
            Control prev = null;
            foreach (GameObject mat in objects)
            {
                CheckBox chb_material = new CheckBox(mat.Name, prev == null ? Vector2.Zero : prev.BottomLeft) { Tag = mat };//.SetTag(mat);//.SetChecked(Filter[mat.Type].Contains(mat.ID));
                List<GameObject.Types> list;
                if(Filter.TryGetValue(mat.Type, out list))
                    if(list.Contains(mat.ID))
                        chb_material.Checked = true;
                chb_material.Click += new UIEvent(chb_material_Click);
                prev = chb_material;
                Panel_ObjectTypes.Controls.Add(chb_material);
            }

            //CHB_ObjectTypes = new CheckBox("Materials", Vector2.Zero).SetTag(ObjectType.Material).SetChecked(Filter.ObjectTypes.Contains(ObjectType.Material));
            //CHB_ObjectTypes.CheckedChanged += new EventHandler<EventArgs>(CHB_ObjectTypes_CheckedChanged);
           
      //      Panel_ObjectTypes.Controls.Add(CHB_ObjectTypes);
            if (Filter.Type != FilterType.None)
                Controls.Add(Panel_ObjectTypes);
            Location = CenterMouseOnControl(Panel_Slots.Controls.First());
            //switch ((container["Container"]["Filter"] as ObjectFilter).Type)
            //{
            //    case FilterType.Include:
            //        RB_FilterIncl.Checked = true;
            //        break;
            //    case FilterType.Exclude:
            //        RB_FilterExcl.Checked = true;
            //        break;
            //    default:
            //        RB_FilterNone.Checked = true;
            //        break;
            //}
            

            
  //          Location = UIManager.Mouse - ClientLocation - Panel_Slots.Location - Panel_Slots.Controls.First().Location - new Vector2(Panel_Slots.Controls.First().Width, Panel_Slots.Controls.First().Height) / 2;



        //    Controls.Add(Panel_Slots);
           // AutoSize = false;
        }

        void chb_material_Click(object sender, EventArgs e)
        {
            CheckBox ch = sender as CheckBox;
            GameObject obj = ch.Tag as GameObject;
            Filter.Set(ch.Checked, obj);
            //GameObject.Types type = (GameObject.Types)ch.Tag;
            //if (ch.Checked)
            //{
            //    //if (!Filter.ObjectIDs.Contains(type))
            //    //    Filter.ObjectIDs.Add(type);

            //}
            //else
            //{
            //    if (Filter.ObjectIDs.Contains(type))
            //        Filter.ObjectIDs.Remove(type);
            //}
        }

        //void CHB_ObjectTypes_CheckedChanged(object sender, EventArgs e)
        //{
        //    string type = (string)CHB_ObjectTypes.Tag;
        //    if (CHB_ObjectTypes.Checked)
        //    {
        //        if (!Filter.ObjectTypes.Contains(type))
        //            Filter.ObjectTypes.Add(type);
        //    }
        //    else
        //    {
        //        if (Filter.ObjectTypes.Contains(type))
        //            Filter.ObjectTypes.Remove(type);
        //    }
        //}

        void RB_FilterNone_MouseLeftPress(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            Controls.Remove(Panel_ObjectTypes);
            Filter.Type = FilterType.None;
        }

        void RB_FilterIncl_MouseLeftPress(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            Controls.Remove(Panel_ObjectTypes);
            Controls.Add(Panel_ObjectTypes);
            Filter.Type = FilterType.Include;
        }

        void RB_FilterExcl_MouseLeftPress(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            Controls.Remove(Panel_ObjectTypes);
            Controls.Add(Panel_ObjectTypes);
            Filter.Type = FilterType.Exclude;
        }

    }
}
