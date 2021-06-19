using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.UI
{
    class ProjectsWindow : Window
    {
        PanelList<GameObject, Button> Panel_List;
        Panel Panel_Details;
        Button Btn_Create, Btn_Edit, Btn_CreatePlan;
        Window Window_Name;
        TextBox Txt_ProjectName;
        IMap Map;
        public Block.Types SelectedTile = Block.Types.Sand;

        ProjectTool Tool;

        static ProjectsWindow _Instance;
        static public ProjectsWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ProjectsWindow();
                return _Instance;
            }
            set { _Instance = value; }
        }

        ProjectsWindow()
        {
            Title = "Projects";
            AutoSize = true;
            Movable = true;

            Panel_List = new PanelList<GameObject, Button>(Vector2.Zero, new Rectangle(0, 0, 200, 300), foo => foo.Name);
            Panel_List.SelectedItemChanged += new EventHandler<EventArgs>(Panel_List_SelectedItemChanged);
            Panel_List.Build(ProjectComponent.ProjectList);

            Btn_Create = new Button(Panel_List.BottomLeft, Panel_List.Width, "Create");
            Btn_Create.LeftClick += new UIEvent(Btn_Create_Click);
            Btn_CreatePlan = new Button(Btn_Create.BottomLeft, Panel_List.Width, "Create plan");
            Btn_CreatePlan.LeftClick += new UIEvent(Btn_CreatePlan_Click);

            Panel_Details = new Panel(Panel_List.TopRight, new Vector2(200, Panel_List.Height + Btn_Create.Height));
            Btn_Edit = new Button("Edit");
            Btn_Edit.LeftClick += new UIEvent(Btn_Edit_Click);
            Panel_Details.Controls.Add(Btn_Edit);

            Controls.Add(Panel_List, Btn_Create, Btn_CreatePlan, Panel_Details);
            this.SnapToScreenCenter();
            InitWindowName();
            Map = Engine.Map;
           // Tool = new ProjectTool();
        }

        void Btn_CreatePlan_Click(object sender, EventArgs e)
        {
            IMap map = Start_a_Town_.Map.Create(new WorldArgs("blueprint1", false, 0, new Terraformer[] { Terraformer.Land }, false));//, Tile.Types.Cobblestone));true, 
            Engine.Map = map;
            map.World.DefaultTile = Block.Types.Cobblestone;
          //  ChunkLoader.Map = map;
            ChunkLoader.ForceLoad(ScreenManager.CurrentScreen, map, new System.Threading.CancellationToken());
           // map.Focus(Vector3.Zero);
        }

        void Panel_List_SelectedItemChanged(object sender, EventArgs e)
        {
            Tool = new ProjectTool(Panel_List.SelectedItem as GameObject);
        }

        void Btn_Edit_Click(object sender, EventArgs e)
        {
            ToolManager.Instance.ActiveTool = Tool;
        }

        public override bool Toggle()
        {
            Engine.Map = Map;
            return base.Toggle();
        }

        private void InitWindowName()
        {
            Window_Name = new Window();
            Window_Name.Title = "Project name";
            Window_Name.AutoSize = true;

            Txt_ProjectName = new TextBox(Vector2.Zero, new Vector2(200, Label.DefaultHeight));
            Txt_ProjectName.TextEntered += new EventHandler<TextEventArgs>(Txt_ProjectName_TextEntered);

            Button
                btn_done = new Button(Txt_ProjectName.BottomLeft, 200, "Done"),
                btn_cancel = new Button(btn_done.BottomLeft, 200, "Cancel");

            btn_done.LeftClick += new UIEvent(btn_done_Click);
            btn_cancel.LeftClick += new UIEvent(btn_cancel_Click);

            Window_Name.Controls.Add(Txt_ProjectName, btn_done, btn_cancel);
            //Window_Name.Location = CenterScreen;
            Window_Name.SnapToScreenCenter();

        }

        void Txt_ProjectName_TextEntered(object sender, TextEventArgs e)
        {
            if (e.Char != '\b')
                Txt_ProjectName.Text += e.Char;
        }

        void Btn_Create_Click(object sender, EventArgs e)
        {
            Window_Name.Show();
        }

        void btn_cancel_Click(object sender, EventArgs e)
        {
            Txt_ProjectName.Text = "";
            Window_Name.Hide();
        }

        void btn_done_Click(object sender, EventArgs e)
        {
            ProjectComponent.Create(Txt_ProjectName.Text);
            Panel_List.Build(ProjectComponent.ProjectList);
            Txt_ProjectName.Text = "";
            Window_Name.Hide();
        }
    }
}
