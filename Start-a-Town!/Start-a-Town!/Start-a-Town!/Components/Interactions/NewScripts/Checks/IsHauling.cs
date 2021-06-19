using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.AI;
using Start_a_Town_.UI;
using Start_a_Town_.AI;

namespace Start_a_Town_.Components.Interactions
{
    public class IsHauling : ScriptTaskCondition
    {
        Func<GameObject, bool> Filter;
        Func<GameObject, TargetArgs, Func<GameObject, bool>> FilterGetter;
        public IsHauling()
        {
            this.FilterGetter = (a, t) => b => b != null;// PersonalInventoryComponent.GetHauling(a).Object != null;
        }
        public IsHauling(Func<GameObject, bool> filter)
            : base("IsHauling")
        {
            this.Filter = filter;
            this.FilterGetter = (a, t) => this.Filter;
        }
        public IsHauling(Func<GameObject, bool> filter, Func<GameObject, TargetArgs, Func<GameObject, bool>> getter)
            : base("IsHauling")
        {
            this.FilterGetter = getter;
            this.Filter = filter;
        }
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            var haulobj = PersonalInventoryComponent.GetHauling(actor).Object;
            //if (this.Getter == null)
            //    return this.Filter(haulobj);
            //else
                return this.FilterGetter(actor, target)(haulobj);
        }
        public override bool AITrySolve(GameObject agent, TargetArgs target, Components.AI.AIState state, List<AIInstruction> instructions)
        {
            if (Filter == null)
                return true;
            var nearbyItems = state.NearbyEntities;
            var item = (from i in nearbyItems
                        where !AIState.IsItemReserved(i)
                        //where this.Filter(i)
                        where this.FilterGetter(agent, target)(i)
                        select i).FirstOrDefault();
            if (item == null)
            {
                //instruction = null;
                return false;
            }
            var instruction = new AIInstruction(new TargetArgs(item), new PickUp());
            instructions.Add(instruction);
            return false;
        }
        //public override bool AITrySolve(GameObject agent, TargetArgs target, Components.AI.AIState state, out AIInstruction instruction)
        //{
        //    var nearbyItems = state.NearbyEntities;
        //    var item = (from i in nearbyItems
        //                where !AIState.IsItemReserved(i)
        //                //where this.Filter(i)
        //                where this.Getter(agent,target)(i)
        //                select i).FirstOrDefault();
        //    if (item == null)
        //    {
        //        instruction = null;
        //        return false;
        //    }
        //    instruction = new AIInstruction(new TargetArgs(item), new PickUp());
        //    return false;
        //}
    }
}
