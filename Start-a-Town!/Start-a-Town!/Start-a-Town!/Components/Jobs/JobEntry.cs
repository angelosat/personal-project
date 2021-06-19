using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.Components.Jobs
{
    class JobEntry : Component
    {
        public override string ComponentName
        {
            get { return "JobEntry"; }
        }
        public GameObject Parent { get { return (GameObject)this["Parent"]; } set { this["Parent"] = value; } }
        public GameObject JobObject { get { return (GameObject)this["JobObject"]; } set { this["JobObject"] = value; } }
        public InteractionOld Goal { get { return (InteractionOld)this["Goal"]; } set { this["Goal"] = value; } }
        public string Name { get { return (string)this["Name"]; } set { this["Name"] = value; } }
        public Time TimeAccepted { get { return (Time)this["TimeAccepted"]; } set { this["TimeAccepted"] = value; } }
        public int AttemptsRemaining { get { return (int)this["AttemptsRemaining"]; } set { this["AttemptsRemaining"] = value; } }
        public int AttemptsMax { get { return (int)this["AttemptsMax"]; } set { this["AttemptsMax"] = value; } }
        // public Job.States State { get { return (Job.States)this["State"]; } set { this["State"] = value; } }

        public JobEntry(InteractionOld goal, GameObject parent, GameObject jobObj)
        {
            this.Goal = goal;
            this.TimeAccepted = DateTime.Now.ToTime();
            Personality personality = (Personality)parent["AI"]["Personality"];
            this.AttemptsMax = personality.PatienceOld;// 50;
            this.AttemptsRemaining = this.AttemptsMax;
            //  this.State = Job.States.Default;
            this.Name = goal.Name;
            this.Parent = parent;
            this.JobObject = jobObj;
        }

        public Job.States Update()
        {
            this.AttemptsRemaining = Math.Max(AttemptsRemaining - 1, 0);
            if (AttemptsRemaining == 0)
                return Job.States.Failed;
            return Job.States.Running;
        }

        public override object Clone()
        {
            return new JobEntry(this.Goal, this.Parent, this.JobObject);
        }
    }
}
