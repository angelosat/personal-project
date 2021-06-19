using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Crafting;
using Start_a_Town_.Components;


namespace Start_a_Town_.UI
{
    class ConstructionWindow : Window
    {
        static ConstructionWindow _Instance;
        public static ConstructionWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ConstructionWindow();
                return _Instance;
            }
        }

        Panel Panel_image, Panel_description, Panel_properties, Panel_buttons, Panel_list, Panel_Help, Panel_Info, Panel_Styles;

        ScrollableList List;
        VScrollbar Scroll;

        static public Blueprint SelectedItem;


        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }
        
        //SortedDictionary<uint, Plan> Plans;
        List<Blueprint> Blueprints;

        int k;
        public ConstructionWindow()
        {
            Title = "Construction";
            Size = new Rectangle(0, 0, 200, 300);
            Tool = new ObjectPlaceTool();
            Panel_list = new Panel(new Vector2(0, 0), new Vector2(200, 200));
            
            Panel_image = new Panel(new Vector2(Panel_list.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));

            AutoSize = true;
            Movable = true;

            Blueprints = BlueprintComponent.Blueprints.Values.ToList();

            //k = 0;

            //SelectedItem = Blueprints[k];

            //Panel_list.AutoSize = true;

            List = new ScrollableList(Vector2.Zero, Panel_list.ClientSize);
            List.ControlAdded += new EventHandler<EventArgs>(List_ControlAdded);
            List.ControlRemoved += new EventHandler<EventArgs>(List_ControlRemoved);

            Scroll = new VScrollbar(new Vector2(List.ClientSize.Width - 16, 0), List.Height);
            Scroll.Tag = List;
            int n = 0;
            foreach (Blueprint bp in Blueprints)
            {
                Label listEntry = new Label(new Vector2(0, Label.DefaultHeight * n++), bp.Name);
                listEntry.Tag = bp;
                listEntry.LeftClick += new UIEvent(listEntry_Click);
                listEntry.DrawMode = UI.DrawMode.OwnerDrawFixed;
                listEntry.DrawItem += new EventHandler<DrawItemEventArgs>(listEntry_DrawItem);
                List.Add(listEntry);
            }

            Panel_list.Controls.Add(List);

            Panel_Styles = new Panel(new Vector2(0, Panel_list.Bottom), new Vector2(Panel_list.Width, 100));
            Panel_Styles.AutoSize = true;

            Panel_Info = new Panel(new Vector2(0, Panel_list.Bottom), new Vector2(Panel_list.Height, Panel_list.Width));


            Panel_buttons = new Panel(new Vector2(0, Panel_Info.Bottom), new Vector2(Panel_Info.Width, 23));

            Controls.Add(Panel_list, Panel_Info);
            this.AutoSize = false;
            Location = BottomLeftScreen;
        }

        void List_DrawTooltip(object sender, TooltipArgs e)
        {
            ListItem listItem = e.Tooltip.Tag as ListItem;
            if (listItem == null)
                return;
            Blueprint bp = listItem.Tag as Blueprint;
            string text = bp.Name;
            GroupBox box = new GroupBox();
            Label name = new Label(bp.Name);
            box.Controls.Add(name);

            int i = 0;
            foreach (BlueprintStage stage in bp.Stages)
            {
                box.Controls.Add(new Label(new Vector2(0, i++ * Label.DefaultHeight), "Stage " + bp.Stages.FindIndex(foo=>foo == stage)));
                foreach (KeyValuePair<GameObject.Types, int> pair in stage)
                {
                    Slot slot = new Slot(new Vector2(0, name.Bottom + i++ * Slot.DefaultHeight));
                    GameObject mat = GameObject.Create(pair.Key);
                    GameObjectSlot invSlot = new GameObjectSlot();
                    invSlot.Object = mat;
                    mat.GetComponent<GuiComponent>("Gui").Properties["StackSize"] = pair.Value;
                    slot.Tag = invSlot;
                    box.Controls.Add(slot);
                }
            }
            //int i = 0;
            //foreach (KeyValuePair<GameObject.Types, int> pair in bp.Materials)
            //{
            //    Slot slot = new Slot(new Vector2(0, name.Bottom + i * Slot.DefaultHeight));
            //    GameObject mat = GameObject.Create(pair.Key);
            //    GameObjectSlot invSlot = new GameObjectSlot();
            //    invSlot.Object = mat;
            //    mat.GetComponent<GuiComponent>("Gui").Properties["StackSize"] = pair.Value;
            //    slot.Tag = invSlot;
            //    box.Controls.Add(slot);
            //}
            

            e.Tooltip.Controls.Add(box);
        }

        void InitInfoPanel(Blueprint blueprint)
        {
            foreach(Control control in Panel_Info.Controls)
                if(control is Slot)
                    control.DrawTooltip -= materialSlot_DrawTooltip;
            Panel_Info.Controls.Clear();


            GameObject obj = GameObject.Objects[blueprint.ProductID];

            Sprite sprite = obj.GetComponent<SpriteComponent>("Sprite").Sprite;
            Variation = Vector2.Zero;
            int k = 0, n = 0;
            foreach (PictureBox picBox in Panel_Styles.Controls)
            {
                picBox.DrawItem -= variation_DrawItem;
                picBox.MouseLeftPress -= variation_MouseLeftPress;
            }
            Panel_Styles.Controls.Clear();
            foreach (Rectangle[] strip in sprite.SourceRects)
            {
                foreach (Rectangle rect in strip)
                {
                    PictureBox variation = new PictureBox(new Vector2(k * rect.Width, n * rect.Height), sprite.Texture, rect, HorizontalAlignment.Left, VerticalAlignment.Top);
                    variation.Tag = new Vector2(k, n);
                    variation.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(variation_MouseLeftPress);
                    variation.DrawMode = UI.DrawMode.OwnerDrawVariable;
                    variation.DrawItem += new EventHandler<DrawItemEventArgs>(variation_DrawItem);
                    Panel_Styles.Controls.Add(variation);
                    k += 1;// variation.Width;
                }
                n += 1;
                k = 0;
            }

            Controls.Add(Panel_Styles);

            Panel_Styles.Location = new Vector2(Panel_list.Right, List.Controls[List.Controls.FindIndex(foo => foo.Tag == blueprint)].Top);

            string text = UIManager.WrapText(obj.GetInfo().ToString(), Panel_Info.ClientSize.Width);
            Label label = new Label(text + "\nMaterials:");
            Panel_Info.Controls.Add(label);

            int i = label.Bottom;
            //foreach (BlueprintStage stage in SelectedItem.Stages)
            for (int num = 0; num < blueprint.Stages.Count; num++)
            {
                BlueprintStage stage = blueprint.Stages[num];
                Panel_Info.Controls.Add(new Label(new Vector2(0, i), "Stage " + num));
                i += Label.DefaultHeight;
                foreach (KeyValuePair<GameObject.Types, int> pair in stage)
                //foreach (KeyValuePair<GameObject.Types, int> pair in SelectedItem.Materials)
                {
                    Slot materialSlot = new Slot(new Vector2(0, i));// * Slot.DefaultHeight));
                    i += Slot.DefaultHeight;
                    //StaticObject mat = StaticObject.Create(pair.Key);

                    GameObject mat = GameObject.Objects[pair.Key];
                    GameObjectSlot invSlot = new GameObjectSlot();
                    invSlot.Object = mat;
                    mat.GetComponent<GuiComponent>("Gui").Properties["StackSize"] = pair.Value;
                    materialSlot.Tag = invSlot;
                    materialSlot.CustomTooltip = true;
                    materialSlot.DrawTooltip += new EventHandler<TooltipArgs>(materialSlot_DrawTooltip);
                    Panel_Info.Controls.Add(materialSlot);
                }
            }
            Controls.Add(Panel_Info);
        }

        void variation_DrawItem(object sender, DrawItemEventArgs e)
        {
            PictureBox box = sender as PictureBox;
            //if ((int)box.Tag == Variation)
            if ((Vector2)box.Tag == Variation)
                box.DrawHighlight(e.SpriteBatch, 0.5f);
            e.SpriteBatch.Draw(box.Texture, box.ScreenLocation, box.SourceRect, Color.White, 0, box.PictureOrigin, 1, SpriteEffects.None, 0);
        }

        Vector2 Variation;
        void variation_MouseLeftPress(object sender, EventArgs e)
        {
            Variation = (Vector2)(sender as PictureBox).Tag;
            Craft();
        }

        ObjectPlaceTool Tool;// = new ObjectSpawnTool();
        private void Craft()
        {
            GameObject product = GameObject.Objects[SelectedItem.ProductID];
            //if (product.GetInfo().Weight < 0)
      //      if (product["Physics"].GetProperty<float>("Weight") < 0)
      //      {
            GameObject obj = GameObject.Create(GameObject.Types.Blueprint);

                CraftableComponent craft = new CraftableComponent(BlueprintComponent.Blueprints[SelectedItem.ProductID]);

                craft.Variation = (int)Variation.Y;
                craft.Orientation = (int)Variation.X;
                obj.AddComponent("Activate", craft);

                Tool.Object = obj;
                Tool.Icon = obj.GetGui().GetProperty<Icon>("Icon");
                Rooms.Ingame.Instance.ToolManager.ActiveTool = Tool;
            //}
            //else
            //{
            //    InventoryComponent inv = Player.Actor.GetComponent<InventoryComponent>("Inventory");
            //    CraftableComponent craft = product.GetComponent<CraftableComponent>("Construction");
            //    List<InventorySlot> slots;
            //    int count;
            //    foreach (KeyValuePair<GameObject.Types, int> mat in craft.Materials)
            //    {
            //        if (!inv.Contains(mat.Key, out count, out slots))
            //        {
            //            NotificationArea.Write("Not enough materials");
            //            return;
            //        }
            //        else if (count < mat.Value)
            //        {
            //            NotificationArea.Write("Not enough materials");
            //            return;
            //        }
            //    }
            //    GameObject newProduct = StaticObject.Create(product.ID);
            //    inv.Give2(newProduct);
            //    newProduct.Components.Remove("Construction");
            //    foreach (KeyValuePair<GameObject.Types, int> mat in SelectedItem.Materials)
            //    {
            //        inv.Remove(mat.Key, mat.Value);
            //    }
            //}
            
        }

        void materialSlot_DrawTooltip(object sender, TooltipArgs e)
        {
            //InventorySlot slot = (e.Tooltip.Tag as Slot).Tag as InventorySlot;
            GameObjectSlot slot = (sender as Slot).Tag as GameObjectSlot;
            GameObject obj = slot.Object as GameObject;
            if (obj == null)
                return;
            GroupBox box = new GroupBox();
            string text = obj.Name;
            GuiComponent gui;
            if (obj.TryGetComponent<GuiComponent>("Gui", out gui))
            {
                if (gui.GetProperty<int>("StackSize") > 1)
                    text += " (" + gui.GetProperty<int>("StackSize") + ")";
            }
            Label label = new Label(text);
            box.Controls.Add(label);
            e.Tooltip.Controls.Add(box);
        }



        void build_Click(object sender, EventArgs e)
        {
            Craft();
        }

        //void list_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    Plan plan = sender as Plan;
        //    BackgroundStyle.Tooltip.Draw(e.SpriteBatch, e.Bounds);
        //    e.SpriteBatch.Draw(plan.Sprite, new Vector2(e.Bounds.X + (e.Bounds.Width - plan.SourceRect.Width) / 2, e.Bounds.Y + (e.Bounds.Height - plan.SourceRect.Height) / 2), plan.SourceRect, Color.White);
        //}

        public override void Dispose()
        {
            foreach (PictureBox picBox in Panel_Styles.Controls)
            {
                picBox.DrawItem -= variation_DrawItem;
                picBox.MouseLeftPress -= variation_MouseLeftPress;
            }
            //List.DrawItem -= list_DrawItem;
          //  List.DrawTooltip -= List_DrawTooltip;
           // List.SelectedValueChanged -= list_SelectedValueChanged;
            base.Dispose();
        }

        void List_ControlRemoved(object sender, EventArgs e)
        {
            ScrollableList list = sender as ScrollableList;
            //if (list.ClientSize.Height <= ClientSize.Height)
            if (list.ClientSize.Height <= Panel_list.ClientSize.Height)
            {
                list.Size = new Rectangle(0, 0, Panel_list.ClientSize.Width, list.Size.Height);
                Controls.Remove(Scroll);
            }
        }

        void List_ControlAdded(object sender, EventArgs e)
        {
            ScrollableList list = sender as ScrollableList;
            //if (list.ClientSize.Height > ClientSize.Height)
            if (list.ClientSize.Height > Panel_list.ClientSize.Height)
            {
                list.Size = new Rectangle(0, 0, Panel_list.ClientSize.Width - 16, list.Size.Height);
                Panel_list.Controls.Add(Scroll);
            }
        }

        void listEntry_DrawItem(object sender, DrawItemEventArgs e)
        {
            Label label = sender as Label;
            if (label.Tag == SelectedItem)
                label.DrawHighlight(e.SpriteBatch, 0.5f);
            //     e.SpriteBatch.Draw(e.
        }

        void listEntry_Click(object sender, EventArgs e)
        {
            SelectedItem = (sender as Label).Tag as Blueprint;
            InitInfoPanel(SelectedItem);//GameObject.Objects[SelectedItem.ProductID]);
        }
    }
}
