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
    class Construction3Window : Window
    {
        //static Construction3Window _Instance;
        //public static Construction3Window Instance
        //{
        //    get
        //    {
        //        if (_Instance == null)
        //            _Instance = new Construction3Window();
        //        return _Instance;
        //    }
        //}

        Panel Panel_image, Panel_description, Panel_properties, Panel_buttons, Panel_list, Panel_Help, Panel_Info, Panel_Styles;

        ScrollableList List;
        VScrollbar Scroll;

        static public Blueprint SelectedItem;

        public event EventHandler<BlueprintEventArgs> ConstructionSelected;
        void OnConstructionSelected(BlueprintEventArgs a)
        {
            if (ConstructionSelected != null)
                ConstructionSelected(this, a);
        }
        

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }
        
        //SortedDictionary<uint, Plan> Plans;
        List<Blueprint> Blueprints;
        GameObject Material;
        Vector3 Global;
        public GameObject Construction;
        int k;
        public Construction3Window(Vector3 global, GameObject construction = null, GameObject material = null)
        {
            Title = "Construction";
            Size = new Rectangle(0, 0, 200, 300);
            Tool = new ObjectPlaceTool();
            this.Global = global;
            this.Construction = construction;
            if (material == null)
                throw(new ArgumentNullException("material", "Material not set for Construction Window"));
            Material = material;
            Label materialText = new Label("Known blueprints for: " + material.Name);

            Panel_list = new Panel(new Vector2(0, materialText.Bottom), new Vector2(200, 200));
            
            Panel_image = new Panel(new Vector2(Panel_list.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));

            AutoSize = true;
            Movable = true;

            //Blueprints = BlueprintComponent.Blueprints.Values.ToList();
            BlueprintCollection bpCollection = BlueprintComponent.Blueprints;
            Blueprints = new List<Blueprint>(bpCollection.Values.ToList().FindAll(foo => foo[0].ContainsKey(material.ID)));


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

            Controls.Add(materialText, Panel_list);//, Panel_Info);
            this.AutoSize = false;
            //Location = new Vector2(Controller.Instance.msCurrent.X, Controller.Instance.msCurrent.Y) - ClientLocation - Panel_list.Location - Panel_list.Controls.First().Location - new Vector2(Label.DefaultHeight / 2);
            Location =UIManager.Mouse - ClientLocation - Panel_list.Location - Panel_list.Controls.First().Location - new Vector2(Label.DefaultHeight / 2);
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

        void variation_DrawItem(object sender, DrawItemEventArgs e)
        {
            PictureBox box = sender as PictureBox;
            //if ((int)box.Tag == Variation)
            if ((Vector2)box.Tag == Variation)
                box.DrawHighlight(e.SpriteBatch, 0.5f);
            e.SpriteBatch.Draw(box.Texture, box.ScreenLocation, box.SourceRect, Color.White, 0, box.PictureOrigin, 1, SpriteEffects.None, 0);
        }

        Vector2 Variation;


        ObjectPlaceTool Tool;// = new ObjectSpawnTool();


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



        public override void Dispose()
        {
            foreach (PictureBox picBox in Panel_Styles.Controls)
            {
                picBox.DrawItem -= variation_DrawItem;

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
            {
                label.DrawHighlight(e.SpriteBatch, 0.5f);
                return;
            }
           // if(label.MouseHover)
          //      label.DrawHighlight(e.SpriteBatch, 0.2f);
        }

        void listEntry_Click(object sender, EventArgs e)
        {
            SelectedItem = (sender as Label).Tag as Blueprint;




            GameObject product = GameObject.Create(SelectedItem.ProductID);// GameObject.Objects[SelectedItem.ProductID];
            SpriteComponent spriteComp = product.GetComponent<SpriteComponent>("Sprite");
            Sprite sprite = spriteComp.Sprite;
            bool hasOrientations = sprite.SourceRects.First().Length > 1;
           // Console.WriteLine(hasOrientations);
            if (!hasOrientations)
                AddConstruction();
            else
            {
                product.Global = Global;
                ObjectOrientationTool orientationControl = new ObjectOrientationTool(product);
                orientationControl.MouseLeft += new EventHandler<EventArgs>(orientationControl_MouseLeft);
                orientationControl.Removed += new EventHandler<EventArgs>(orientationControl_Removed);
                ToolManager.Instance.ActiveTool = orientationControl;
                //Log.Enqueue(Log.EntryTypes.Default, "Choose orientation");
            }
            Hide();
        }

        void orientationControl_Removed(object sender, EventArgs e)
        {
            ObjectOrientationTool tool = sender as ObjectOrientationTool;
            tool.Removed -= orientationControl_Removed;
            tool.MouseLeft -= orientationControl_MouseLeft;
        }

        void orientationControl_MouseLeft(object sender, EventArgs e)
        {
            ObjectOrientationTool tool = sender as ObjectOrientationTool;
            tool.Removed -= orientationControl_Removed;
            tool.MouseLeft -= orientationControl_MouseLeft;
            AddConstruction(tool.Variation, (int)tool.Orientation);
        }



        private void AddConstruction(int variation = 0, int orientation = 0)
        {
            OnConstructionSelected(new BlueprintEventArgs(SelectedItem, variation, orientation));
            return;
            //Map.RemoveObject(Material);
            GameObject obj = GameObjectDb.Construction;
            ConstructionOldComponent constr = obj.GetComponent<ConstructionOldComponent>("Construction");
            GameObject product = constr.SetBlueprint(SelectedItem);

            SpriteComponent spriteComp = (SpriteComponent)product["Sprite"];
            spriteComp["Variation"] = variation;
            spriteComp["Orientation"] = orientation;


         //   constr.Add(Material);

            if (Construction == null)
            {
                throw new NotImplementedException();
                //Material.Remove();
                Chunk.AddObject(obj, Engine.Map, Material.Global);
            }
        }
    }
}
