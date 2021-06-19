using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using Start_a_Town_.Control;
using Start_a_Town_.Crafting;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class Crafting2Window : Window
    {
        Panel Panel_image, Panel_description, Panel_properties, Panel_buttons, Panel_list, Panel_Help, Panel_Info, Panel_Styles;
        //PictureBox Image;
        //Label Properties, Description;

        static Crafting2Window _Instance;
        static public Crafting2Window Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Crafting2Window();
                return _Instance;
            }
        }

        //List<Wall> Walls = new List<Wall>();
        ListBox List;
        
        //static public Plan SelectedItem;
        static public Blueprint SelectedItem;
        static public GameObject Material;
        //static public GameObject PlaceableObject;

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }
        
        List<Blueprint> Blueprints;

        int k;
        public Crafting2Window(GameObject material = null)
        {
            if (material == null)
                return;
            Label materialText = new Label("Known blueprints for: " + material.Name);
            Title = "Crafting";
            Tool = new ObjectPlaceTool();
            Panel_list = new Panel(new Vector2(0, materialText.Bottom), new Vector2(200, 200));
            
            Panel_image = new Panel(new Vector2(Panel_list.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));

            AutoSize = true;
            Movable = true;


            

            BlueprintCollection bpCollection = BlueprintComponent.Blueprints;
            //Blueprints = new List<Blueprint>(bpCollection.Values.TakeWhile(foo => foo[0].ContainsKey(material.ID)));
            Blueprints = new List<Blueprint>(bpCollection.Values.ToList().FindAll(foo => foo[0].ContainsKey(material.ID)));
            //BlueprintStage firstStage = 
            //Blueprints = BlueprintComponent.Blueprints.TakeWhile(

          //  k = 0;

          //  SelectedItem = Blueprints[k];

            Panel_list.AutoSize = true;

            List = new ListBox(new Vector2(0, materialText.Bottom), Panel_list.ClientSize);
            List.CustomTooltip = true;
            List.SelectedValueChanged += new EventHandler<EventArgs>(list_SelectedValueChanged);
            List.DrawTooltip += new EventHandler<TooltipArgs>(List_DrawTooltip);
            List.DisplayMember = "Name";
            List.ValueMember = "ProductID";

            foreach (Blueprint bp in Blueprints)
                List.Items.Add(bp);

            List.Build();
            Panel_list.Controls.Add(List);

            Panel_Styles = new Panel(new Vector2(0, Panel_list.Bottom), new Vector2(Panel_list.Width, 100));
            Panel_Styles.AutoSize = true;

            Panel_Info = new Panel(new Vector2(0, Panel_list.Bottom), new Vector2(200, 150));

            Panel_buttons = new Panel(new Vector2(0, Panel_Info.Bottom), new Vector2(Panel_Info.Width, 23));
            Button build = new Button(new Vector2(0), Panel_buttons.ClientSize.Width, "Build!");
            Panel_buttons.AutoSize = true;
            Panel_buttons.Controls.Add(build);

            

            Controls.Add(materialText, Panel_list, Panel_buttons, Panel_Info);
            AutoSize = false;
            Location = BottomLeftScreen;
            build.Click += new UIEvent(build_Click);
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
            //Label materials = new Label(bp.Materials);
            int i = 0;
            //foreach (KeyValuePair<GameObject.Types, int> pair in bp.Materials)
            foreach (BlueprintStage stage in SelectedItem.Stages)
                foreach (KeyValuePair<GameObject.Types, int> pair in stage)
            {
                //text += "\n" + StaticObject.Objects[pair.Key].Name + " x" + pair.Value;
                Slot slot = new Slot(new Vector2(0, name.Bottom + i * Slot.DefaultHeight));
                GameObject mat = GameObject.Create(pair.Key);
                GameObjectSlot invSlot = new GameObjectSlot();
                invSlot.Object = mat;
                mat.GetComponent<GuiComponent>("Gui").Properties["StackSize"] = pair.Value;
                slot.Tag = invSlot;
                box.Controls.Add(slot);
            }
            
            //box.Controls.Add(new Label(text));


            //box.Controls.Add(name);
            e.Tooltip.Controls.Add(box);
        }

        void list_SelectedValueChanged(object sender, EventArgs e)
        {
            //SelectedItem = Plans[(uint)List.SelectedValue];
            //SelectedItem = Blueprints[(int)List.SelectedValue];
            SelectedItem = Blueprints.Find(foo => foo.ProductID == (GameObject.Types)List.SelectedValue);

            foreach(Control control in Panel_Info.Controls)
                if(control is Slot)
                    control.DrawTooltip -= materialSlot_DrawTooltip;
            Panel_Info.Controls.Clear();



            GameObject obj = GameObject.Objects[SelectedItem.ProductID];
            Sprite sprite = obj.GetComponent<SpriteComponent>("Sprite").Sprite;
            Variation = Vector2.Zero;
            int k = 0, n = 0;
            foreach (PictureBox picBox in Panel_Styles.Controls)
            {
                picBox.DrawItem -= variation_DrawItem;
                picBox.MouseLeftPress -= variation_MouseLeftPress;
            }
            Panel_Styles.Controls.Clear();
            foreach (Rectangle[] strip in sprite.SourceRect)
            {
                foreach (Rectangle rect in strip)
                {
                    PictureBox variation = new PictureBox(new Vector2(k * rect.Width, n * rect.Height), sprite.Texture, rect, HorizontalAlignment.Left);
                    variation.Tag = new Vector2(k, n);
                    variation.MouseLeftPress += new EventHandler<InputState>(variation_MouseLeftPress);
                    variation.DrawMode = UI.DrawMode.OwnerDrawVariable;
                    variation.DrawItem += new EventHandler<DrawItemEventArgs>(variation_DrawItem);
                    Panel_Styles.Controls.Add(variation);
                    k += 1;// variation.Width;
                }
                n += 1;
                k = 0;
            }

            Controls.Add(Panel_Styles);
            Panel_Styles.Location = new Vector2(Panel_list.Right, List.Controls[List.SelectedIndex].Top);

            string text = UIManager.WrapText(obj.GetInfo().ToString(), Panel_Info.ClientSize.Width);
            Label label = new Label(text + "\nMaterials:");
            Panel_Info.Controls.Add(label);

            int i = 0;
            //foreach (KeyValuePair<GameObject.Types, int> pair in SelectedItem.Materials)
            foreach (BlueprintStage stage in SelectedItem.Stages)
                foreach (KeyValuePair<GameObject.Types, int> pair in stage)
            {
                Slot materialSlot = new Slot(new Vector2(0, label.Bottom + i * Slot.DefaultHeight));
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
            if(product["Physics"].GetProperty<float>("Weight") < 0)
            {
                GameObject obj = GameObject.Create(GameObject.Types.Blueprint);

                //CraftableComponent craft = product.GetComponent<CraftableComponent>("Construction").Clone() as CraftableComponent;
                CraftableComponent craft = new CraftableComponent(WorkbenchComponent.Blueprints[SelectedItem.ProductID]);
               // craft.ProductID = SelectedItem.ProductID;
                craft.Variation = (int)Variation.Y;
                craft.Orientation = (int)Variation.X;
                obj.AddComponent("Construction", craft);
                //Position playerPosition = Player.Actor.GetComponent<MovementComponent>("Position").CurrentPosition;
                //DragDropManager.Instance.Item = obj;
                //DragDropManager.Instance.Sprite = obj.GetComponent<SpriteComponent>("Sprite").Sprite.Texture;
                Tool.Object = obj;
                Tool.Icon = obj.GetGui().GetProperty<Icon>("Icon");// StaticObject.Objects[Tool.Type].GetGui().Icon;
                Rooms.Ingame.Instance.ToolManager.ActiveTool = Tool;
            }
            else
            {
                InventoryComponent inv = Player.Actor.GetComponent<InventoryComponent>("Inventory");
                CraftableComponent craft = new CraftableComponent(WorkbenchComponent.Blueprints[SelectedItem.ProductID]); // product.GetComponent<CraftableComponent>("Construction");
                List<GameObjectSlot> slots;
                int count;
                Dictionary<GameObject.Types, int> reqMats = craft.GetProperty<Blueprint>("Blueprint").Stages[(int)craft[Stat.Stage.Name]];
                foreach (KeyValuePair<GameObject.Types, int> mat in reqMats)
                {
                    if (!inv.Contains(mat.Key, out count, out slots))
                    {
                        NotificationArea.Write("Not enough materials");
                        return;
                    }
                    else if (count < mat.Value)
                    {
                        NotificationArea.Write("Not enough materials");
                        return;
                    }
                }
                GameObject newProduct = GameObject.Create(product.ID);
                inv.Give(newProduct);
                newProduct.Components.Remove("Construction");

                // TODO: something not right here
                foreach (BlueprintStage stage in SelectedItem.Stages)
                    foreach (KeyValuePair<GameObject.Types, int> mat in stage)
                        inv.Remove(mat.Key, mat.Value);
                //foreach (KeyValuePair<GameObject.Types, int> mat in SelectedItem.Materials)
                //{
                //    inv.Remove(mat.Key, mat.Value);
                //}
            }
            
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
            List.DrawTooltip -= List_DrawTooltip;
            List.SelectedValueChanged -= list_SelectedValueChanged;
            base.Dispose();
        }
    }
}
