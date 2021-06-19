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
    class CraftingWindow : Window
    {
        static CraftingWindow _Instance;
        static public CraftingWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new CraftingWindow();
                return _Instance;
            }
        }
        Panel Panel_image, Panel_description, Panel_properties, Panel_buttons, Panel_Help, Panel_Info, Panel_Styles;


        
      //  Scrollbar Scroll;

        static public GameObjectSlot SelectedItem;

        public event EventHandler<BlueprintEventArgs> ConstructionSelected;
        void OnConstructionSelected(BlueprintEventArgs a)
        {
            if (ConstructionSelected != null)
                ConstructionSelected(this, a);
        }

        static public void Initialize(Vector3 global, GameObject construction = null, GameObject material = null)
        {
        }

        List<GameObjectSlot> Memorized;
        GameObject Workbench;
        public void Initialize(GameObject workbench)
        {
            Workbench = workbench;
            //List<GameObjectSlot> blueprints = (workbench["Container"]["Slots"] as List<GameObjectSlot>).ToList();
            //blueprints.Insert(0, Blueprint.NoBlueprint.ToSlot());
            //Panel_list.Build(blueprints.FindAll(foo => foo.HasValue).FindAll(foo => foo.Object.Type == ObjectType.Blueprint));//.Select(foo => foo.Object));

            Memorized = new List<GameObjectSlot>();
            if (Player.Actor != null)
            {
                MemoryCollection memorized = new MemoryCollection(Player.Actor["Memory"].GetProperty<MemoryCollection>("Memories").FindAll(obj => GameObject.Objects[obj.Object].Type == ObjectType.Blueprint).ToArray());
                this.Memorized.AddRange(memorized.ConvertAll<GameObjectSlot>(mem => GameObject.Objects[mem.Object].ToSlot()));
            }

           // Panel_list.Build(workbench["Crafting"]["BlueprintSlots"] as List<GameObjectSlot>);
            Panel_list.Build((workbench["Crafting"]["BlueprintSlots"] as List<GameObjectSlot>).Union(this.Memorized));
          //  Panel_list.Build(workbench["Crafting"]["BlueprintSlots"] as List<GameObjectSlot>);
            Panel_list.SelectedItem = Workbench["Crafting"]["Blueprint"] as GameObjectSlot; //).Object
            

            Location = UIManager.Mouse - ClientLocation - Panel_list.Location - Panel_list.Controls.First().Location - new Vector2(Label.DefaultHeight / 2);
        }

        
        public void Initialize(List<GameObjectSlot> blueprints)
        {
            blueprints.Insert(0, Blueprint.NoBlueprint.ToSlot());

            Panel_list.Build(blueprints.FindAll(foo => foo.HasValue).FindAll(foo => foo.Object.Type == ObjectType.Blueprint));//.Select(foo => foo.Object));
            //Panel_list.Build(blueprints.TakeWhile(foo=>foo.HasValue).Select(foo => foo.Object).TakeWhile(foo=>foo.Type == ObjectType.Blueprint));
        }

        protected override void OnHidden()
        {
            Workbench = null;
            base.OnHidden();
        }

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }
        PanelList<GameObjectSlot, SlotWithText> Panel_list;
        //SortedDictionary<uint, Plan> Plans;
        List<GameObject> Blueprints;
        GameObject Material;
        Vector3 Global;
       // public GameObject SelectedItem;
        int k;
        public CraftingWindow()
        {
            Title = "Crafting";
            Size = new Rectangle(0, 0, 200, 300);
            Tool = new ObjectPlaceTool();

          //  Panel_list = new Panel(Vector2.Zero, new Vector2(200, 200));
            Panel_list = new PanelList<GameObjectSlot, SlotWithText>(Vector2.Zero, new Vector2(200, 200), foo => foo.Object.Name); //new ScrollableList(Vector2.Zero, Panel_list.ClientSize);

            Blueprints = WorkbenchComponent.Blueprints;
            
          //  Blueprints.Insert(0, new GameObject(GameObject.Types.BlueprintBlank, "Clear blueprint", "").SetGui());
         //   Panel_list.Build(Blueprints);
            Panel_list.SelectedItemChanged += new EventHandler<EventArgs>(Panel_list_SelectedItemChanged);
            Panel_list.ItemRightClick += new EventHandler<ListItemEventArgs<GameObjectSlot>>(Panel_list_ItemRightClick);

            Panel_image = new Panel(new Vector2(Panel_list.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));

            AutoSize = true;
            Movable = true;
           
            Panel_Styles = new Panel(new Vector2(0, Panel_list.Bottom), new Vector2(Panel_list.Width, 100));
            Panel_Styles.AutoSize = true;

            Panel_Info = new Panel(new Vector2(0, Panel_list.Bottom), new Vector2(Panel_list.Height, Panel_list.Width));


            Panel_buttons = new Panel(new Vector2(0, Panel_Info.Bottom), new Vector2(Panel_Info.Width, 23));

            Controls.Add(Panel_list);
            this.AutoSize = false;
            //Location = new Vector2(Controller.Instance.msCurrent.X, Controller.Instance.msCurrent.Y) - ClientLocation - Panel_list.Location - Panel_list.Controls.First().Location - new Vector2(Label.DefaultHeight / 2);
            Location =UIManager.Mouse - ClientLocation - Panel_list.Location - Panel_list.Controls.First().Location - new Vector2(Label.DefaultHeight / 2);
        }

        void Panel_list_ItemRightClick(object sender, ListItemEventArgs<GameObjectSlot> e)
        {
            if(Workbench==null)
                return;
            GameObject obj = e.Item.Object;
            if (obj.ID == GameObject.Types.BlueprintBlank)
                return;
            Player.Actor.HandleMessage(Message.Types.Begin, null, Workbench, Message.Types.Retrieve, e.Item);
            Initialize(Workbench);
            //if (Workbench.GetComponent<ContainerComponent>("Container").Remove(obj).Clear())
            //{
            //    WorkbenchComponent.PopLoot(Workbench, obj);
            //    Initialize(Workbench);
            //}
        }

        void Panel_list_SelectedItemChanged(object sender, EventArgs e)
        {
            SelectedItem = Panel_list.SelectedItem;
            OnSelectedItemChanged();
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

        public override bool Close()
        {
            return Hide();
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

        //void listEntry_Click(object sender, EventArgs e)
        //{
        //    SelectedItem = (sender as Label).Tag as Blueprint;




        //    GameObject product = GameObject.Create(SelectedItem.ProductID);// GameObject.Objects[SelectedItem.ProductID];
        //    SpriteComponent spriteComp = product.GetComponent<SpriteComponent>("Sprite");
        //    Sprite sprite = spriteComp.Sprite;
        //    bool hasOrientations = sprite.SourceRect.First().Length > 1;
        //   // Console.WriteLine(hasOrientations);
        //    if (!hasOrientations)
        //        AddConstruction();
        //    else
        //    {
        //        product.GetPosition()["Position"] = new Position(Global);
        //        ObjectOrientationTool orientationControl = new ObjectOrientationTool(product);
        //        orientationControl.MouseLeft += new EventHandler<EventArgs>(orientationControl_MouseLeft);
        //        orientationControl.Removed += new EventHandler<EventArgs>(orientationControl_Removed);
        //        ToolManager.Instance.ActiveTool = orientationControl;
        //        //Log.Enqueue(Log.EntryTypes.Default, "Choose orientation");
        //    }
        //    Hide();
        //}

        //void orientationControl_Removed(object sender, EventArgs e)
        //{
        //    ObjectOrientationTool tool = sender as ObjectOrientationTool;
        //    tool.Removed -= orientationControl_Removed;
        //    tool.MouseLeft -= orientationControl_MouseLeft;
        //}

        //void orientationControl_MouseLeft(object sender, EventArgs e)
        //{
        //    ObjectOrientationTool tool = sender as ObjectOrientationTool;
        //    tool.Removed -= orientationControl_Removed;
        //    tool.MouseLeft -= orientationControl_MouseLeft;
        //    AddConstruction(tool.Variation, (int)tool.Orientation);
        //}



        //private void AddConstruction(int variation = 0, int orientation = 0)
        //{
        //    OnConstructionSelected(new BlueprintEventArgs(SelectedItem, variation, orientation));
        //    return;
        //    //Map.RemoveObject(Material);
        //    GameObject obj = GameObjectDb.Construction;
        //    ConstructionComponent constr = obj.GetComponent<ConstructionComponent>("Construction");
        //    GameObject product = constr.SetBlueprint(SelectedItem);

        //    SpriteComponent spriteComp = (SpriteComponent)product["Sprite"];
        //    spriteComp["Variation"] = variation;
        //    spriteComp["Orientation"] = orientation;
        //   // OnConstructionSelected();
        //    constr.Add(Material);

        //    if (Construction == null)
        //    {
        //        Map.RemoveObject(Material);
        //        Chunk.AddObject(obj, Material.Global);
        //    }
        //}

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
    }
}
