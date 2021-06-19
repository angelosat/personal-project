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
    class OwnershipWindow : Window
    {
        static OwnershipWindow _Instance;
        public static OwnershipWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new OwnershipWindow();
                return _Instance;
            }
        }

        public override bool Close()
        {
            return Hide();
        }

        Panel Panel_image, Panel_description, Panel_properties, Panel_buttons, Panel_Help, Panel_Variations, Panel_NpcCard;
        PanelList<GameObject, Button> Panel_List;
        Button Button_Select;

        static public GameObject SelectedItem;

        public Predicate<GameObject> Filter;

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }

        public OwnershipWindow()
        {
            Title = "Select Owner";
            Size = new Rectangle(0, 0, 200, 300);

            Panel_List = new PanelList<GameObject, Button>(Vector2.Zero, new Vector2(Size.Width, 300), foo => foo.Name);
            Panel_List.SelectedItemChanged += new EventHandler<EventArgs>(Panel_List_SelectedItemChanged);

            Panel_image = new Panel(new Vector2(Panel_List.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));
            Panel_buttons = new Panel(Panel_List.BottomLeft);

            AutoSize = true;
            Movable = true;

            List<GameObject> npclist = NpcComponent.NpcDirectory.OrderBy(x => x.Name).ToList();
            npclist.Insert(0, new GameObject(GameObject.Types.Npc, "<No Owner>", ""));
            Panel_List.Build(npclist);

            Panel_Variations = new Panel(new Vector2(0, Panel_List.Bottom), new Vector2(Panel_List.Width, 100));
            Panel_Variations.AutoSize = true;

            Panel_NpcCard = new Panel(Vector2.Zero, new Vector2(200));


            Panel_buttons.AutoSize = true;
            Button_Select = new Button(Vector2.Zero, Panel_List.ClientSize.Width, "Select");
            Button_Select.Click += new UIEvent(Button_Select_Click);
            Panel_buttons.Controls.Add(Button_Select);

            Client.Controls.Add(Panel_List, Panel_buttons);

            AutoSize = false;
            Anchor = new Vector2(0.1f, 0.2f);
            this.Location = Anchor * UIManager.Size;

            NpcComponent.NpcDirectoryChanged += new EventHandler<EventArgs>(NpcComponent_NpcDirectoryChanged);
        }

        void NpcComponent_NpcDirectoryChanged(object sender, EventArgs e)
        {
            List<GameObject> npclist = NpcComponent.NpcDirectory.OrderBy(x => x.Name).ToList();
            npclist.Insert(0, new GameObject(GameObject.Types.Npc, "<No Owner>", ""));
            Panel_List.Build(npclist);
        }

        void InitNpcCard(GameObject npc)
        {
            Panel_NpcCard.Controls.Clear();
            Sprite npcSprite = npc["Sprite"]["Sprite"] as Sprite;
            PictureBox pic = new PictureBox(Vector2.Zero, npcSprite.Texture, npcSprite.SourceRects[(int)npc["Sprite"]["Variation"]][(int)npc["Sprite"]["Orientation"]]);
            Label name = new Label(pic.TopRight, npc.Name);
            Label similar = new Label(pic.BottomLeft, "Similar items already owned:");
            Panel property = new Panel(similar.BottomLeft, new Vector2(Panel_NpcCard.ClientSize.Width, Panel_NpcCard.ClientSize.Height - pic.Height - similar.Height));
            //property.AutoSize = true;
            PropertyComponent prop = npc.GetComponent<PropertyComponent>("Property");
            foreach (GameObject obj in prop.Property)
                if (Filter(obj))
                    property.Controls.Add(new Label(property.Controls.Count > 0 ? property.Controls.Last().BottomLeft : Vector2.Zero, obj.Name));
            Panel_NpcCard.Controls.Add(pic, name, similar, property);
            Panel_NpcCard.Location = new Vector2(ClientSize.Width, Panel_List.List.Controls[Panel_List.List.Controls.FindIndex(foo => foo.Tag == npc)].Top + (int)Panel_List.List.ClientLocation.Y);
            Panel_NpcCard.Location.Y -= Math.Max(0, Panel_Variations.ScreenLocation.Y + Panel_Variations.Height - WindowManager.ScreenHeight);//Game1.Instance.graphics.PreferredBackBufferHeight);
        }

        void Button_Select_Click(object sender, EventArgs e)
        {
            GameObject obj = Tag as GameObject;
            obj.PostMessage(Message.Types.SetOwner, null, SelectedItem);
            if (SelectedItem != null)
                InitNpcCard(SelectedItem);

            Hide();
        }

        void Panel_List_SelectedItemChanged(object sender, EventArgs e)
        {
            SelectedItem = (Panel_List.SelectedItem.Name == "<No Owner>") ? null : Panel_List.SelectedItem; // TODO: check if index == 0 instead
            if (SelectedItem != null)
            {
                InitNpcCard(Panel_List.SelectedItem);
                Controls.Add(Panel_NpcCard);
            }
            else
                Controls.Remove(Panel_NpcCard);
            OnSelectedItemChanged();
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
              //  e.SpriteBatch.Draw(label.TextSprite, new Vector2(finalRect.X, finalRect.Y), source, Color.Lerp(Color.Transparent, Color.White, label.Opacity));
            }
        }

        void listEntry_DrawItem(object sender, DrawItemEventArgs e)
        {
            Label label = sender as Label;
            if (label.Tag == SelectedItem)
                label.DrawHighlight(e.SpriteBatch, 0.5f);
        }

        public override bool Show(params object[] p)
        {
            List<GameObject> npclist = NpcComponent.NpcDirectory.OrderBy(x => x.Name).ToList();
            npclist.Insert(0, new GameObject(GameObject.Types.Npc, "<No Owner>", ""));
            Panel_List.Build(npclist);
            AlignToMouse(Panel_List.Location + Panel_List.Controls.First().Location + new Vector2(Label.DefaultHeight / 2));
            return base.Show(p);
        }
    }
}
