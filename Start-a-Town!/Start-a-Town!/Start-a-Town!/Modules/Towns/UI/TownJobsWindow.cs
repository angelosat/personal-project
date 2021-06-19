using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Towns;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class TownJobsWindow: Window
    {
        static TownJobsWindow _Instance;
        static public TownJobsWindow Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new TownJobsWindow();
                return _Instance;
            }
        }

        //ListBox<TownJobStep, Button> Joblist { get; set; }
        ListBox<TownJob, Button> Joblist { get; set; }

        UIFinishedJobs UIFinishedJobs { get; set; }

        TownJobsWindow()
        {
            Title = "Jobs";
            this.AutoSize = true;
            this.Movable = true;
            Panel panel_joblist = new Panel() { AutoSize = true };

            this.Joblist = new ListBox<TownJob, Button>(new Rectangle(0, 0, 150, 300));

            panel_joblist.Controls.Add(this.Joblist);

            Panel panel_jobhistory = new Panel() { AutoSize = true }; //Location = panel_joblist.TopRight, 
            this.UIFinishedJobs = new UIFinishedJobs();
            panel_jobhistory.Controls.Add(this.UIFinishedJobs);

            Panel panel_buttons = new Panel() {Location = panel_joblist.BottomLeft,  AutoSize = true };

            Button btn_history = new Button("History", Joblist.Width)
            {
                LeftClickAction = () => Popup.Manager.Create(c => c.Controls.Add(panel_jobhistory))//Popup.ShowNew(c => c.Controls.Add(panel_jobhistory))
            };
            panel_buttons.Controls.Add(btn_history);

            this.Client.Controls.Add(panel_joblist, panel_buttons);//, panel_jobhistory);
            this.Location = CenterScreen;
        }
        public TownJobsWindow Show(Town town)//TownJobCollection jobs)
        {
            this.Show();
            Refresh(town);
            return this;
        }

        public void Refresh(Town town)//TownJobCollection jobs)
        {
            this.Joblist.Build(town.Jobs.Values,
                j => j.ID.ToString(),
                //Ability.GetScript(j.Script).Name + ": " + j.Target.Object.Name,
                (j, c) =>
                {
                    //c.LeftClickAction = () => TownJobEditWindow.Instance.Show(j);
                    c.LeftClickAction = () =>
                    {
                        ContextMenu2.Instance.Initialize(
                            new ContextAction(() => "Edit", () => TownJobEditWindow.Instance.Show(j)),
                            new ContextAction(() => "Delete", () => Net.Client.Request(Net.PacketType.JobDelete, w => w.Write(j.ID)))
                            );
                    };
                }
            );

            this.UIFinishedJobs.Refresh(town.JobHistory);
        }
        //public TownJobsWindow Show(IEnumerable<TownJobStep> jobs)
        //{
        //    this.Show();
        //    this.Joblist.Build(jobs,
        //        j =>
        //            Ability.GetScript(j.Script).Name + ": " + j.Target.Object.Name,
        //        (j, c) => { }
        //    );
        //    return this;
        //}
    }
}
