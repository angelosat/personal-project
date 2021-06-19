using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class WorkerComponent : Component
    {
        public override string ComponentName
        {
            get { return "Worker"; }
        }
        public override string ToString()
        {
            string text = "";
            foreach (var job in this.Jobs)
                text += job.Name + "\n";
            return text.TrimEnd('\n');
        }
        public HashSet<TownJob> Jobs { get; set; }
        public WorkerComponent()
        {
            this.Jobs = new HashSet<TownJob>();
        }
        public override void GetClientActions(GameObject parent, List<ContextAction> actions)
        {
            base.GetClientActions(parent, actions);
            actions.Add(new ContextAction(() => "Assign Job", () =>
            {
               // new Popup().Initialize(p =>
                Popup.Manager.Create(p =>
                {
                    p.Controls.Add(new ListBox<TownJob, Button>(new Rectangle(0, 0, 200, 300))
                        .Build(Net.Client.Instance.Map.GetTown().Jobs.Values, j => j.Name, (j, c) => 
                        {
                            c.LeftClickAction = () =>
                            {
                                //this.Jobs.Add(j);
                                Net.Client.PostPlayerInput(parent, Message.Types.AssignJob, w => w.Write(j.ID));
                                p.Hide();
                            };
                        }));
                });//.Show();
            }));
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.AssignJob:
                    e.Translate(r => 
                    {
                        var jobid = r.ReadInt32();
                        var job = e.Network.Map.GetTown().Jobs[jobid];
                        this.Jobs.Add(job);
                        job.Workers.Add(parent);
                    });
                    return true;

                default:
                    return base.HandleMessage(parent, e);
            }
        }

        public override object Clone()
        {
            return new WorkerComponent();
        }
    }
}
