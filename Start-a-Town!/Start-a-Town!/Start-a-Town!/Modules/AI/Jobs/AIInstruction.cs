﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.AI
{
    public class AIInstruction
    {
        public override string ToString()
        {
            return this.Interaction.Name + ": " + this.Target.ToString();
        }

        //public string Interaction;
        public Interaction Interaction;
        public TargetArgs Target;
        public bool Completed;

        //public AIInstruction(TargetArgs target, string work)
        public AIInstruction(TargetArgs target, Interaction work)
        {
            this.Target = target;
            if (this.Target.Type == TargetType.Entity)
                if (this.Target.Object == null)
                    throw new Exception();
            this.Interaction = work.Clone() as Interaction;
        }

        public AITarget GetTargetRange()
        {
            if (this.Interaction == null)
                return new AITarget(this.Target, 0, 0.1f);
            var range = this.Interaction.Conditions.GetConditions().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
            if (range != null)
                return new AITarget(this.Target, range.Min, range.Max);
            else return null;
        }

        //List<AIInstruction> GetRequiredSteps(GameObject agent, TargetArgs target, Components.AI.AIState state)
        //{
        //    var conditions = this.Interaction.Conditions.GetConditions();
        //    var steps = new List<AIInstruction>();
        //    foreach (var cond in conditions)
        //    {
        //        // if condition evaluates, continue, otherwise try to get instruction and if it's null, return
        //        if (cond.Condition(agent, target))// == null)
        //            continue;
        //        AIInstruction instruction;
        //        if (cond.AITrySolve(agent, target, state, out instruction))
        //            continue;

        //        // if the condition didn't evaluate and it did output null, it didn't find a solution so return null
        //        if (instruction == null)
        //            return null;
        //        steps.Add(instruction);
        //    }
        //    //steps.Add(new AIInstruction(target, this.Interaction));
        //    return steps;
        //}

        //public bool CanExecuteOld(GameObject agent, TargetArgs target, Components.AI.AIState state, out List<AIInstruction> steps)
        //{
        //    var conditions = this.Interaction.Conditions.GetConditions();
        //    steps = new List<AIInstruction>();
        //    foreach (var cond in conditions)
        //    {
        //        // if condition evaluates, continue, otherwise try to get instruction and if it's null, return
        //        if (cond.Condition(agent, target))// == null)
        //            continue;
        //        AIInstruction instruction;
        //        if (cond.AITrySolve(agent, target, state, out instruction))
        //            continue;

        //        // if the condition didn't evaluate and it did output null, it didn't find a solution so return null
        //        if (instruction == null)
        //            return false;
        //        steps.Add(instruction);
        //    }
        //    //steps.Add(new AIInstruction(target, this.Interaction));
        //    return true;// steps.Count == 0;
        //}

        /// <summary>
        /// TODO: make it non-recursive
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="target"></param>
        /// <param name="state"></param>
        /// <param name="steps"></param>
        /// <returns></returns>
        //public bool FindPlanOld(GameObject agent, TargetArgs target, Components.AI.AIState state, List<AIInstruction> steps)
        //{
        //    var conditions = this.Interaction.Conditions.GetConditions();
        //    //steps = new List<AIInstruction>();
        //    foreach (var cond in conditions)
        //    {
        //        // if condition evaluates, continue, otherwise try to get instruction and if it's null, return
        //        if (cond.Condition(agent, target))// == null)
        //            continue;
        //        AIInstruction instruction;
        //        if (cond.AITrySolve(agent, target, state, out instruction))
        //            continue;

        //        // if the condition didn't evaluate and it did output null, it didn't find a solution so return null
        //        if (instruction == null)
        //            return false;
        //        if (!instruction.FindPlanOld(agent, target, state, steps))
        //            return false;
        //        steps.Add(instruction);

        //    }
        //    //steps.Add(new AIInstruction(target, this.Interaction));
        //    return true;// steps.Count == 0;
        //}

        public bool FindPlan(GameObject agent, TargetArgs target, Components.AI.AIState state, List<AIInstruction> steps)
        {
            var successfulConditions = new List<ScriptTaskCondition>();
            var root = this.Interaction.Conditions.Root;
            if (root.Condition(agent, target))
                return true;
            return root.AITrySolve(agent, target, state, steps);

            //var conditions = this.Interaction.Conditions.GetChildren();//.GetConditions();
            //foreach (var cond in conditions)
            //{
            //    // if condition evaluates, continue, otherwise try to get instruction and if it's null, return
            //    if (cond.Condition(agent, target))
            //    {
            //        successfulConditions.Add(cond);
            //        continue;
            //    }
            //    AIInstruction instruction;
            //    if (cond.AITrySolve(agent, target, state, out instruction))
            //        continue;

            //    // if the condition didn't evaluate and it did output null, it didn't find a solution so return null
            //    if (instruction == null)
            //        return false;
            //    if (!instruction.FindPlan(agent, target, state, steps))
            //        return false;
            //    steps.Add(instruction);
            //}
            //return true;
        }

        public void Write(BinaryWriter w)
        {
            this.Target.Write(w);
            w.Write(this.Interaction.Name);
        }
        public void Read(IObjectProvider net, BinaryReader r)
        {
            this.Target = TargetArgs.Read(net, r);
            this.Interaction = this.Target.GetInteraction(net, r.ReadString());
        }
        public AIInstruction(IObjectProvider net, BinaryReader r)
        {
            this.Read(net, r);
        }
    }
}
