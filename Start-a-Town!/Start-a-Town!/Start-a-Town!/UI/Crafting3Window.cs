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

        static public GameObjectSlot SelectedItem;

        public event EventHandler<BlueprintEventArgs> ConstructionSelected;
        void OnConstructionSelected(BlueprintEventArgs a)
        {
            if (ConstructionSelected != null)
                ConstructionSelected(this, a);
        }

        List<GameObjectSlot> Memorized;
        GameObject Workbench;
        public void Initialize(GameObject workbench)
        {
            Workbench = workbench;

            Memorized = new List<GameObjectSlot>();
            if (Player.Actor != null)
            {
                MemoryCollection memorized = new MemoryCollection(Player.Actor["Memory"].GetProperty<MemoryCollection>("Memories").FindAll(obj => GameObject.Objects[obj.Object].Type == ObjectType.Blueprint).ToArray());
                this.Memorized.AddRange(memorized.ConvertAll<GameObjectSlot>(mem => GameObject.Objects[mem.Object].ToSlot()));
            }

            Panel_blueprints.Build((workbench["Crafting"]["BlueprintSlots"] as List<GameObjectSlot>).Union(this.Memorized));
            Panel_blueprints.SelectedItem = Workbench["Crafting"]["Blueprint"] as GameObjectSlot; //).Object

            Location = UIManager.Mouse - ClientLocation - Panel_blueprints.Location - Panel_blueprints.Controls.First().Location - new Vector2(Label.DefaultHeight / 2);
        }

        
        public void Initialize(List<GameObjectSlot> blueprints)
        {
            Panel_blueprints.Build(blueprints.FindAll(foo => foo.HasValue).FindAll(foo => foo.Object.Type == ObjectType.Blueprint));
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
        PanelList<GameObjectSlot, SlotWithText> Panel_blueprints;
        List<GameObject> Blueprints;
        GameObject Material;
        Vector3 Global;

        public CraftingWindow()
        {
            Title = "Crafting";
            Size = new Rectangle(0, 0, 200, 300);
            AutoSize = true;
            Movable = true;
           
            Panel_blueprints = new PanelList<GameObjectSlot, SlotWithText>(Vector2.Zero, new Vector2(200, 200), foo => foo.Object.Name); //new ScrollableList(Vector2.Zero, Panel_list.ClientSize);

            Blueprints = WorkbenchComponent.Blueprints;

            Panel_blueprints.SelectedItemChanged += new EventHandler<EventArgs>(Panel_list_SelectedItemChanged);
            Panel_blueprints.ItemRightClick += new EventHandler<ListItemEventArgs<GameObjectSlot>>(Panel_list_ItemRightClick);

            Panel_image = new Panel(new Vector2(Panel_blueprints.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));

           
            Panel_Styles = new Panel(new Vector2(0, Panel_blueprints.Bottom), new Vector2(Panel_blueprints.Width, 100));
            Panel_Styles.AutoSize = true;

            Panel_Info = new Panel(new Vector2(0, Panel_blueprints.Bottom), new Vector2(Panel_blueprints.Height, Panel_blueprints.Width));

            Panel_buttons = new Panel(new Vector2(0, Panel_blueprints.Bottom), new Vector2(Panel_Info.Width, 23)) { AutoSize = true };

            Button btn_clear = new Button(Vector2.Zero, (int)Panel_Info.ClientDimensions.Y, "Clear")
            {
                LeftClickAction = () =>
                {
                    //Player.Actor.PostMessage(Message.Types.BeginInteraction, null, 
                    Workbench.PostMessage(Message.Types.SetBlueprint, Player.Actor, GameObjectSlot.Empty);
                }
            };
            Panel_buttons.Controls.Add(btn_clear);

            Client.Controls.Add(Panel_blueprints, Panel_buttons);
            this.AutoSize = false;
            Location =UIManager.Mouse - ClientLocation - Panel_blueprints.Location - Panel_blueprints.Controls.First().Location - new Vector2(Label.DefaultHeight / 2);
        }

        void Panel_list_ItemRightClick(object sender, ListItemEventArgs<GameObjectSlot> e)
        {
            if(Workbench==null)
                return;
            GameObject obj = e.Item.Object;
            //if (obj.ID == GameObject.Types.BlueprintBlank)
            //    return;
            Player.Actor.PostMessage(Message.Types.Begin, null, Workbench, Message.Types.Retrieve, e.Item);
            Initialize(Workbench);
            //if (Workbench.GetComponent<ContainerComponent>("Container").Remove(obj).Clear())
            //{
            //    WorkbenchComponent.PopLoot(Workbench, obj);
            //    Initialize(Workbench);
            //}
        }

        void Panel_list_SelectedItemChanged(object sender, EventArgs e)
        {
            SelectedItem = Panel_blueprints.SelectedItem;
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
