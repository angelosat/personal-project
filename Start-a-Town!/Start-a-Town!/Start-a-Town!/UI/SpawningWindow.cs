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
    class SpawningWindow : Window
    {
        static SpawningWindow _Instance;
        public static SpawningWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new SpawningWindow();
                return _Instance;
            }
        }

        public override bool Close()
        {
            return Hide();
        }

        Panel Panel_image, Panel_description, Panel_properties, Panel_buttons, Panel_Help, Panel_Variations, Panel_Search, Panel_List;
        //PanelList<GameObject, Button> Panel_List;
        ListBox<GameObject, Button> Box_List;
        TextBox Txt_Search;

        //PanelList<GameObject> Panel_List;
       // ListBox List;

        static public GameObject SelectedItem;

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }


        GameObjectCollection Objects;

     //   int k;
        public SpawningWindow()
        {
            Title = "Objects";
            Size = new Rectangle(0, 0, 200, 300);

            //Panel_list = new Panel(Vector2.Zero, new Vector2(Size.Width, 300));
            Panel_Search = new Panel();
            Panel_Search.AutoSize = true;
            Panel_Search.ClientSize = new Rectangle(0, 0, ClientSize.Width, Label.DefaultHeight);
            Txt_Search = new TextBox();
            Txt_Search.Width = this.Width;
            Txt_Search.TextEntered += new EventHandler<TextEventArgs>(Txt_Search_TextEntered);
            IconButton btn_clear = new IconButton() { Location = Txt_Search.TopRight, BackgroundTexture = UIManager.Icon16Background, Icon = new Icon(UIManager.Icons16x16, 0, 16), HoverFunc = () => "Clear" };
           // btn_clear.HoverText = "Clear";
            btn_clear.LeftClick += new UIEvent(btn_clear_Click);
            Panel_Search.Controls.Add(Txt_Search, btn_clear);


            Box_List = new ListBox<GameObject, Button>(new Rectangle(0, 0, Panel_Search.ClientSize.Width, 300)) { Name = "Box List" };// { Location = Panel_Search.BottomLeft };//, Width = Panel_Search.Width, Height = 300, };

            Box_List.SelectedItemChanged += new EventHandler<EventArgs>(Panel_List_SelectedItemChanged);

            Panel_image = new Panel(new Vector2(Box_List.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));            

            AutoSize = true;
            Movable = true;

            //Box_List.Build(GameObject.Objects.Values.OrderBy(x => x.Name).ToList(), foo => foo.Name, foo => foo.Description);
            Box_List.Build(GameObject.Objects.Values.OrderBy(x => x.Name).ToList(), foo => foo.Name, onControlInit: (obj, ctrl) =>
            {
                ctrl.TooltipFunc = (tooltip) =>
                {
                    (ctrl.Tag as GameObject).GetTooltipBasic(tooltip);
                    //GameObject obj = ctrl.Tag as GameObject;
                    //Color quality = InfoComponent.GetQualityColor(obj);
                    //Sprite sprite = (Sprite)obj["Sprite"]["Sprite"];
                    ////tooltip.Controls.Add(new Panel() { AutoSize = true }.AddControls(sprite.ToPictureBox()));
                    //PictureBox pic = sprite.ToPictureBox();
                    //Label name = new Label(pic.TopRight, obj.Name, quality, Color.Black, UIManager.FontBold);// { Location = pic.TopRight };// obj.Name.ToLabel(pic.TopRight);
                    //Label desc = obj.Description.ToLabel(name.BottomLeft);
                    //tooltip.AddControls(pic, name, desc);
                    //tooltip.Color = quality;
                };
            });

            Panel_Variations = new Panel(new Vector2(0, Box_List.Bottom), new Vector2(Box_List.Width, 100)) { ClipToBounds = false };
            Panel_Variations.AutoSize = true;


            Panel_List = new Panel() { Name = "Panel_List", Location = Panel_Search.BottomLeft, AutoSize = true };
            Panel_List.Controls.Add(Box_List);
            Client.Controls.Add(Panel_Search, Panel_List);//Box_List);//, ScrollBar);//, Panel_buttons });
            
            AutoSize = false;
            Anchor = new Vector2(0.1f, 0.2f);
            this.Location = Anchor * UIManager.Size;
            //this.Location = new Vector2((int)Math.Floor(CenterScreen.X / 2), (int)Math.Floor(CenterScreen.Y / 2));
            //Location = BottomLeftScreen;
          //  build.Click += new UIEvent(build_Click);

            Tool = new ObjectSpawnTool();
        }

        void btn_clear_Click(object sender, EventArgs e)
        {
            Txt_Search.Text = "";
            Box_List.Build(GameObject.Objects.Values.OrderBy(x => x.Name).ToList(), foo => foo.Name, foo => foo.Description);
        }

        void Txt_Search_TextEntered(object sender, TextEventArgs e)
        {
            switch (e.Char)
            {
                case '\b':
                    if (Txt_Search.Text == "")
                    {
                        Box_List.Build(GameObject.Objects.Values.OrderBy(x => x.Name).ToList(), foo => foo.Name, foo => foo.Description);
                        return;
                    }
                    break;

                default:
                    Txt_Search.Text += e.Char;
                    break;
            }
            Box_List.Build(GameObject.Objects.Values.ToList().FindAll(foo => foo.Name.ToLower().Contains(Txt_Search.Text.ToLower())), foo => foo.Name, foo => foo.Description);
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
            GameObject obj = GameObject.Create(SelectedItem.ID);
            Spawn(obj);

          //  InitVariationPanel(SelectedItem);
            //Control ctrl = sender as Control;
            //Panel_Variations.Location = new Vector2(ClientSize.Width, Panel_List.Top + ctrl.Top + (int)Box_List.Client.ClientLocation.Y + Box_List.Location.Y);
        }

        void InitVariationPanel(GameObject obj)
        {

            Sprite sprite = (Sprite)obj["Sprite"]["Sprite"];

            Variation = Vector2.Zero;
            int k = 0, n = 0;
            foreach (PictureBox picBox in Panel_Variations.Controls)
            {
                picBox.DrawItem -= variation_DrawItem;
                picBox.MouseLeftPress -= variation_MouseLeftPress;
            }
            Panel_Variations.Controls.Clear();
            foreach (Rectangle[] strip in sprite.SourceRects)
            {
                foreach (Rectangle rect in strip)
                {
                    PictureBox variation = new PictureBox(new Vector2(k * rect.Width, n * rect.Height), sprite.Texture, rect, HorizontalAlignment.Left);
                    variation.Tag = new Vector2(k, n);
                    variation.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(variation_MouseLeftPress);
                    variation.DrawMode = UI.DrawMode.OwnerDrawVariable;
                    variation.DrawItem += new EventHandler<DrawItemEventArgs>(variation_DrawItem);
                    Panel_Variations.Controls.Add(variation);
                    k += 1;
                }
                n += 1;
                k = 0;
            }

            Client.Controls.Add(Panel_Variations);


            //Panel_Variations.Location = new Vector2(ClientSize.Width, Panel_List.List.Controls[Panel_List.List.Controls.FindIndex(foo => foo.Tag == obj)].Top + (int)Panel_List.List.ClientLocation.Y);
            //Panel_Variations.Location.Y -= Math.Max(0, Panel_Variations.ScreenLocation.Y + Panel_Variations.Height - WindowManager.ScreenHeight);
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

            GameObject obj = GameObject.Create(SelectedItem.ID);
            obj["Sprite"]["Variation"] = (int)Variation.Y;
            obj["Sprite"]["Orientation"] = (int)Variation.X;
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

        ObjectSpawnTool Tool;
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

            Tool.Type = SelectedItem.ID;
            Tool.Icon = new Icon(Map.ItemSheet, 0, 32);// StaticObject.Objects[Tool.Type].GetGui().Icon;
            Tool.Object = obj;// GameObject.Create(SelectedItem.ID);//.Initialize();// obj.Clone().Initialize();

            Rooms.Ingame.Instance.ToolManager.ActiveTool = Tool;
        }

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}
