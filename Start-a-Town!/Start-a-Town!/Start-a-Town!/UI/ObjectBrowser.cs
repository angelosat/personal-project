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
    class ObjectBrowser : Window
    {
        Panel Panel_image, Panel_description, Panel_properties, Panel_buttons, Panel_list, Panel_Help;
        //PictureBox Image;
        //Label Properties, Description;


        //List<Wall> Walls = new List<Wall>();
        ListBox List;

        static public GameObject SelectedItem;

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }
        
        //SortedDictionary<uint, Plan> Plans;
        GameObjectCollection Objects;

        int k;
        public ObjectBrowser()
        {
            Title = "Browser";

            //Panel_image = new Panel(new Vector2(0), new Vector2(100, 100));
            //Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(200, 100));
            //Panel_description = new Panel(new Vector2(0, Panel_image.Bottom), new Vector2(300, 100));
            //Panel_buttons = new Panel(new Vector2(0, Panel_description.Bottom), new Vector2(300, 23));
            //Panel_list = new Panel(new Vector2(0, Panel_buttons.Bottom), new Vector2(300, 100));

            Panel_list = new Panel(new Vector2(0, 0), new Vector2(150, 100));
            
            Panel_image = new Panel(new Vector2(Panel_list.Right, 0), new Vector2(100, 100));
            Panel_properties = new Panel(new Vector2(Panel_image.Right, 0), new Vector2(150, 100));
            Panel_description = new Panel(new Vector2(Panel_image.Left, Panel_image.Bottom), new Vector2(250, 70));
            Panel_Help = new Panel(new Vector2(Panel_properties.Right, 0));

            

            AutoSize = true;
            Movable = true;

            Objects = GameObject.Objects;

            k = 0;
            //SelectedItem = StaticObject.Objects[k];
            SelectedItem = GameObject.Objects[(GameObject.Types)k];

            Panel_list.AutoSize = true;

            List = new ListBox(Vector2.Zero, Panel_list.ClientSize);
            List.SelectedValueChanged += new EventHandler<EventArgs>(list_SelectedValueChanged);
            List.DisplayMember = "Name";
            List.ValueMember = "ID";
            //foreach (KeyValuePair<uint, Plan> plankey in Plans)
            //    List.Items.Add(plankey.Value);
            foreach (KeyValuePair<GameObject.Types, GameObject> pair in Objects)
                List.Items.Add(pair.Value);
            //SelectedItem = Plans[Plans.Keys.ElementAt(k++)];

            //Panel_list.AutoSize = true;

            //List = new ListBox(Vector2.Zero, Panel_list.ClientSize);
            ////List.AutoSize = true;
            //List.SelectedValueChanged += new EventHandler<EventArgs>(list_SelectedValueChanged);
            //List.DisplayMember = "Name";
            //List.ValueMember = "ID";
            //foreach (KeyValuePair<uint, Plan> plankey in Plans)
            //    List.Items.Add(plankey.Value);

            List.Build();
            //List.ItemHeight = UIManager.SampleButton.Height;
            //List.ItemWidth = UIManager.SampleButton.Width;
            //List.DrawMode = DrawMode.OwnerDrawFixed;
            //List.DrawItem += new EventHandler<DrawItemEventArgs>(list_DrawItem);
            Panel_list.Controls.Add(List);

            //Properties = new Label(Vector2.Zero, SelectedItem.Name + '\n' + "Complexity " + SelectedItem.Complexity.ToString());
            //Description = new Label(Vector2.Zero, UIManager.WrapText(SelectedItem.Description, Panel_description.ClientSize.Width));

            //Image = new PictureBox(Vector2.Zero, SelectedItem.Sprite, SelectedItem.SourceRect, TextAlignment.Center);
            Panel_buttons = new Panel(new Vector2(0, Panel_list.Bottom), new Vector2(Panel_list.Width, 23));
            Button build = new Button(new Vector2(0), Panel_buttons.ClientSize.Width, "Spawn");
            Panel_buttons.AutoSize = true;
            Panel_buttons.Controls.Add(build);

            //Panel_list.Height = Panel_image.Height + Panel_description.Height - Panel_buttons.Height;
            //Panel_buttons.Location.Y = Panel_list.Bottom;

            
            //Panel_Help.AutoSize = true;
            //Label help_text = new Label(Vector2.Zero,
            //    "Left click to place\nCtrl click to remove\nRight click to stop building\n<, > to rotate\nK, L to change style");
            //Panel_Help.Controls.Add(help_text);

            //Panel_Help.Height = Panel_properties.Height;
            //Panel_description.Width = Panel_image.Width + Panel_properties.Width + Panel_Help.Width;
            //Panel_image.Controls.Add(Image);
            //Image.Location = new Vector2(Panel_image.ClientSize.Width / 2, Panel_image.ClientSize.Height / 2); 

            //Panel_properties.Controls.Add(Properties);
            //Panel_description.Controls.Add(Description);
            
            //Panel_buttons.ClientSize = Panel_buttons.PreferredSize;


            //AutoSize = true;
            //Controls.AddRange(new Control[] { Panel_list, Panel_buttons, Panel_properties, Panel_description, Panel_image, Panel_Help });

            Controls.Add(Panel_list, Panel_buttons);

            
            Location = BottomLeftScreen;
            build.Click += new UIEvent(build_Click);
        }

        void list_SelectedValueChanged(object sender, EventArgs e)
        {
            //ListBox list = sender as ListBox;
            //SelectedItem = Plans[(uint)list.SelectedValue];
            //Properties.Text = SelectedItem.Name + '\n' + "Complexity " + SelectedItem.Complexity.ToString();
            //Description.Text = UIManager.WrapText(SelectedItem.Description, Panel_description.ClientSize.Width); //SelectedItem.Description;
            //Image.Texture = SelectedItem.Sprite;
            //Image.SourceRect = SelectedItem.SourceRect;

            //SelectedItem = Plans[(uint)List.SelectedValue];
            SelectedItem = Objects[(GameObject.Types)List.SelectedValue];
            //if (!((Game1.Instance.CurrentRoom as Start_a_Town_.Rooms.Ingame).ToolManager.Current is BuildTool))
            //    (Game1.Instance.CurrentRoom as Rooms.Ingame).ToolManager.NewBuildTool();
        }

        void build_Click(object sender, EventArgs e)
        {
            InventoryComponent.Give(Player.Actor, (GameObject.Types)List.SelectedValue, 1);
            //if (!((Game1.Instance.CurrentRoom as Start_a_Town_.Rooms.Ingame).ToolManager.Current is BuildTool))
            //    (Game1.Instance.CurrentRoom as Rooms.Ingame).ToolManager.NewBuildTool();
        }

        //void list_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    Plan plan = sender as Plan;
        //    BackgroundStyle.Tooltip.Draw(e.SpriteBatch, e.Bounds);
        //    e.SpriteBatch.Draw(plan.Sprite, new Vector2(e.Bounds.X + (e.Bounds.Width - plan.SourceRect.Width) / 2, e.Bounds.Y + (e.Bounds.Height - plan.SourceRect.Height) / 2), plan.SourceRect, Color.White);
        //}

        public override void Dispose()
        {
            //List.DrawItem -= list_DrawItem;
            base.Dispose();
        }
    }
}
