using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.Components
{
    class JobBoardComponent : Component
    {
        public override string ComponentName
        {
            get { return "JobBoard"; }
        }
        //List<GameObject> JobList { get { return (List<GameObject>)this["Jobs"]; } set { this["Jobs"] = value; } }
      //  static public List<GameObject> JobList = new List<GameObject>();
        //static public Queue<GameObjectSlot> JobList = new Queue<GameObjectSlot>();
        static public List<GameObjectSlot> PostedJobs = new List<GameObjectSlot>();
        static public Queue<GameObject> JobPool = new Queue<GameObject>();
        //public JobBoardComponent()
        //{
        //    JobList = new List<GameObject>();
        //}

        static public void Initialize()
        {
            PostedJobs = new List<GameObjectSlot>();
            JobPool = new Queue<GameObject>();
        }

        static void PostAll()
        {
            while (JobPool.Count > 0)
                PostedJobs.Add(JobPool.Dequeue().ToSlot());
        }
        static void Post(GameObject job)
        {
            List<GameObject> newpool = JobPool.ToList();
            if (newpool.Remove(job))
            {
                PostedJobs.Add(job.ToSlot());
                JobPool = new Queue<GameObject>(newpool);
            }
        }
        static void Remove(GameObject job)
        {
            JobPool.Enqueue(job);
            PostedJobs = JobBoardComponent.PostedJobs.Where(foo => foo.Object != job).ToList();
        }
        static void RemoveAll()
        {
            PostedJobs.ForEach(foo => JobPool.Enqueue(foo.Object));
            PostedJobs.Clear();
        }
        static void Delete(GameObject job)
        {
            var newList = JobPool.ToList();
            newList.Remove(job);
            JobPool = new Queue<GameObject>(newList);
        }
        static void DeleteAll()
        {
            JobPool.Clear();
        }

        static public object Enqueue(GameObject job)
        {
            JobPool.Enqueue(job);
         //   NotificationArea.Write("Job queued: " + job.Name);
            Log.Enqueue(Log.EntryTypes.System, "Job enqueued: " + job.Name);
            JobBoardWindow.Instance.RefreshPostedJobs();
            return null;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.PostAll:
                    PostAll();
                    AIComponent.Invalidate(parent);
                    return true;

                case Message.Types.Post:
                    Post(e.Parameters[0] as GameObject);
                    AIComponent.Invalidate(parent);
                    return true;

                case Message.Types.JobRemove:
                    Remove(e.Parameters[0] as GameObject);
                    return true;

                case Message.Types.JobDelete:
                    Delete(e.Parameters[0] as GameObject);
                    return true;

                case Message.Types.JobDeleteAll:
                    DeleteAll();
                    return true;

                case Message.Types.JobRemoveAll:
                    RemoveAll();
                    return true;

                case Message.Types.Activate:
                    throw new NotImplementedException();
                    //GameObject.PostMessage(e.Sender, Message.Types.UIJobBoard, parent);
                    return true;

                case Message.Types.JobGet:
                    if (PostedJobs.Count == 0)
                        return true;
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.UpdateJobs, parent, PostedJobs.First().Object.ToSlot());// JobList.Dequeue());
                    return true;
                case Message.Types.Give:
                    GameObjectSlot jobSlot = e.Parameters[0] as GameObjectSlot;
                    //JobList.Enqueue(jobSlot);
                    PostedJobs.Add(jobSlot);
                    return true;
                //case Message.Types.Query:
                //    Query(parent, e);
                //    return true;
                default:
                    return true;
            }
        }

        public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        {
            // List<Interaction> list = e.Parameters[0] as List<Interaction>;
            list.Add(new InteractionOld(timespan: TimeSpan.Zero, message: Message.Types.Activate, source: parent, name: "View jobs") { CanBeJob = false });
            if (JobPool.Count > 0)
                list.Add(new InteractionOld(timespan: new TimeSpan(0, 0, 1), message: Message.Types.PostAll, source: parent, name: "Post Jobs", verb: "Posting Jobs"));

            list.Add(new InteractionOld(timespan: new TimeSpan(0, 0, 1), message: Message.Types.Give, source: parent, name: "Give", verb: "Giving"));

            if (PostedJobs.Count == 0)
                return;


            list.Add(new InteractionOld(
                timespan: new TimeSpan(0, 0, 1),
                message: Message.Types.JobGet,
                source: parent,
                name: "Look for job",
                verb: "Looking for job",
                //      cond: new InteractionConditionCollection(),
                //actorCond:
                //new InteractionConditionCollection(
                //    new InteractionCondition(actor =>
                //    {
                //        AIAutonomy bhav = (actor["AI"]["Behaviors"] as List<Behavior>).FirstOrDefault(b => b is AIAutonomy) as AIAutonomy;
                //        if (bhav == null)
                //            return false;
                //        if (bhav.AcceptedJobs.Except(PostedJobs.Select().Count == 0)
                //            return false;
                //        return true;
                //    })),
                effect: new NeedEffectCollection() { new AIAdvertisement("Work", 20) }
                ));
        }

        public override object Clone()
        {
            return new JobBoardComponent();
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            tooltip.Controls.Add(new Label(tooltip.Controls.Last().BottomLeft, "Jobs: " + PostedJobs.Count.ToString()));
        }
    }
}
