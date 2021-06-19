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
        public new GameObject Tag;

        Panel Panel_Slots, Panel_Settings, Panel_ObjectTypes;
        RadioButton RB_FilterNone, RB_FilterIncl, RB_FilterExcl;
        CheckBox CHB_ObjectTypes;
        Rectangle DragBoxFromMouseDown = Rectangle.Empty;
        ContainerComponent ContainerComp;
        public static int LineMax = 4;
        public GameObject Container;
        ObjectFilter FilterOld;
        ObjectFilter2 Filter;
        public void InitSlots(GameObject obj)
        {
            Container = obj;
            if (Container == null)
            {
                Close();
                return;
            }
            Title = Container.Name;

           // InitInvSlots();

            Client.Controls.Add(Panel_Slots);
            Location = BottomRightScreen;// -new Vector2(Width / 2, Height / 2);

        }

        //private void InitInvSlots()
        //{

        //    ContainerComp = Container.GetComponent<ContainerComponent>("Container");

        //    List<GameObjectSlot> objSlots = ContainerComp.Slots;

        //    Client.Controls.Remove(Panel_Slots);
        //    Panel_Slots.Controls.Clear();

        //    for (int i = 0; i < objSlots.Count; i++)
        //        Panel_Slots.Controls.Add(new InventorySlot(objSlots[i], Container) { Filter = this.Filter, Location = new Vector2((i % LineMax) * UIManager.SlotSprite.Width, (i / LineMax) * UIManager.SlotSprite.Height) });
           
        //}

        void listView_DrawItem(object sender, DrawListViewItemEventArgs e)
        {
            e.SpriteBatch.Draw(UIManager.SlotSprite, e.Item.ScreenBounds, Color.White);
            GameObjectSlot slot = (GameObjectSlot)e.Item.Tag;
            if (slot != null)
            {
                GuiComponent gui;
                if(slot.Object.TryGetComponent<GuiComponent>("Gui", out gui))
                    e.SpriteBatch.Draw(gui.GetProperty<Icon>("Icon").SpriteSheet, e.Bounds, gui.GetProperty<Icon>("Icon").SourceRect, Color.White);
            }
        }

        void slot_MouseMove(object sender, EventArgs e)
        {
            if (Controller.Instance.msCurrent.LeftButton == ButtonState.Pressed)
                if (DragBoxFromMouseDown != Rectangle.Empty && !DragBoxFromMouseDown.Contains(new Rectangle(Controller.Instance.msCurrent.X, Controller.Instance.msCurrent.Y, 1, 1)))
                {
                    Slot slot = sender as Slot;
                    DragDropManager.Create(slot.Tag, slot, DragDropEffects.Move | DragDropEffects.Link);
                    DragBoxFromMouseDown = Rectangle.Empty;
                }
        }

        public override void Update()
        {
            base.Update();
            if (this.Tag.IsNull())
                return;
            if (Vector3.Distance(Player.Actor.Global, Tag.Global) > InteractionOld.DefaultRange)
                Hide();
        }

        public ContainerWindow(GameObject container)  
            : base()
        {
            Tag = container;
            Title = "Inventory";
            Movable = true;
            AutoSize = true;
            Anchor = new Vector2(1, 1);

            Location = Vector2.Zero;
            Panel_Slots = new Panel();
            Panel_Slots.AutoSize = true;
            Panel_Slots.ClientSize = new Rectangle(0, 0, 4 * UIManager.SlotSprite.Width, 4 * UIManager.SlotSprite.Width);
            Filter = (Tag["Container"]["Filter"] as ObjectFilter2);
            InitSlots(container);

            InitFilterPanel();
            
            Location = CenterScreen;
            AutoSize = false;
        }

        void InitFilterPanel()
        {
            
            if (Filter.Protected)
                return;
            Panel_Settings = new Panel(Panel_Slots.TopRight);
            RB_FilterNone = new RadioButton("None", Vector2.Zero, FilterOld.Type == FilterType.None) { Tag = FilterType.None };
            RB_FilterIncl = new RadioButton("Include", RB_FilterNone.BottomLeft, FilterOld.Type == FilterType.Include) { Tag = FilterType.Include };
            RB_FilterExcl = new RadioButton("Exclude", RB_FilterIncl.BottomLeft, FilterOld.Type == FilterType.Exclude) { Tag = FilterType.Exclude };
            GroupBox box = new GroupBox(Panel_Slots.TopRight) { Name = "Filter box" };
            box.Controls.Add(RB_FilterNone, RB_FilterIncl, RB_FilterExcl);
            Client.Controls.Add(box);

            RB_FilterExcl.LeftClick += new UIEvent(RB_FilterExcl_MouseLeftPress);
            RB_FilterNone.LeftClick += new UIEvent(RB_FilterNone_MouseLeftPress);
            RB_FilterIncl.LeftClick += new UIEvent(RB_FilterIncl_MouseLeftPress);

            Panel_ObjectTypes = new Panel() { Location = box.BottomLeft, Color = Color.Black, AutoSize = true };

            List<GameObject> objects = GameObject.Objects.Values.ToList().FindAll(foo => foo.Type == ObjectType.Material || foo.Type == ObjectType.Consumable);

            Control prev = null;
            foreach (GameObject mat in objects)
            {
                CheckBox chb_material = new CheckBox(mat.Name, prev == null ? Vector2.Zero : prev.BottomLeft) { Tag = mat };
                List<GameObject.Types> list;
                if (FilterOld.TryGetValue(mat.Type, out list))
                    if (list.Contains(mat.ID))
                        chb_material.Checked = true;
                chb_material.LeftClick += new UIEvent(chb_material_Click);
                prev = chb_material;
                Panel_ObjectTypes.Controls.Add(chb_material);
            }

            if (FilterOld.Type != FilterType.None)
                Panel_Settings.Controls.Add(Panel_ObjectTypes);
        }

        void chb_material_Click(object sender, EventArgs e)
        {
            CheckBox ch = sender as CheckBox;
            GameObject obj = ch.Tag as GameObject;
            FilterOld.Set(ch.Checked, obj);
        }

        void RB_FilterNone_MouseLeftPress(object sender, EventArgs e)
        {
            Client.Controls.Remove(Panel_ObjectTypes);
            FilterOld.Type = FilterType.None;
        }

        void RB_FilterIncl_MouseLeftPress(object sender, EventArgs e)
        {
            Client.Controls.Add(Panel_ObjectTypes);
            FilterOld.Type = FilterType.Include;
        }

        void RB_FilterExcl_MouseLeftPress(object sender, EventArgs e)
        {
            Client.Controls.Add(Panel_ObjectTypes);
            FilterOld.Type = FilterType.Exclude;
        }

    }
}
