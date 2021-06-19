using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Jobs;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class NpcJobsWindow : Window
    {
        static Dictionary<GameObject, NpcJobsWindow> _OpenJobs;
        static Dictionary<GameObject, NpcJobsWindow> OpenJobs
        {
            get
            {
                if (_OpenJobs == null)
                    _OpenJobs = new Dictionary<GameObject, NpcJobsWindow>();
                return _OpenJobs;
            }
        }

        //PanelList<JobEntry> List_Jobs;
        ListBox<JobEntry, Label> List_Jobs;

        NpcJobsWindow()
        {
            this.Title = "Jobs";
            //this.Size = new Rectangle(0, 0, 300, 500);
            this.AutoSize = true;
            this.Movable = true;
            //List_Jobs = new PanelList<JobEntry>(Vector2.Zero, new Vector2(300, 500), foo => foo.Name);
            //List_Jobs.List.HoverFunc = (foo => foo.ToString());//TimeAccepted.ToString() + "\n" + foo.Name);
            List_Jobs = new ListBox<JobEntry, Label>(new Rectangle(0, 0, 200, 300));
        }

        public NpcJobsWindow Initialize(GameObject npc, List<JobEntry> acceptedJobs)
        {
            this.Title = npc.Name + "'s jobs";
            Client.Controls.Remove(List_Jobs);
           // List_Jobs.Build();
            List_Jobs.Build(acceptedJobs, foo => foo.Name, hoverTextFunc: foo => foo.ToString());
            Client.Controls.Add(List_Jobs);
            return this;
        }

        static public NpcJobsWindow Show(GameObject obj, List<JobEntry> acceptedJobs)
        {
            NpcJobsWindow win;
            if (!OpenJobs.TryGetValue(obj, out win))
            {
                win = new NpcJobsWindow().Initialize(obj, acceptedJobs);
                win.Tag = obj;
                OpenJobs[obj] = win;
            }
            win.Show();
            return win;
        }
    }
}
