using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class UIFinishedJobs : GroupBox
    {
        ListBox<TownJob, Button> List_Finished { get; set; }
        public UIFinishedJobs()//IEnumerable<TownJob> finishedJobs)
        {
         //   this.AutoSize = true;
            this.List_Finished = new ListBox<TownJob, Button>(new Rectangle(0, 0, 150, 300));
            this.Controls.Add(this.List_Finished);
                //.Build(finishedJobs, j => j.Name, (j, c) => { });
        }

        public UIFinishedJobs Refresh(IEnumerable<TownJob> finishedJobs)
        {
            this.List_Finished.Build(finishedJobs, j => j.Name, (j, c) => { });
            return this;
        }
    }
}
