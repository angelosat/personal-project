using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class JobWindow : Window
    {
        static JobWindow _Instance;
        public static JobWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new JobWindow();
                return _Instance;
            }
        }

        //List<Interaction> Job;
        Job Job;
        PanelList<InteractionOld> Panel_Interactions;
        JobTool Tool;
        Panel Panel_Name, Panel_Buttons;
        PanelList<GameObjectSlot> Panel_JobList;
        PanelList<GameObject> Panel_Workers;
        Button BTN_Clear, BTN_Create, BTN_New, BTN_Delete;
        TextBox Txt_JobName;

      //  GroupBox Box_JobCreate, Box_JobList;

        JobWindow()
        {
            Title = "Create Job";
            //Size = new Rectangle(0, 0, 200, 200);
            AutoSize = true;
            Movable = true;

            Job = new Job(); // List<Interaction>();

            Panel_JobList = new PanelList<GameObjectSlot>(Vector2.Zero, new Vector2(200, 300), jobSlot => jobSlot.Object.Name);
            Panel_JobList.SelectedItemChanged += new EventHandler<EventArgs>(Panel_JobList_SelectedItemChanged);
            //Panel_Interactions = new Panel(Vector2.Zero, new Vector2(200, 100));
            Panel_Name = new Panel(Panel_JobList.TopRight);
            Panel_Name.AutoSize = true;
            this.Txt_JobName = new TextBox(Vector2.Zero, new Vector2(200, Label.DefaultHeight));
            this.Txt_JobName.Text = Job.Name;// "Job " + JobComponent.Counter;
            this.Txt_JobName.TextEntered += new EventHandler<TextEventArgs>(Txt_JobName_TextEntered);

            

            Panel_Name.Controls.Add(Txt_JobName);
            Panel_Name.BackgroundStyle = BackgroundStyle.TickBox;

            Panel_Interactions = new PanelList<InteractionOld>(this.Panel_Name.BottomLeft, new Vector2(Panel_Name.Width, 150), foo => foo.Name + " " + foo.Source.Name, HorizontalAlignment.Left);

            Panel_Workers = new PanelList<GameObject>(this.Panel_Interactions.BottomLeft, new Vector2(Panel_Name.Width, 150), foo => foo.Name, HorizontalAlignment.Left);


            Panel_Buttons = new Panel(Panel_Interactions.TopRight);
            Panel_Buttons.AutoSize = true;

            BTN_New = new Button(Vector2.Zero, 100, "New");
            BTN_New.LeftClick += new UIEvent(BTN_New_Click);
            BTN_Clear = new Button(BTN_New.BottomLeft, 100, "Clear");
            BTN_Clear.LeftClick += new UIEvent(BTN_Clear_Click);
            BTN_Create = new Button(BTN_Clear.BottomLeft, 100, "Save");
            BTN_Create.LeftClick += new UIEvent(BTN_Create_Click);
            BTN_Delete = new Button(BTN_Create.BottomLeft, 100, "Delete");
            BTN_Delete.LeftClick += new UIEvent(BTN_Delete_Click);

            Panel_Buttons.Controls.Add(BTN_New, BTN_Clear, BTN_Create, BTN_Delete);

            Controls.Add(Panel_JobList, Panel_Name, Panel_Interactions, Panel_Workers, Panel_Buttons);

            Location = Center;

            Tool = JobTool.Instance;
            Tool.SelectedItemChanged += new EventHandler<EventArgs>(Tool_SelectedItemChanged);
          //  Rooms.Ingame.Instance.ToolManager.ActiveTool = Tool;
        }

        public void RefreshJobList()
        {
            Panel_JobList.SelectedItem = null;
            Panel_JobList.Build(JobBoardComponent.PostedJobs);
            RefreshInteractions();
            RefreshWorkers();
        }

        public void RefreshInteractions()
        {
            Panel_Interactions.Build();
        }

        public void RefreshWorkers()
        {
            GameObjectSlot jobSlot = Panel_JobList.SelectedItem as GameObjectSlot;
            if (jobSlot == null)
                Panel_Workers.Build();
            else
                Panel_Workers.Build(jobSlot.Object["Job"]["Workers"] as List<GameObject>);
            return;
        }

        void BTN_Delete_Click(object sender, EventArgs e)
        {
            if (Panel_JobList.SelectedItem == null)
                return;
            GameObjectSlot jobSlot = Panel_JobList.SelectedItem as GameObjectSlot;
            JobBoardComponent.PostedJobs.Remove(jobSlot);
            Panel_JobList.Build(JobBoardComponent.PostedJobs);
        }

        

        void Panel_JobList_SelectedItemChanged(object sender, EventArgs e)
        {
            GameObjectSlot jobSlot = Panel_JobList.SelectedItem as GameObjectSlot;
            Txt_JobName.Text = jobSlot.Object.Name.Split(new string[] {"Job: "}, StringSplitOptions.RemoveEmptyEntries)[0];
            Panel_Interactions.Build(jobSlot.Object["Job"]["Tasks"] as List<InteractionOld>);
        }

        void Txt_JobName_TextEntered(object sender, TextEventArgs e)
        {
            TextBox.DefaultTextHandling(sender as TextBox, e);
        }

        

        void BTN_Create_Click(object sender, EventArgs e)
        {
            GameObject obj;
            if (Panel_JobList.SelectedItem != null)
            {
                obj = (Panel_JobList.SelectedItem as GameObjectSlot).Object;
                obj.Name = "Job: " + Txt_JobName.Text;//.Replace("Job: ", "");
            }
            else
            {
                if (Job.Count == 0)
                    return;
                obj = GameObject.Create(GameObject.Types.TestJob);
                JobBoardComponent.PostedJobs.Add(obj.ToSlot());
                obj.Name = "Job: " + this.Job.Name;
            }
            throw new NotImplementedException();
            //obj.PostMessage(Message.Types.SetJob, Player.Actor, Job);
            

            Job = new Job();
            Panel_Interactions.Build(Job);
            this.Txt_JobName.Text = Job.Name;// "Job " + JobComponent.Counter;
            Panel_JobList.Build(JobBoardComponent.PostedJobs);
          //  Hide();
        }

        void BTN_New_Click(object sender, EventArgs e)
        {
            Panel_JobList.SelectedItem = null;
            Txt_JobName.Text = Job.Name;
            Panel_Interactions.Build();//new List<Interaction>());
            Rooms.Ingame.Instance.ToolManager.ActiveTool = Tool;
        }

        void BTN_Clear_Click(object sender, EventArgs e)
        {
            Job.Clear();
            Panel_Interactions.Build(Job);
        }

        void Tool_SelectedItemChanged(object sender, EventArgs e)
        {
            //Panel_Interactions.Controls.Add(new Label(Vector2.Zero, Tool.SelectedItem.Name));
            Job.Add(Tool.SelectedItem);
            Panel_Interactions.Build(Job);
        }

        public override bool Show(params object[] p)
        {
            Panel_JobList.Build(JobBoardComponent.PostedJobs);
            Rooms.Ingame.Instance.ToolManager.ActiveTool = Tool;
            return base.Show(p);
        }
        public override bool Hide()
        {
            Rooms.Ingame.Instance.ToolManager.ActiveTool = null;
            return base.Hide();
        }

        public override void Dispose()
        {
            //base.Dispose();
        }
    }
}
