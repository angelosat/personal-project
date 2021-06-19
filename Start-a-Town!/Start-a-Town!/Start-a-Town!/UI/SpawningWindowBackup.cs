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

        Panel Panel_image, Panel_description, Panel_properties, Panel_buttons, Panel_list, Panel_Help, Panel_Variations;
        ScrollableList List;
        Scrollbar Scroll;

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

            Panel_list = new Panel(Vector2.Zero, new Vector2(Size.Width, 300));
            
            Panel_image = new Panel(new Vector2(Panel_list.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));

            

            AutoSize = true;
            Movable = true;

            Objects = GameObject.Objects;
           // SelectedItem = GameObject.Objects.First().Value;
            
            List = new ScrollableList(Vector2.Zero, Panel_list.ClientSize.Width, Panel_list.ClientSize.Height);
          //  List.SelectedItem = SelectedItem;
            List.ControlAdded += new EventHandler<EventArgs>(List_ControlAdded);
            List.ControlRemoved+=new EventHandler<EventArgs>(List_ControlRemoved);
            //Scroll = new Scrollbar(new Vector2(Panel_list.Right, 0), Panel_list.Height);
            //Scroll = new Scrollbar(new Vector2(Panel_list.ClientSize.Width - 16, 0), Panel_list.Height);

            Scroll = new Scrollbar(new Vector2(List.ClientSize.Width - Scrollbar.Width, 0), List.Height);
            //Scroll = new Scrollbar(new Vector2(Panel_list.Right - 16, 0), List.Height);
            Scroll.Tag = List;

            int n = 0;
          // Panel_list.AutoSize = true;
            foreach (KeyValuePair<GameObject.Types, GameObject> obj in GameObject.Objects)
            {
                Label listEntry = new Label(new Vector2(0, n), obj.Value.Name);
                n += Label.DefaultHeight;
                listEntry.Tag = obj.Value;
                listEntry.Click += new UIEvent(listEntry_Click);
                listEntry.DrawMode = UI.DrawMode.OwnerDrawFixed;
                listEntry.DrawItem += new EventHandler<DrawItemEventArgs>(listEntry_DrawItem);
                //Panel_list.Controls.Add(listEntry);
                List.Add(listEntry);
            }
          //  Panel_list.Height /= 2;
           // Panel_list.DrawMode = UI.DrawMode.OwnerDrawFixed;
           // Panel_list.DrawItem += new EventHandler<DrawItemEventArgs>(Panel_list_DrawItem);
            Panel_list.Controls.Add(List);

            Panel_Variations = new Panel(new Vector2(0, Panel_list.Bottom), new Vector2(Panel_list.Width, 100));
            Panel_Variations.AutoSize = true;


            
            Controls.Add(Panel_list);//, ScrollBar);//, Panel_buttons });
            
            AutoSize = false;
            this.Location = new Vector2((int)Math.Floor(CenterScreen.X / 2), (int)Math.Floor(CenterScreen.Y / 2));
            //Location = BottomLeftScreen;
          //  build.Click += new UIEvent(build_Click);

            Tool = new ObjectSpawnTool();
        }

        void  List_ControlRemoved(object sender, EventArgs e)
        {
            ScrollableList list = sender as ScrollableList;
            if (list.ClientSize.Height <= ClientSize.Height)
            {
                list.Size = new Rectangle(0, 0, Panel_list.ClientSize.Width, list.Size.Height);
                Controls.Remove(Scroll);
            }
        }

        void List_ControlAdded(object sender, EventArgs e)
        {
            ScrollableList list = sender as ScrollableList;
            if (list.ClientSize.Height > ClientSize.Height)
            {
                list.Size = new Rectangle(0, 0, Panel_list.ClientSize.Width - 16, list.Size.Height);
                Panel_list.Controls.Add(Scroll);
            }
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

        void listEntry_Click(object sender, EventArgs e)
        {
            SelectedItem = (sender as Label).Tag as GameObject;
            List.SelectedItem = SelectedItem;
            InitVariationPanel(SelectedItem);
        }

        void InitVariationPanel(GameObject obj)
        {
            Sprite sprite = obj.GetComponent<SpriteComponent>("Sprite").Sprite;
            Variation = Vector2.Zero;
            int k = 0, n = 0;
            foreach (PictureBox picBox in Panel_Variations.Controls)
            {
                picBox.DrawItem -= variation_DrawItem;
                picBox.MouseLeftPress -= variation_MouseLeftPress;
            }
            Panel_Variations.Controls.Clear();
            foreach (Rectangle[] strip in sprite.SourceRect)
            {
                foreach (Rectangle rect in strip)
                {
                    PictureBox variation = new PictureBox(new Vector2(k * rect.Width, n * rect.Height), sprite.Texture, rect, TextAlignment.Left);
                    variation.Tag = new Vector2(k, n);
                    variation.MouseLeftPress += new EventHandler<InputState>(variation_MouseLeftPress);
                    variation.DrawMode = UI.DrawMode.OwnerDrawVariable;
                    variation.DrawItem += new EventHandler<DrawItemEventArgs>(variation_DrawItem);
                    Panel_Variations.Controls.Add(variation);
                    k += 1;// variation.Width;
               //    Console.WriteLine(variation.Size);
                }
                n += 1;
                k = 0;
            }

            Controls.Add(Panel_Variations);
            ////Panel_Variations.Location = new Vector2(ScrollBar.Right, Panel_list.Controls[Panel_list.Controls.FindIndex(foo => foo.Tag == obj)].Top + Panel_list.ClientLocation.Y);//List.Controls[List.SelectedIndex].Top);
            //Panel_Variations.Location = new Vector2(ClientSize.Width, Panel_list.Controls[Panel_list.Controls.FindIndex(foo => foo.Tag == obj)].Top + Panel_list.ClientLocation.Y);//List.Controls[List.SelectedIndex].Top);
            //Panel_Variations.Location.Y -= Math.Max(0, Panel_Variations.ScreenLocation.Y + Panel_Variations.Height - Game1.Instance.graphics.PreferredBackBufferHeight);


            Panel_Variations.Location = new Vector2(ClientSize.Width, List.Controls[List.Controls.FindIndex(foo => foo.Tag == obj)].Top + (int)List.ClientLocation.Y);
            Panel_Variations.Location.Y -= Math.Max(0, Panel_Variations.ScreenLocation.Y + Panel_Variations.Height - WindowManager.ScreenHeight);//Game1.Instance.graphics.PreferredBackBufferHeight);
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
            //Tool.Type = (GameObject.Types)List.SelectedValue;
            if (SelectedItem == null)
            {
                Log.Write(Log.EntryTypes.Default, "SpawningWindow Error: No item selected!");
                return;
            }
            Tool.Type = SelectedItem.ID;
            Tool.Icon = new Icon(Map.ItemSheet, 0);// StaticObject.Objects[Tool.Type].GetGui().Icon;
            Tool.Object = obj;

            Rooms.Ingame.Instance.ToolManager.ActiveTool = Tool;
        }



        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
