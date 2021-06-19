using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class Job : List<InteractionOld>
    {
        public enum States { Default, Accepted, Running, Finished, Failed }

        public string Name { get; set; }
        public Job() { this.Name = "Job " + JobComponent.Counter; }
        public Job(string name) : base() { this.Name = name; }

        static public GameObject Create(InteractionOld inter)
        {
            Job job = new Job();
            job.Name += ": " + inter.Name + " " + inter.Source.Name;
            job.Add(inter);

            GameObject obj = GameObject.Create(GameObject.Types.TestJob);
            throw new NotImplementedException();
            //obj.PostMessage(Message.Types.SetJob, Player.Actor, job);

            

            obj.Name = job.Name;// "Job: " + inter.Name + " " + inter.Source.Name;
            return obj;
        }
    }

    class JobComponent : Component
    {
        public override string ComponentName
        {
            get { return "Job"; }
        }
        static uint _Counter = 0;
        static public uint Counter { get { return _Counter++; } }

        static public event Action<GameObject, GameObject> JobComplete;
        static void OnJobComplete(GameObject actor, GameObjectSlot jobSlot)
        {
            // TODO: make JobBoardComponent.Jobs private
            // WARNING: bug probably
            JobBoardComponent.PostedJobs.Remove(jobSlot);


           // JobBoardComponent.Remove(jobSlot.Object);
            if (JobComplete != null)
                JobComplete(actor, jobSlot.Object);
        }

        public List<InteractionOld> Tasks { get { return (List<InteractionOld>)this["Tasks"]; } set { this["Tasks"] = value; } }
        public List<GameObject> Workers { get { return (List<GameObject>)this["Workers"]; } set { this["Workers"] = value; } }
        public bool Finished { get { return (bool)this["Finished"]; } set { this["Finished"] = value; } }

        public override string ToString()
        {
            string text = "Job:\n";
            foreach (InteractionOld i in Tasks)
                text += i + "\n";
            return text.TrimEnd('\n');
        }

        public JobComponent()
        {
            this.Tasks = new List<InteractionOld>();
            this.Workers = new List<GameObject>();
            this.Finished = false;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.InteractionFinished:
                    InteractionOld inter = e.Parameters[0] as InteractionOld;
                    foreach (GameObjectSlot slot in JobBoardComponent.PostedJobs.ToList())
                    {
                        List<InteractionOld> interList = slot.Object["Job"]["Tasks"] as List<InteractionOld>;
                        if (!interList.Remove(inter))
                            continue;

                        if (interList.Count == 0)
                        {
                            this.Finished = true;
                            throw new NotImplementedException();
                            //GameObject.PostMessage(e.Sender, Message.Types.JobComplete, parent);
                            
                            OnJobComplete(parent, slot);
                        }
                        // TODO: put UI logic elsewhere
                      //  UI.JobWindow.Instance.RefreshJobList();

                        return true;
                        
                    }
                    return true;
                case Message.Types.JobAccepted:
                   // this.Workers.Add(e.Sender);
                    AddWorker(parent, e.Sender);
                    return true;
                case Message.Types.SetJob:
                    Tasks = e.Parameters[0] as List<InteractionOld>;
                    return true;
                default:
                    return false;
            }
        }

        static void AddWorker(GameObject parent, GameObject worker)
        {
            JobComponent jobComp;
            if (!parent.TryGetComponent<JobComponent>("Job", out jobComp))
                return;
            jobComp.Workers.Add(worker);

            // TODO: not sure if wise to modify the interface from here instead of doing events
            UI.JobWindow.Instance.RefreshWorkers();
        }

        public override object Clone()
        {
            JobComponent jobComp = new JobComponent();
            return jobComp;
        }
    }
}
