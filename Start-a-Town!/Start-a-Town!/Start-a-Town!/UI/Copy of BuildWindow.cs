using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using Start_a_Town_.Control;

using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class BuildWindow : Window
    {
        static BuildWindow _Instance;
        public static BuildWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new BuildWindow();
                return _Instance;
            }
        }

        public override bool Close()
        {
            return Hide();
        }
        public override bool Show(params object[] p)
        {
            Refresh(Player.Actor["Inventory"]["Holding"] as GameObjectSlot);
            return base.Show(p);
        }
        public override bool Toggle()
        {
            Refresh(Player.Actor["Inventory"]["Holding"] as GameObjectSlot);
            return base.Toggle();
        }

        Panel Panel_image, Panel_description, Panel_properties, Panel_buttons, Panel_Help, Panel_Variations, Panel_Filter;//, Panel_Hauling;
        PanelList<GameObject, Button> Panel_List;
        Label Lbl_Available;
        CheckBox Chk_Filter;
        Button Btn_Build;
        SlotWithText Slot_Hauling;
        //PanelList<GameObject> Panel_List;
       // ListBox List;

        static public GameObject SelectedItem;

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }

        List<GameObject> List;
        GameObjectCollection Objects;

     //   int k;
        BuildWindow()
        {
            Title = "Build";
            Size = new Rectangle(0, 0, 200, 300);

            

            Label lbl_haul = new Label(Vector2.Zero, "Hauling:");
            Slot_Hauling = new SlotWithText(lbl_haul.BottomLeft);
            //Panel_Hauling = new Panel(lbl_haul.BottomLeft);
            //Panel_Hauling.AutoSize = true;
            //Panel_Hauling.Controls.Add(Slot_Hauling);
            //Panel_Hauling.Width = Size.Width;

            //Panel_list = new Panel(Vector2.Zero, new Vector2(Size.Width, 300));
            Lbl_Available = new Label(Slot_Hauling.BottomLeft, "Available Constructions:");
            Panel_List = new PanelList<GameObject, Button>(Lbl_Available.BottomLeft, new Vector2(Size.Width, 300), foo => foo.Name);
            //Panel_List = new PanelList<GameObject>(Vector2.Zero, new Vector2(Size.Width, 300), foo => foo.Name);
            Panel_List.SelectedItemChanged += new EventHandler<EventArgs>(Panel_List_SelectedItemChanged);

            Panel_image = new Panel(new Vector2(Panel_List.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));
            Panel_Filter = new Panel(Panel_List.BottomLeft, new Vector2(Panel_List.Width, 1));

            Panel_Filter.AutoSize = true;
            Chk_Filter = new CheckBox("Filter", Vector2.Zero);
            Chk_Filter.Checked = true;
            Chk_Filter.Click += new UIEvent(Chk_Filter_Click);
            Panel_Filter.Controls.Add(Chk_Filter);

            AutoSize = true;
            Movable = true;

            Btn_Build = new Button(Vector2.Zero, "Craft");
            Btn_Build.Click += new UIEvent(Btn_Build_Click);

            //Panel_List.Build(GameObject.Objects.Values.OrderBy(x=>x.Name).ToList());
            //List<GameObject> planList = WorkbenchComponent.Blueprints.FindAll(bp => bp.Type == ObjectType.Plan);
            //List = planList;//.Select(foo => GameObject.Objects[(foo["Blueprint"]["Blueprint"] as Blueprint).ProductID]).ToList();
            //Panel_List.Build(List);
            Panel_Variations = new Panel(new Vector2(0, Panel_List.Bottom), new Vector2(Panel_List.Width, 100));
            Panel_Variations.AutoSize = true;

            Controls.Add(lbl_haul, Slot_Hauling, Lbl_Available, Panel_List, Panel_Filter);//, ScrollBar);//, Panel_buttons });
            
            AutoSize = false;
            Anchor = new Vector2(0.1f, 0.2f);
            this.Location = Anchor * UIManager.Size;
            //this.Location = new Vector2((int)Math.Floor(CenterScreen.X / 2), (int)Math.Floor(CenterScreen.Y / 2));
            //Location = BottomLeftScreen;
          //  build.Click += new UIEvent(build_Click);

            PlaceTool = new ConstructionTool();
            PlaceTool.MouseLeft += new EventHandler<EventArgs>(PlaceTool_MouseLeft);
            OrientationTool = new ObjectOrientationTool2();
            OrientationTool.MouseLeft += new EventHandler<EventArgs>(OrientationTool_MouseLeft);

            Game1.TextInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(TextInput_KeyPress);

        }

        void Btn_Build_Click(object sender, EventArgs e)
        {
            Blueprint bp = SelectedItem["Blueprint"]["Blueprint"] as Blueprint;
            Player.Actor.HandleMessage(Message.Types.BeginInteraction, null, new Interaction(timespan: new TimeSpan(0, 0, 1), message: Message.Types.CraftObject, source: Player.Actor, verb: "Crafting"), bp);
        }
        //void Btn_Build_Click(object sender, EventArgs e)
        //{
        //  //  bool ok = false;
        //    Blueprint bp = SelectedItem["Blueprint"]["Blueprint"] as Blueprint;
        //    foreach (KeyValuePair<GameObject.Types, int> mat in bp[0])
        //        if (InventoryComponent.GetAmount(Player.Actor, obj => obj.ID == mat.Key) < mat.Value)
        //            return;
        //    foreach (KeyValuePair<GameObject.Types, int> mat in bp[0])
        //        InventoryComponent.RemoveObjects(Player.Actor, obj => obj.ID == mat.Key, mat.Value);
        //    Player.Actor.HandleMessage(Message.Types.Give, null, GameObject.Create(bp.ProductID).ToSlot());
        //    //Player.Actor.HandleMessage(Message.Types.BeginInteraction, null, new Interaction(timespan: new TimeSpan(0, 0, 1), message: Message.Types.Give, source: Player.Actor, verb: "Crafting"), GameObject.Create(bp.ProductID).ToSlot());
        //}

        void Chk_Filter_Click(object sender, EventArgs e)
        {
            Refresh(Player.Actor["Inventory"]["Holding"] as GameObjectSlot);
        }

        void TextInput_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case 'q':
                    if (PlaceTool.Object != null)
                        SpriteComponent.ChangeOrientation(PlaceTool.Object);
                    break;
                default:
                    break;
            }
        }

        GameObject LastHauling;
        public override void Update()
        {
            //if (PlaceTool.Object != null)
            //{
            //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.Q))
            //        SpriteComponent.ChangeOrientation(PlaceTool.Object);
            //       // PlaceTool.Object.HandleMessage(Message.Types.ChangeOrientation, Player.Actor);
            //}

            GameObjectSlot haulSlot = Player.Actor["Inventory"]["Holding"] as GameObjectSlot;
            if (haulSlot.Object != LastHauling)
                Refresh(haulSlot);
            LastHauling = haulSlot.Object;
            base.Update();
        }

        public void Refresh(GameObjectSlot haulSlot)
        {
            //List<GameObjectSlot> Memorized = new List<GameObjectSlot>();
            //if (Player.Actor != null)
            //{
            //    MemoryCollection memorized = new MemoryCollection(Player.Actor["Memory"].GetProperty<MemoryCollection>("Memories").FindAll(obj => GameObject.Objects[obj.Object].Type == ObjectType.Blueprint).ToArray());
            //    Memorized.AddRange(memorized.ConvertAll<GameObjectSlot>(mem => GameObject.Objects[mem.Object].ToSlot()));
            //}
            List<GameObject> Memorized = new List<GameObject>();
            if (Player.Actor != null)
            {
                MemoryCollection memorized = new MemoryCollection(Player.Actor["Memory"].GetProperty<MemoryCollection>("Memories").FindAll(obj => GameObject.Objects[obj.Object].Type == ObjectType.Blueprint).ToArray());
                Memorized.AddRange(memorized.ConvertAll<GameObject>(mem => GameObject.Objects[mem.Object]));
            }


            Controls.Remove(Panel_Variations);

            if (Chk_Filter.Checked)
            {
                Slot_Hauling.SetTag(haulSlot);
                List<GameObject> planList = WorkbenchComponent.Blueprints.FindAll(bp => bp.Type == ObjectType.Plan);
                if (haulSlot.HasValue)
                    List = planList.FindAll(bp => (bp["Blueprint"]["Blueprint"] as Blueprint)[0].ContainsKey(haulSlot.Object.ID));
                else
                    List = new List<GameObject>();// planList;
            }
            else
            {
                List = WorkbenchComponent.Blueprints.Skip(1).ToList();
            }
            Panel_List.Build(List);
            Panel_List.Location = Lbl_Available.BottomLeft;
            ToolManager.Instance.ActiveTool = null;
        }

        void OrientationTool_MouseLeft(object sender, EventArgs e)
        {
            AddConstruction(OrientationTool.Variation, (int)OrientationTool.Orientation);
        }

        void AddConstruction(int variation = 0, int orientation = 0)
        {
            Blueprint bp = SelectedItem["Blueprint"]["Blueprint"] as Blueprint;
            GameObjectSlot haulSlot = Player.Actor["Inventory"]["Holding"] as GameObjectSlot;
            if (!haulSlot.HasValue)
                return;
            if (!bp[0].ContainsKey(haulSlot.Object.ID))
                return;

            //  haulSlot.StackSize--;
            GameObject obj = GameObjectDb.Construction;
            // ConstructionComponent constr = obj.GetComponent<ConstructionComponent>("Construction");

            obj.HandleMessage(Message.Types.SetBlueprint, Player.Actor, SelectedItem["Blueprint"]["Blueprint"] as Blueprint, variation, orientation);
            obj.HandleMessage(Message.Types.DropOn, Player.Actor);
            Chunk.AddObject(obj, Engine.Map, PlaceTool.Global);

            Rooms.Ingame.Instance.ToolManager.ActiveTool = null;
        }

        void PlaceTool_MouseLeft(object sender, EventArgs e)
        {
            Vector3 d = (Player.Actor.Global - PlaceTool.Global);
            float l = d.Length();
            if (l > 2)
                return;

            //SpriteComponent spriteComp = PlaceTool.Object.GetComponent<SpriteComponent>("Sprite");
            //Sprite sprite = spriteComp.Sprite;
            //bool hasOrientations = sprite.SourceRect.First().Length > 1;
            //if (hasOrientations)
            //{
            //    OrientationTool.Global = PlaceTool.Global;
            //    OrientationTool.Object = PlaceTool.Object;
            //    Rooms.Ingame.Instance.ToolManager.ActiveTool = OrientationTool;
            //}


            //ToolManager.Instance.ActiveTool = new InteractionTool(PlaceTool.TargetObject, Message.Types.Construct, System.Windows.Forms.Keys.LButton);
            //AddConstruction((int)PlaceTool.Object["Sprite"]["Variation"], (int)PlaceTool.Object["Sprite"]["Orientation"]);
            return;
        }

        void Panel_list_DrawItem(object sender, DrawItemEventArgs e)
        {
            Panel panel = sender as Panel;
            foreach (Label label in panel.Controls)
            {
                Rectangle finalRect;
                Rectangle labelRect = label.Bounds;
                Rectangle panelRect = panel.Bounds;//Bounds;
                Rectangle.Intersect(ref labelRect, ref panelRect, out finalRect);
                //  panel.DrawHighlight(e.SpriteBatch, panel.ScreenClientRectangle, 0.5f);
                if (label.Tag == SelectedItem)
                    label.DrawHighlight(e.SpriteBatch, finalRect, 0.5f);
                //e.SpriteBatch.Draw(label.TextSprite, finalRect, Color.Lerp(Color.Transparent, Color.White, label.Opacity));
                Rectangle source = new Rectangle(0, finalRect.Y - labelRect.Y, finalRect.Width, finalRect.Height);
                e.SpriteBatch.Draw(label.TextSprite, new Vector2(finalRect.X, finalRect.Y), source, Color.Lerp(Color.Transparent, Color.White, label.Opacity));
                //  panel.ScreenClientRectangle
            }
        }

        void listEntry_DrawItem(object sender, DrawItemEventArgs e)
        {
            Label label = sender as Label;
            if (label.Tag == SelectedItem)
                label.DrawHighlight(e.SpriteBatch, 0.5f);
            //     e.SpriteBatch.Draw(e.
        }

        void Panel_List_SelectedItemChanged(object sender, EventArgs e)
        {
            SelectedItem = Panel_List.SelectedItem;
          //  InitVariationPanel(SelectedItem);
            Blueprint bp = SelectedItem["Blueprint"]["Blueprint"] as Blueprint;
            InitVariationPanel(GameObject.Objects[bp.ProductID]);
            Select();
        }

        void InitVariationPanel(GameObject obj)
        {

          //  Sprite sprite = obj.GetComponent<SpriteComponent>("Sprite").Sprite;
            Sprite sprite = (Sprite)obj["Sprite"]["Sprite"];
            Variation = Vector2.Zero;
            int k = 0, n = 0;
            foreach (PictureBox picBox in Panel_Variations.Controls.FindAll(foo=>foo is PictureBox))
            {
                picBox.DrawItem -= variation_DrawItem;
             //   picBox.MouseLeftPress -= variation_MouseLeftPress;
                picBox.DrawTooltip -= variation_DrawTooltip;
            }
            Panel_Variations.Controls.Clear();
            //foreach (Rectangle[] strip in sprite.SourceRect)
            //{
            //    foreach (Rectangle rect in strip)
            //    {
            Rectangle rect = sprite.SourceRect[0][0];
                    PictureBox variation = new PictureBox(new Vector2(k * rect.Width, n * rect.Height), sprite.Texture, rect, HorizontalAlignment.Left);
                    variation.Tag = new Vector2(k, n);
                 //   variation.MouseLeftPress += new EventHandler<InputState>(variation_MouseLeftPress);
                    variation.DrawMode = UI.DrawMode.OwnerDrawVariable;
                    variation.DrawItem += new EventHandler<DrawItemEventArgs>(variation_DrawItem);
                    variation.CustomTooltip = true;
                    variation.DrawTooltip += new EventHandler<TooltipArgs>(variation_DrawTooltip);
                    Panel_Variations.Controls.Add(variation);

                    if (SelectedItem.Type == ObjectType.Blueprint)
                    {
                        Btn_Build.Location = variation.BottomLeft;
                        Panel_Variations.Controls.Add(Btn_Build);
                    }
            //        k += 1;
            //    }
            //    n += 1;
            //    k = 0;
            //}

            Controls.Add(Panel_Variations);

            //Panel_Variations.Location = new Vector2(ClientSize.Width, Panel_List.List.Controls[Panel_List.List.Controls.FindIndex(foo => foo.Tag == SelectedItem)].Top + (int)Panel_List.List.ClientLocation.Y);
            //Panel_Variations.Location.Y -= Math.Max(0, Panel_Variations.ScreenLocation.Y + Panel_Variations.Height - WindowManager.ScreenHeight);//Game1.Instance.graphics.PreferredBackBufferHeight);
            Panel_Variations.Location = new Vector2(ClientSize.Width, Panel_List.List.Controls[Panel_List.List.Controls.FindIndex(foo => foo.Tag == SelectedItem)].Top + (int)Panel_List.List.ClientLocation.Y + (int)Panel_List.Location.Y);
            Panel_Variations.Location.Y -= Math.Max(0, Panel_Variations.ScreenLocation.Y + Panel_Variations.Height - WindowManager.ScreenHeight);//Game1.Instance.graphics.PreferredBackBufferHeight);
        }

        void variation_DrawTooltip(object sender, TooltipArgs e)
        {
            //Blueprint bp = SelectedItem["Blueprint"]["Blueprint"] as Blueprint;
            //GameObject.Objects[bp.ProductID].GetTooltip(e.Tooltip);
            SelectedItem.GetTooltip(e.Tooltip);
        }

        //void list_SelectedValueChanged(object sender, EventArgs e)
        //{
        //    SelectedItem = Objects[(GameObject.Types)List.SelectedValue];

        //    GameObject obj = SelectedItem;
        //    Sprite sprite = obj.GetComponent<SpriteComponent>("Sprite").Sprite;
        //    Variation = Vector2.Zero;
        //    int k = 0, n = 0;
        //    foreach (PictureBox picBox in Panel_Variations.Controls)
        //    {
        //        picBox.DrawItem -= variation_DrawItem;
        //        picBox.MouseLeftPress -= variation_MouseLeftPress;
        //    }
        //    Panel_Variations.Controls.Clear();
        //    foreach (Rectangle[] strip in sprite.SourceRect)
        //    {
        //        foreach (Rectangle rect in strip)
        //        {
        //            PictureBox variation = new PictureBox(new Vector2(k * rect.Width, n * rect.Height), sprite.Texture, rect, TextAlignment.Left);
        //            variation.Tag = new Vector2(k, n);
        //            variation.MouseLeftPress += new EventHandler<InputState>(variation_MouseLeftPress);
        //            variation.DrawMode = UI.DrawMode.OwnerDrawVariable;
        //            variation.DrawItem += new EventHandler<DrawItemEventArgs>(variation_DrawItem);
        //            Panel_Variations.Controls.Add(variation);
        //            k += 1;// variation.Width;
        //        }
        //        n += 1;
        //        k = 0;
        //    }
            
        //    Controls.Add(Panel_Variations);
        //    Panel_Variations.Location = new Vector2(Panel_list.Right, List.Controls[List.SelectedIndex].Top);
        //    //Panel_Variations.Location.Y = Math.Min(Camera.Height - Panel_Variations.Height, Panel_Variations.Location.Y);
        //    Panel_Variations.Location.Y -= Math.Max(0, Panel_Variations.ScreenLocation.Y + Panel_Variations.Height - Game1.Instance.graphics.PreferredBackBufferHeight);
        //}

        Vector2 Variation;
        void variation_MouseLeftPress(object sender, EventArgs e)
        {
            Variation = (Vector2)(sender as PictureBox).Tag;
        }

        void Select()
        {
            Blueprint bp = SelectedItem["Blueprint"]["Blueprint"] as Blueprint;
            GameObjectSlot haulSlot = Player.Actor["Inventory"]["Holding"] as GameObjectSlot;
            if (!haulSlot.HasValue)
                return;
            if (!bp[0].ContainsKey(haulSlot.Object.ID))
                return;

            GameObject obj = GameObject.Create(GameObject.Types.Construction);
            // obj.HandleMessage(Message.Types.SetBlueprint, Player.Actor, List.Find(bp=>(bp["Blueprint"]["Blueprint"] as Blueprint).ProductID == SelectedItem.ID), 0, 0);
            obj.HandleMessage(Message.Types.SetBlueprint, Player.Actor, bp, 0, 0);
            //GameObject obj = GameObject.Create(SelectedItem.ID);
            //obj["Sprite"]["Variation"] = (int)Variation.Y;
            //obj["Sprite"]["Orientation"] = (int)Variation.X;

            Spawn(obj);
        }
        void variation_DrawItem(object sender, DrawItemEventArgs e)
        {
            PictureBox box = sender as PictureBox;
            //if ((int)box.Tag == Variation)
            if ((Vector2)box.Tag == Variation)
                box.DrawHighlight(e.SpriteBatch, 0.5f);
            e.SpriteBatch.Draw(box.Texture, box.ScreenLocation, box.SourceRect, Color.White, 0, box.PictureOrigin, 1, SpriteEffects.None, 0);
            //Console.WriteLine(box.SourceRect);
        }

        ConstructionTool PlaceTool;
        ObjectOrientationTool2 OrientationTool;
        void build_Click(object sender, EventArgs e)
        {
           // Spawn();
            
        }

        private void Spawn(GameObject obj)
        {
            //DragDropManager.Instance.Item = obj.Clone().Initialize().ToSlot();
            //DragDropManager.Instance.Source = DragDropManager.Instance.Item;
            //return;

            //Tool.Type = (GameObject.Types)List.SelectedValue;
            if (SelectedItem == null)
            {
                Log.Enqueue(Log.EntryTypes.Default, "SpawningWindow Error: No item selected!");
                return;
            }

            //DragDropManager.Instance.Item = obj.ToSlot();// itemSlot;//.Item;
            //DragDropManager.Instance.Effects = DragDropEffects.Move | DragDropEffects.Link;
            //DragDropManager.Instance.Source = DragDropManager.Instance.Item;// this;

            PlaceTool.Type = SelectedItem.ID;
            PlaceTool.Icon = new Icon(Map.ItemSheet, 0);// StaticObject.Objects[Tool.Type].GetGui().Icon;
            PlaceTool.Object = GameObject.Create((SelectedItem["Blueprint"]["Blueprint"] as Blueprint).ProductID); // SelectedItem;// GameObject.Create(SelectedItem.ID);//.Initialize();// obj.Clone().Initialize();

            Rooms.Ingame.Instance.ToolManager.ActiveTool = PlaceTool;
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
         //   AddConstruction(tool.Variation, (int)tool.Orientation);
        }

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
        //    // OnConstructionSelected();
        //    constr.Add(Material);

        //    if (Construction == null)
        //    {
        //        Map.RemoveObject(Material);
        //        Chunk.AddObject(obj, Material.Global);
        //    }
        //}

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
