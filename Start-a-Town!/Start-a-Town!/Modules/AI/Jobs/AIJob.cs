using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace Start_a_Town_.AI
{
    [Obsolete]
    public class AIJob : ICloneable
    {
        public enum States { Default, GeneratingPlan, CalculatingPaths, Ready, Inaccessible }
        public States State;

        public List<AIInstruction> Instructions = new List<AIInstruction>();
        int Step;
        public GameObject ReservedBy;
        public GameObject EvaluatedBy;
        bool Cancelled = false;
        public AIJob Source;
        public AITask Task;
        public JobDef Labor;
        string _Description;
        //private IObjectProvider objectProvider;
        //private BinaryReader r;
        
        public virtual void Reserve(GameObject actor)
        {
            if (this.ReservedBy != null)
                throw new Exception();
            this.ReservedBy = actor;
        }
        public void Unreserve()
        {
            this.ReservedBy = null;
        }
        public GameObject GetReservedBy()
        {
            return this.ReservedBy;
        }
        public string Description
        {
            get
            {
                return string.IsNullOrEmpty(_Description) ? this.Instructions.Last().ToString() : _Description;
            }
            set
            {
                _Description = value;
            }
        }
        //public AIGoalState GoalState;
        //public ScriptTaskCondition GoalState;

        public AIJob()
        {

        }
        public AIJob(params AIInstruction[] instructions)
        {
            this.Instructions = instructions.ToList();
        }

        internal void AddStep(AIInstruction i)
        {
            //if (i.Target.Type == TargetType.Entity)
            //    if (AIState.IsItemReserved(i.Target.Object))
            //        throw new Exception();
            this.Instructions.Add(i);
        }
        internal void NextStep()
        {
            //unreserve items used in the last interaction here? check if they're used again in the instruction sequence?
            //if (this.CurrentStep.Target.Type == TargetType.Entity)
            //    AIState.UnreserveItem(this.CurrentStep.Target.Object);
            //this.Step++;
        }
        internal AIInstruction CurrentStep
        {
            get
            {
                return this.Instructions.ElementAtOrDefault(this.Step);
                //return this.Instructions[this.Step];
            }
        }
        internal bool IsCancelled(GameObject actor)
        {
            if (this.Cancelled)
                return true;
            if (this.Source != null)
                if (this.Source.IsCancelled(actor))
                    return true;
            foreach (var step in this.Instructions)
            {
                if (step.Interaction.IsCancelled(actor, step.Target))
                {
                    this.Cancel();
                    return true;
                }
            }
            return false;
        }
        internal virtual void Cancel()
        {
            this.Cancelled = true;
            if (this.ReservedBy == null)
                return;
            var state = AIState.GetState(this.ReservedBy);
            this.Dispose(this.ReservedBy); // if this throws then GOOD! the reservedby should be set if we ever need call Cancel
        }
        public virtual void Dispose(GameObject actor)
        {
            //foreach (var i in this.Instructions)
            //    if (i.Target.Type == TargetType.Entity)
            //        AIState.UnreserveItem(i.Target.Object);
        }
        internal bool IsFinished
        {
            get
            {
                return this.Step >= this.Instructions.Count;
            }
        }
        internal bool IsAvailable
        {
            get
            {
                return this.ReservedBy == null & this.EvaluatedBy == null;
            }
        }

        public override string ToString()
        {
            return this.Instructions.Last().ToString() + (this.Instructions.Count > 1 ? " +" + (this.Instructions.Count - 1).ToString() : "");
        }

        internal void Release()
        {
            this.ReservedBy = null;
        }


        public void Write(BinaryWriter w)
        {
            w.Write(this.ReservedBy != null ? this.ReservedBy.RefID : -1);
            w.Write(this.Description);
            w.Write(this.Instructions.Count);
            foreach (var i in this.Instructions)
                i.Write(w);
            w.Write(this.Step);
        }
        public void Read(IObjectProvider net, BinaryReader r)
        {
            var entityid = r.ReadInt32();
            if (entityid > -1)
                this.ReservedBy = net.GetNetworkObject(entityid);
            this.Description = r.ReadString();
            var icount = r.ReadInt32();
            for (int i = 0; i < icount; i++)
            {
                this.Instructions.Add(new AIInstruction(net, r));
            }
            this.Step = r.ReadInt32();
        }
        public AIJob(IObjectProvider objectProvider, BinaryReader r)
        {
            this.Read(objectProvider, r);
        }
        public List<SaveTag> Save()
        {
            //SaveTag tag = new SaveTag(SaveTag.Types.Compound);
            List<SaveTag> tag = new List<SaveTag>();
            //tag.Add(new SaveTag(SaveTag.Types.Int, "InstructionCount", this.Instructions.Count));
            SaveTag instructionsTag = new SaveTag(SaveTag.Types.List, "Instructions", SaveTag.Types.Compound);
            foreach (var i in this.Instructions)
                instructionsTag.Add(i.Save());
            tag.Add(new SaveTag(SaveTag.Types.Int, "CurrentStep", this.Step));
            return tag;
        }
        public void Load(IObjectProvider net, SaveTag tag)
        {
            List<SaveTag> instructions = tag["Instructions"].Value as List<SaveTag>;
            foreach(var itag in instructions)
            {
                var i = new AIInstruction(net, itag);
                this.Instructions.Add(i);
            }
            this.Step = tag.GetValue<int>("CurrentStep");
        }
        public AIJob(IObjectProvider net, SaveTag tag)
        {
            this.Load(net, tag);
        }

        
        public virtual object Clone()
        {
            return new AIJob();
        }
    }

    public class AIJobOld
    {
        //public List<AIInstruction> Instructions = new List<AIInstruction>();
        public Queue<AIInstruction> Instructions = new Queue<AIInstruction>();
        public Queue<AIInstruction> FinishedInstructions = new Queue<AIInstruction>();

        public AIJobOld()
        {

        }
        public AIJobOld(params AIInstruction[] instructions)
        {
            //this.Instructions.AddRange(instructions);
            foreach (var i in instructions)
                this.Instructions.Enqueue(i);
        }

        public bool Cancelled = false;

        internal void NextStep()
        {
            this.FinishedInstructions.Enqueue(this.Instructions.Dequeue());
        }

        public override string ToString()
        {
            return this.Instructions.First().ToString();
        }
    }
}
