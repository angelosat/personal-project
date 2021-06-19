using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    class JobBoardWindow : Window
    {
        #region Singleton
        static JobBoardWindow _Instance;
        public static JobBoardWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new JobBoardWindow();
                return _Instance;
            }
        }
        #endregion

        static bool TryGetNearbyJobBoard(out GameObject board)
        {
            board = Player.Actor.GetNearbyObjects(
                range => range <= Math.Sqrt(3), 
                filter: obj => obj.ID == GameObject.Types.JobBoard)
                .FirstOrDefault();//obj => obj.ID == GameObject.Types.JobBoard);
            return board != null;
        }

        PanelList<GameObject> Panel_JobQueue, Panel_PostedJobs;
        Button Btn_PostAll, Btn_Delete, Btn_Remove, Btn_RemoveAll, Btn_DeleteAll, Btn_PostJob;
        float UpdateTimer = Engine.TargetFps;

        JobBoardWindow()
        {
            JobComponent.JobComplete += new Action<GameObject, GameObject>(JobComponent_JobComplete);

            this.Title = "Job Board";
            AutoSize = true;
            Movable = true;
            Closable = true;

            Label
                queuelabel = new Label("Job Queue");


            Panel_JobQueue = new PanelList<GameObject>(queuelabel.BottomLeft, new Vector2(200, 300), foo => foo.Name) { Name = "Job Queue" };

            Label jobsLabel = new Label( "Posted Jobs") {Location = new Vector2(Panel_JobQueue.Width, 0), };
            Panel_PostedJobs = new PanelList<GameObject>(jobsLabel.BottomLeft, new Vector2(200, 300), foo => foo.Name) { Name = "Posted Jobs" };

            Btn_PostJob = new Button("Post Job", Panel_JobQueue.Width) { Location = Panel_JobQueue.BottomLeft };
            Btn_PostJob.LeftClick += new UIEvent(Btn_PostJob_Click);

            Btn_PostAll = new Button(Btn_PostJob.BottomLeft, Panel_JobQueue.Width, "Post All");
            Btn_PostAll.LeftClick += new UIEvent(Btn_PostAll_Click);

            Btn_Delete = new Button("Delete Job", Btn_PostAll.Width) { Location = Btn_PostAll.BottomLeft };
            Btn_Delete.LeftClick += new UIEvent(Btn_Delete_Click);

            Btn_DeleteAll = new Button("Delete All", Btn_PostAll.Width) { Location = Btn_Delete.BottomLeft };
            Btn_DeleteAll.LeftClick += new UIEvent(Btn_DeleteAll_Click);

            Btn_Remove = new Button("Remove Job", Btn_PostAll.Width) { Location = Panel_PostedJobs.BottomLeft };
            Btn_Remove.LeftClick += new UIEvent(Btn_Remove_Click);

            Btn_RemoveAll = new Button("Remove All", Btn_PostAll.Width) { Location = Btn_Remove.BottomLeft };
            Btn_RemoveAll.LeftClick += new UIEvent(Btn_RemoveAll_Click);


            Client.Controls.Add(Panel_JobQueue, Panel_PostedJobs, Btn_PostAll, queuelabel, jobsLabel, Btn_Delete, Btn_Remove, Btn_PostJob, Btn_RemoveAll, Btn_DeleteAll);

            Location = CenterScreen;// UIManager.Mouse;
        }

        void Btn_DeleteAll_Click(object sender, EventArgs e)
        {
            MessageBox.Create("Warning!", "Are you sure you want to delete all jobs?",
                yesAction: () =>
                {
                    GameObject board;
                    if (TryGetNearbyJobBoard(out board))
                        board.PostMessage(Message.Types.JobDeleteAll, Player.Actor, RefreshPostedJobs);
                })
                .ShowDialog();
        }

        void Btn_RemoveAll_Click(object sender, EventArgs e)
        {
            MessageBox.Create("Warning!", "Are you sure you want to remove all jobs?",
                yesAction: () =>
                {
                    GameObject board;
                    if (TryGetNearbyJobBoard(out board))
                        board.PostMessage(Message.Types.JobRemoveAll, Player.Actor, RefreshPostedJobs);
                })
                .ShowDialog();
        }

        public override void Update()
        {
            base.Update();
            //GameObject board = Tag as GameObject;
            //if (board == null)
            //    return;
            //if (Vector3.Distance(Player.Actor.Global, board.Global) > Math.Sqrt(3))
            //    Hide();

            this.UpdateTimer -= 1;//GlobalVars.DeltaTime;
            if (UpdateTimer <= 0)
            {
                UpdateTimer = Engine.TargetFps;
                UpdateButtons();
            }
        }

        private void UpdateButtons()
        {
            GameObject board;
            bool active = TryGetNearbyJobBoard(out board);
            Controls.Where(ctrl => ctrl is Button).ToList().ForEach(
                btn =>
                {
                    btn.Active = active;
                    btn.HoverText = active ? "" : "Requires a Job Board in range";
                });
        }

        void Btn_PostJob_Click(object sender, EventArgs e)
        {
            GameObject job = Panel_JobQueue.SelectedItem;
            if (job == null)
                return;

            GameObject board;
            if(TryGetNearbyJobBoard(out board))
                board.PostMessage(Message.Types.Post, Player.Actor, RefreshPostedJobs, job);
            //(Tag as GameObject).PostMessage(Message.Types.Post, Player.Actor, RefreshPostedJobs, job);

        }

        void Btn_Remove_Click(object sender, EventArgs e)
        {
            GameObject job = Panel_PostedJobs.SelectedItem;
            if (job == null)
                return;

            GameObject board;
            if (TryGetNearbyJobBoard(out board))
                board.PostMessage(Message.Types.JobRemove, Player.Actor, RefreshPostedJobs, job);
            //(Tag as GameObject).PostMessage(Message.Types.JobRemove, Player.Actor, RefreshPostedJobs, job);

        }

        void Btn_Delete_Click(object sender, EventArgs e)
        {
            GameObject job = Panel_JobQueue.SelectedItem;
            if (job == null)
                return;

            GameObject board;
            if (TryGetNearbyJobBoard(out board))
                board.PostMessage(Message.Types.JobDelete, Player.Actor, RefreshPostedJobs, job);
           // (Tag as GameObject).PostMessage(Message.Types.JobDelete, Player.Actor, RefreshPostedJobs, job);
        }

        void Btn_PostAll_Click(object sender, EventArgs e)
        {
            GameObject board;
            if (TryGetNearbyJobBoard(out board))
                board.PostMessage(Message.Types.PostAll, Player.Actor, RefreshPostedJobs);
           // (Tag as GameObject).PostMessage(Message.Types.PostAll, Player.Actor, RefreshPostedJobs);

        }

        void JobComponent_JobComplete(GameObject arg1, GameObject arg2)
        {
            RefreshPostedJobs();
        }

        

        public void RefreshPostedJobs(GameObject board)
        {
            if(board.ID != GameObject.Types.JobBoard)
                throw new ArgumentException(board.Name + " not a valid object");

            Panel_PostedJobs.Build(JobBoardComponent.PostedJobs.ConvertAll(foo=>foo.Object));
            Panel_JobQueue.Build(JobBoardComponent.JobPool.ToList());
        }
        public void RefreshPostedJobs()
        {
            Panel_PostedJobs.Build(JobBoardComponent.PostedJobs.ConvertAll(foo => foo.Object));
            Panel_JobQueue.Build(JobBoardComponent.JobPool.ToList());
            
        }
        //public JobBoardWindow Initialize(GameObject obj)
        //{
        //   // Controls.Clear();

        //    if (obj == null)
        //        throw new ArgumentException("Null object");
        //    if (obj.ID != GameObject.Types.JobBoard)
        //        throw new ArgumentException("Invalid object");
        //    Tag = obj;

        //    Panel_JobQueue.Build(JobBoardComponent.JobPool.ToList());
        //    Panel_PostedJobs.Build(JobBoardComponent.PostedJobs.ConvertAll(foo => foo.Object));
        // //   Controls.Add(Panel_Jobs);
        //    return this;
        //}

        public override bool Show(params object[] p)
        {
            Panel_JobQueue.Build(JobBoardComponent.JobPool.ToList());
            Panel_PostedJobs.Build(JobBoardComponent.PostedJobs.ConvertAll(foo => foo.Object));
            UpdateButtons();
            return base.Show(p);
        }

        //static public JobBoardWindow Initialize(GameObject obj)
        //{
        //    if (obj == null)
        //        throw new ArgumentException("Null object");
        //    if (obj.ID != GameObject.Types.JobBoard)
        //        throw new ArgumentException("Invalid object");

        //    Instance.Initialize(obj);

        //    return Instance;
        //}
    }
}
