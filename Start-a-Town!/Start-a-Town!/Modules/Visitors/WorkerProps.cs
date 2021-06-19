using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class WorkerProps : ISaveable, ISerializable
    {
        public int ActorID;
        public Dictionary<JobDef, Job> Jobs = new();
        public WorkerProps()
        {

        }
        public WorkerProps(Actor actor, params JobDef[] jobDefs) : this(actor.RefID, jobDefs)
        {
            //this.ActorID = actor.InstanceID;
            //foreach (var j in jobDefs)
            //    this.Jobs.Add(j, new Job(j));
        }
        public WorkerProps(int actorID, params JobDef[] jobDefs)
        {
            this.ActorID = actorID;
            foreach (var j in jobDefs)
                this.Jobs.Add(j, new Job(j));
        }
        public Job GetJob(JobDef def)
        {
            return this.Jobs[def];
            if(!this.Jobs.TryGetValue(def, out var job))
            {
                job = new Job(def);
                this.Jobs.Add(def, job);
            }
            return job;
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            this.ActorID.Save(tag, "ActorID");
            //this.Jobs.Save(tag, "Jobs", SaveTag.Types.String, j => j.Name);
            this.Jobs.Values.SaveNewBEST(tag, "Jobs");
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            this.ActorID = tag.GetValue<int>("ActorID");
            //this.Jobs.TrySync(tag, "Jobs", keyTag => Def.TryGetDef<JobDef>((string)keyTag.Value));
            tag.TryGetTag("Jobs", v => this.Jobs = v.LoadList<Job>().ToDictionary(j => j.Def, j => j));

            return this;
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.ActorID);
            //this.Jobs.Sync(w);
            this.Jobs.Values.Write(w);
        }
        public ISerializable Read(BinaryReader r)
        {
            this.ActorID = r.ReadInt32();
            //this.Jobs.Sync(r);
            this.Jobs = r.ReadList<Job>().ToDictionary(j => j.Def, j => j);
            return this;
        }
    }
}
