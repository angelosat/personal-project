using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class TownJob : ITooltippable
    {
        public enum States { Unstarted, InProgress, Finished }
        public States State { get; set; }
        public int ID { get; set; }
        public string Name { get; set; }
        public GameObject Creator { get; set; }
        public List<TownJobStep> Steps { get; set; }
        public HashSet<TownJobStep> FinishedSteps { get; set; }
        public HashSet<GameObject> Workers { get; set; }
        public bool Instantiated { get { return ID > 0; } }
        public Time TimeFinished { get; set; }

        public void Complete()
        {
            this.State = States.Finished;
            this.TimeFinished = DateTime.Now.ToTime();
        }

        public TownJob(IObjectProvider net, params TownJobStep[] steps)
        {
            this.Workers = new HashSet<GameObject>();
            this.State = States.Unstarted;
            this.ID = 0;
            this.Steps = new List<TownJobStep>(steps);
            this.FinishedSteps = new HashSet<TownJobStep>();
            //net.GameEvent += net_GameEvent;
        }

        //void net_GameEvent(object sender, GameEvent e)
        //{
        //    switch (e.Type)
        //    {
        //        case Message.Types.ScriptFinished:
        //            GameObject actor = e.Parameters[0] as GameObject;
        //            TargetArgs target = e.Parameters[1] as TargetArgs;
        //            Script.Types script = (Script.Types)e.Parameters[2];
        //            var found = this.Steps.FirstOrDefault(step => step.Target.Object == target.Object && step.Script == script);
        //            if (found.IsNull())
        //                return;
        //            this.Steps.Remove(found);
        //            this.FinishedSteps.Add(found);
        //            if (this.Steps.Count == 0)
        //            {
        //                this.Complete();
        //                e.Net.GameEvent -= net_GameEvent;
        //            }
        //            break;

        //        default:
        //            break;
        //    }
        //    return;
        //}

        public void GetTooltipInfo(Tooltip tooltip)
        {
            if (this.State == States.Finished)
                tooltip.Controls.Add(("Completed on: " + this.TimeFinished.ToString()).ToLabel());
        }

        //public TownJob(IEnumerable<TownJobStep> steps)
        //    : this(steps.ToArray())
        //{
        //    //this.Workers = new HashSet<GameObject>();
        //    //this.State = States.Unstarted;
        //    //this.ID = 0;
        //    //this.Steps = new List<TownJobStep>(steps);
        //}

        public void Write(BinaryWriter w)
        {
            w.WriteASCII(this.Name);
            TargetArgs.Write(w, this.Creator);
            w.Write(this.Steps.Count);
            foreach (var step in this.Steps)
                step.Write(w);
        }
        static public TownJob Read(BinaryReader r, IObjectProvider net)
        {
            TownJob job = new TownJob(net);
            job.Name = r.ReadASCII();
            job.Creator = TargetArgs.Read(net, r).Object;
            int count = r.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                job.Steps.Add(TownJobStep.Read(r, net));
            }
            return job;
        }
    }
}
