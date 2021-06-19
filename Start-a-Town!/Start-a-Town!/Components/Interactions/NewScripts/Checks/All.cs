using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class AllCheck : ScriptTaskCondition
    {
        //List<ScriptTaskCondition> Children { get; set; }
        //ScriptTaskCondition Failed;
        public AllCheck(params ScriptTaskCondition[] conditions)
            : base("All")
        {
            this.Children = new List<ScriptTaskCondition>(conditions);
            this.ErrorEvent = Message.Types.InvalidTarget;
        }
        // TODO: return failed condition instaed of bool, and don't trigger interaction failed event here because i may just simply want to evaluate outside of actual interaction
        public override bool Condition(GameObject a, TargetArgs t)
        {
            foreach (var item in this.Children)
                if (!item.Condition(a, t))
                {
                    // DON"T post an event here cause this method is not only called when performing the interaction, but also when evaluating
                    //a.Net.EventOccured(Message.Types.InteractionFailed, a, item);
                    return false;
                }
            return true;
        }
        public override ScriptTaskCondition GetFailedCondition(GameObject a, TargetArgs t)
        {
            foreach (var item in this.Children)
            {
                var innerItem = item.GetFailedCondition(a, t);
                if (innerItem != null)
                    return innerItem;
            }
            return null;

            //foreach (var item in this.Children)
            //    if (!item.Condition(a, t))
            //    {
            //        return item;
            //    }
            //return null;
        }
        
        //public override ScriptTaskCondition Condition(GameObject a, TargetArgs t)
        //{
        //    foreach (var item in this.Children)
        //        if (item.Condition(a, t) != null)
        //        {
        //            return item;
        //            //a.Net.EventOccured(Message.Types.InteractionFailed, a, item);
        //        }
        //    return null;
        //}
        public override void GetChildren(List<ScriptTaskCondition> list)
        {
            foreach (var item in this.Children)
                item.GetChildren(list);
        }

        public override void AIInit(GameObject agent, TargetArgs target, AIState state)
        {
            foreach (var item in this.Children)
                item.AIInit(agent, target, state);
        }
        //public override bool AITrySolve(GameObject agent, TargetArgs target, AI.AIState state, out Start_a_Town_.AI.AIInstruction solution)
        //{
        //    return base.AITrySolve(agent, target, state, out solution);
        //}
        public override bool AITrySolve(GameObject agent, TargetArgs target, AIState state, List<AIInstruction> solution)
        {
            //foreach (var cond in this.Children)
            //{
            //    if (!cond.Condition(agent, target))
            //        if (!cond.AITrySolve(agent, target, state, solution))
            //            return false;
            //}

            //-----

            foreach (var cond in this.Children)
            {
                if (!cond.Condition(agent, target))
                {
                    //AIInstruction instr;
                    List<AIInstruction> instructions = new List<AIInstruction>();
                    if (!cond.AITrySolve(agent, target, state, instructions)) // if condition isn't met, try to find a solution
                    {
                        if (instructions.Count == 0)
                            return false;

                        foreach(var instr in instructions)
                        { 
                        // TODO: why does it return false if a solution is found??? 
                        // if a solution has been found, recursively check solvability of said solution
                        // if it's solvable, add the instruction to the list
                        //if (instr != null)
                        //{
                            if (instr.Interaction.Conditions.Root.AITrySolve(agent, target, state, solution))
                                solution.Add(instr);
                        //}
                        //else
                        //    // if returned false and no solution found, instruction is unsolvable, so return false
                        //    return false;
                        }
                    }
                }
            }
            return true;

            //foreach (var cond in this.Children)
            //{
            //    if (!cond.Condition(agent, target))
            //    {
            //        AIInstruction instr;
            //        if (!cond.AITrySolve(agent, target, state, out instr)) // if condition isn't met, try to find a solution
            //        {
            //            // TODO: why does it return false if a solution is found??? 
            //            // if a solution has been found, recursively check solvability of said solution
            //            // if it's solvable, add the instruction to the list
            //            if (instr != null)
            //            {
            //                if (instr.Interaction.Conditions.Root.AITrySolve(agent, target, state, solution))
            //                    solution.Add(instr);
            //            }
            //            else
            //                // if returned false and no solution found, instruction is unsolvable, so return false
            //                return false;
            //        }
            //    }
            //}
            //return true;
        }
    }
}
