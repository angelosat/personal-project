using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI
{
    public class AIJob
    {
        public enum States { Default, GeneratingPlan, CalculatingPaths, Ready, Inaccessible }
        public States State;

        public List<AIInstruction> Instructions = new List<AIInstruction>();
        int Step;
        public GameObject Entity;
        public GameObject EvaluatedBy;
        bool Cancelled = false;
        public AIJob Source;
        public AILabor Labor;
        string _Description;
        
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
            if (i.Target.Type == TargetType.Entity)
                if (Components.AI.AIState.IsItemReserved(i.Target.Object))
                    throw new Exception();
            this.Instructions.Add(i);
        }
        internal void NextStep()
        {
            //this.FinishedInstructions.Enqueue(this.Instructions.Dequeue());
            this.Step++;
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
        internal void Cancel()
        {
            this.Cancelled = true;
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
                return this.Entity == null & this.EvaluatedBy == null;
            }
        }

        public override string ToString()
        {
            return this.Instructions.Last().ToString() + (this.Instructions.Count > 1 ? " +" + (this.Instructions.Count - 1).ToString() : "");
        }

        internal void Release()
        {
            this.Entity = null;
        }

        //public void Write(BinaryWriter w)
        //{
        //    var goal = this.Instructions.First();
        //    goal.Target.Write(w);
        //    w.Write(goal.Interaction.Name);
        //}
        //public void Read(BinaryReader r)
        //{
        //    AIInstruction 
        //}
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
