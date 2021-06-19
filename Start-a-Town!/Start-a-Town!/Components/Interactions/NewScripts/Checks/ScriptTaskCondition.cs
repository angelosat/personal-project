using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class TaskConditions
    {
        List<ScriptTaskCondition> Inner;
        public List<ScriptTaskCondition> GetChildren()
        {
            return new List<ScriptTaskCondition>() { this.Root };
            
            //return this.Inner;
        }
        public ScriptTaskCondition Root;
        /// <summary>
        /// TODO: why not return them nested as they are? i dont have to evaluate each and every one of them
        /// </summary>
        /// <returns></returns>
        public List<ScriptTaskCondition> GetConditions()
        {
            return new List<ScriptTaskCondition>() { this.Root };
            //var list = new List<ScriptTaskCondition>();
            //foreach(var item in this.Inner)
            //{
            //    item.GetChildren(list);

            //}
            //return list;
        }
        public List<ScriptTaskCondition> GetAllChildren()
        {
            var list = new List<ScriptTaskCondition>();
            this.Root.GetChildren(list);
            return list;
        }
       
        public TaskConditions(AnyCheck any)
        {
            this.Root = any;
        }
        public TaskConditions(AllCheck all)
        {
            this.Root = all;
        }
        public TaskConditions()
        {
            this.Root = new AllCheck();
        }
        //public TaskConditions(params ScriptTaskCondition[] conditions)
        //{
        //    this.Inner = new List<ScriptTaskCondition>(conditions);
        //}
        public bool Evaluate(GameObject actor, TargetArgs target)
        {
            return this.Root.Condition(actor, target);
            //foreach (var item in this.Inner)
            //    if (!item.Condition(actor, target))
            //    {
            //        //actor.Net.EventOccured(item.ErrorEvent, actor, item);

            //        ////actor.Net.EventOccured(Message.Types.InteractionFailed, actor, item);
            //        return false;
            //    }
            //return true;
        }
        public bool Evaluate(GameObject actor, TargetArgs target, out ScriptTaskCondition failed)
        {
            foreach (var item in this.Inner)
                if (!item.Condition(actor, target))
                {
                    //actor.Net.EventOccured(item.ErrorEvent, actor, item);
                    failed = item;
                    ////actor.Net.EventOccured(Message.Types.InteractionFailed, actor, item);
                    return false;
                }
            failed = null;
            return true;
        }
        public ScriptTaskCondition GetFailedCondition(GameObject actor, TargetArgs target)
        {
            foreach (var item in this.Root.Children)
            {
                var failed = item.GetFailedCondition(actor, target);
                if (failed != null)
                    return failed;
            }
            return null;
            //foreach (var item in this.Inner)
            //{
            //    var failed = item.GetFailedCondition(actor, target);
            //    if (failed != null)
            //        return failed;
            //}
            //return null;
        }
        //public bool Evaluate(GameObject actor, TargetArgs target)
        //{
        //    return this.GetFailedCondition(actor, target) == null;
        //}
        //public ScriptTaskCondition GetFailedCondition(GameObject actor, TargetArgs target)
        //{
        //    foreach (var item in this.Inner)
        //        if (item.Condition(actor, target) !=null)
        //        {
        //            return item;
        //        }
        //    return null;
        //}
        public void GetTooltip(UI.Control tooltip)
        {
            foreach (var item in this.Inner)
                item.GetTooltip(tooltip);
        }
    }

    public class ScriptTaskCondition
    {
        public virtual void GetChildren(List<ScriptTaskCondition> list)
        {
            list.Add(this);
            foreach (var item in this.Children)
                item.GetChildren(list);
        }
        public List<ScriptTaskCondition> Children = new();
        Func<GameObject, TargetArgs, bool> Func { get; set; }
        public virtual string Name { get; set; }
        public Message.Types ErrorEvent { get; set; }
        protected ScriptTaskCondition() : this("untitled") { }
        protected ScriptTaskCondition(string name)
        {
            this.Name = name;
            this.Func = (a,b) => true;
            this.ErrorEvent = Message.Types.Default;
        }
        public ScriptTaskCondition(string name, Func<GameObject, TargetArgs, bool> condition, Message.Types errorEvent = Message.Types.InvalidTarget)
        {
            this.Name = name;
            this.Func = condition;
            this.ErrorEvent = errorEvent;
        }
        public virtual bool Condition(GameObject actor, TargetArgs target)
        {
            return this.Func(actor, target);
        }
        //public virtual ScriptTaskCondition Condition(GameObject actor, TargetArgs target)
        //{
        //    //return this.Func(actor, target);
        //    return this.Func(actor, target) ? null : this; //return itself if condition fails, otherwise null
        //}
        public virtual ScriptTaskCondition GetFailedCondition(GameObject actor, TargetArgs target)
        {
            //return this.Func(actor, target);
            //return this.Func(actor, target) ? null : this; //return itself if condition fails, otherwise null
            return this.Condition(actor, target) ? null : this; //return itself if condition fails, otherwise null
        }
        public virtual void GetTooltip(UI.Control tooltip)
        { }

        public virtual AIInstruction AIGetPreviousStep(GameObject agent, TargetArgs target, AIState state)
        {
            return null;
        }

        /// <summary>
        /// Returns true if the interaction can be performed, false otherwise. If it can't be performed, 
        /// and a solution to satisfy the failed condition is found,
        /// an AIInstruction containing the solution is output. Null if no solution found.
        /// </summary>
        /// <param name="agent"></param>
        /// <param name="target"></param>
        /// <param name="state"></param>
        /// <param name="solution"></param>
        /// <returns></returns>
        //public virtual bool AITrySolve(GameObject agent, TargetArgs target, AIState state, out AIInstruction solution)
        //{
        //    solution = null;
        //    return true;
        //}
        public virtual bool AITrySolve(GameObject agent, TargetArgs target, AIState state, List<AIInstruction> solution)
        {
            return true;

            //AIInstruction step;
            //var ok = this.AITrySolve(agent, target, state, out step);
            //if (step != null)
            //{
            //    solution.Add(step);
            //    return true;
            //}
            //return ok;
        }
        public virtual void AIInit(GameObject agent, TargetArgs target, AIState state)
        {

        }

        public virtual void AIFindSolution(GameObject agent, TargetArgs target, AIState state)
        {

        }
    }

}
