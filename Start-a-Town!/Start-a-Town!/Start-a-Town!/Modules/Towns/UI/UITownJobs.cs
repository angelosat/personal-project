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
    class UITownJobs: GroupBox
    {
        static UITownJobs _Instance;
        static public UITownJobs Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new UITownJobs();
                return _Instance;
            }
        }

        ListBox<TownJob, Button> Joblist { get; set; }

        UITownJobs()
        {
            this.AutoSize = true;
            Panel panel_joblist = new Panel() { AutoSize = true };

            this.Joblist = new ListBox<TownJob, Button>(new Rectangle(0, 0, 200, 400));

            panel_joblist.Controls.Add(this.Joblist);

            this.Controls.Add(panel_joblist);
            this.Location = CenterScreen;
        }

        //public TownJobsWindow Show(TownJobCollection jobs)
        //{
        //    this.Show();
        //    Refresh(jobs);
        //    return this;
        //}

        public void Refresh(TownJobCollection jobs)
        {
            this.Joblist.Build(jobs.Values,
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
        }
    }
}
