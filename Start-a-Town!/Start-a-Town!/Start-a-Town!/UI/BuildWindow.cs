using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using Start_a_Town_.PlayerControl;

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
           // return ConstructionComponent.ToggleConstructions(base.Show(p));// base.Show(p);
        }
        public override bool Toggle()
        {
            Refresh(Player.Actor["Inventory"]["Holding"] as GameObjectSlot);
          //  return ConstructionComponent.ToggleConstructions(base.Toggle());
            return base.Toggle();
        }
        //public override bool Hide()
        //{
        //    return ConstructionComponent.ToggleConstructions(!base.Hide());
        //}

        Panel Panel_image, Panel_description, Panel_properties, Panel_buttons, Panel_Help, Panel_Variations, Panel_Filter, Panel_List;//, Panel_Hauling;
        //PanelList<GameObject, Button> Panel_List;
        ListBox<GameObject, Button> Box_List;
        Label Lbl_Available;
        CheckBox Chk_Mats, Chk_Known;
        Button Btn_Build;
        SlotWithText Slot_Hauling;

        ConstructionTool PlaceTool;
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

        BuildWindow()
        {
            Title = "Build";
            Size = new Rectangle(0, 0, 300, 300);
            AutoSize = true;
            Movable = true;

            Label lbl_haul = new Label(Vector2.Zero, "Hauling:");
            Slot_Hauling = new SlotWithText(lbl_haul.BottomLeft);

            Lbl_Available = new Label(Slot_Hauling.BottomLeft, "Available Constructions:");

            Panel_Filter = new Panel(Lbl_Available.BottomLeft, new Vector2(Size.Width, 1));
            Panel_Filter.AutoSize = true;

            Chk_Mats = new CheckBox("Have mats", Vector2.Zero);
            Chk_Mats.Checked = true;
            Chk_Mats.LeftClick += new UIEvent(Chk_Filter_Click);

            Chk_Known = new CheckBox("Known", Chk_Mats.TopRight);
            Chk_Known.Checked = true;
            Chk_Known.LeftClick += new UIEvent(Chk_Filter_Click);

            Panel_Filter.Controls.Add(Chk_Mats, Chk_Known);

            Panel_List = new Panel() { Location = Panel_Filter.BottomLeft, AutoSize = true, };
            Box_List = new ListBox<GameObject, Button>(new Rectangle(0, 0, Size.Width, 200));
            Box_List.SelectedItemChanged += new EventHandler<EventArgs>(Panel_List_SelectedItemChanged);
            Panel_List.Controls.Add(Box_List);

            Panel_image = new Panel(new Vector2(Box_List.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));

            Btn_Build = new Button(Vector2.Zero, "Craft");
            Btn_Build.LeftClick += new UIEvent(Btn_Build_Click);

            Panel_Variations = new Panel(Panel_List.BottomLeft, new Vector2(Panel_List.Width, 100));

            Client.Controls.Add(lbl_haul, Slot_Hauling, Lbl_Available, Panel_List, Panel_Filter, Panel_Variations);
            
            AutoSize = false;
            Anchor = new Vector2(0.1f, 0.2f);
            this.Location = Anchor * UIManager.Size;

            PlaceTool = ConstructionTool.Instance;
            PlaceTool.MouseLeft += new EventHandler<EventArgs>(PlaceTool_MouseLeft);
            OrientationTool = new ObjectOrientationTool2();
            OrientationTool.MouseLeft += new EventHandler<EventArgs>(OrientationTool_MouseLeft);

            Game1.TextInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(TextInput_KeyPress);
        }

        void Btn_Build_Click(object sender, EventArgs e)
        {
            Blueprint bp = SelectedItem["Blueprint"]["Blueprint"] as Blueprint;
            throw new NotImplementedException();
            //Player.Actor.PostMessage(Message.Types.BeginInteraction, null, new Interaction(timespan: new TimeSpan(0, 0, 1), message: Message.Types.CraftObject, source: Player.Actor, verb: "Crafting"), bp);
        }

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
            GameObjectSlot haulSlot = Player.Actor["Inventory"]["Holding"] as GameObjectSlot;
            if (haulSlot.Object != LastHauling)
                Refresh(haulSlot);
            LastHauling = haulSlot.Object;
            base.Update();
        }

        public void Refresh(GameObjectSlot haulSlot)
        {


            List<GameObject> Memorized = new List<GameObject>();
            if (Player.Actor != null)
            {
                KnowledgeCollection memorized = new KnowledgeCollection(Player.Actor["Memory"].GetProperty<KnowledgeCollection>("Memories").FindAll(obj => GameObject.Objects[obj.Object].Type == ObjectType.Blueprint || GameObject.Objects[obj.Object].Type == ObjectType.Plan).ToArray());
                Memorized.AddRange(memorized.ConvertAll<GameObject>(mem => GameObject.Objects[mem.Object]));
            }


            Controls.Remove(Panel_Variations);


            Slot_Hauling.Tag = haulSlot;// SetTag(haulSlot);
            List<GameObject> finalList = WorkbenchComponent.Blueprints.Skip(1).ToList().FindAll(bp =>
                    (!Chk_Known.Checked || (Chk_Known.Checked && Memorized.Contains(bp))) &&
                    (!Chk_Mats.Checked  || (Chk_Mats.Checked && HasMats(bp)))
                );
            List = finalList;
            Box_List.Build(List, foo=>foo.Name);
         //   Box_List.Location = Lbl_Available.BottomLeft;
            ToolManager.Instance.ActiveTool = null;
        }

        bool HasMats(GameObject blueprint)
        {
            Blueprint bp = (blueprint["Blueprint"]["Blueprint"] as Blueprint);
            if (blueprint.Type == ObjectType.Plan || blueprint.Type == ObjectType.Schematic)
            {
                GameObjectSlot haulSlot = Slot_Hauling.Tag as GameObjectSlot;
                if (haulSlot.HasValue)
                    if (bp[0].ContainsKey(haulSlot.Object.ID))
                        return true;
            }
            else if (blueprint.Type == ObjectType.Blueprint)
            {
                if (InventoryComponent.HasItems(Player.Actor, bp[0]))
                    return true;
            }
            return false;
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


            GameObject obj = GameObjectDb.Construction;
            throw new NotImplementedException();
            //obj.PostMessage(Message.Types.SetBlueprint, Player.Actor, SelectedItem["Blueprint"]["Blueprint"] as Blueprint, variation, orientation);
            //obj.PostMessage(Message.Types.DropOn, Player.Actor);
            Chunk.AddObject(obj, Engine.Map, PlaceTool.Global);

            Rooms.Ingame.Instance.ToolManager.ActiveTool = null;
        }

        void PlaceTool_MouseLeft(object sender, EventArgs e)
        {
            Vector3 d = (Player.Actor.Global - PlaceTool.Global);
            float l = d.Length();
            if (l > 2)
                return;

            return;
        }

        void Panel_list_DrawItem(object sender, DrawItemEventArgs e)
        {
            Panel panel = sender as Panel;
            foreach (Label label in panel.Controls)
            {
                Rectangle finalRect;
                Rectangle labelRect = label.ScreenBounds;
                Rectangle panelRect = panel.ScreenBounds;//Bounds;
                Rectangle.Intersect(ref labelRect, ref panelRect, out finalRect);

                if (label.Tag == SelectedItem)
                    label.DrawHighlight(e.SpriteBatch, finalRect, 0.5f);
  
                Rectangle source = new Rectangle(0, finalRect.Y - labelRect.Y, finalRect.Width, finalRect.Height);
             //   e.SpriteBatch.Draw(label.TextSprite, new Vector2(finalRect.X, finalRect.Y), source, Color.Lerp(Color.Transparent, Color.White, label.Opacity));

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
            SelectedItem = Box_List.SelectedItem;
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
            Rectangle rect = sprite.SourceRects[0][0];
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

         //   Controls.Add(Panel_Variations);

            //Panel_Variations.Location = new Vector2(ClientSize.Width, Box_List.List.Controls[Box_List.List.Controls.FindIndex(foo => foo.Tag == SelectedItem)].Top + (int)Box_List.List.ClientLocation.Y + (int)Box_List.Location.Y);
            //Panel_Variations.Location.Y -= Math.Max(0, Panel_Variations.ScreenLocation.Y + Panel_Variations.Height - WindowManager.ScreenHeight);
        }

        void variation_DrawTooltip(object sender, TooltipArgs e)
        {
            //Blueprint bp = SelectedItem["Blueprint"]["Blueprint"] as Blueprint;
            //GameObject.Objects[bp.ProductID].GetTooltip(e.Tooltip);
            SelectedItem.GetTooltip(e.Tooltip);
        }



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
            throw new NotImplementedException();
            //obj.PostMessage(Message.Types.SetBlueprint, Player.Actor, bp, 0, 0);


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

            PlaceTool.Type = SelectedItem.ID;
            PlaceTool.Icon = new Icon(Map.ItemSheet, 0, 32);// StaticObject.Objects[Tool.Type].GetGui().Icon;
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

    }
}
