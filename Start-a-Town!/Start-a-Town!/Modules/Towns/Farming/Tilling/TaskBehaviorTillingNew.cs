using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;

namespace Start_a_Town_.Towns.Farming
{
    class TaskBehaviorTillingNew : BehaviorPerformTask
    {
        //public const TargetIndex ToolInd = 0;
        public const TargetIndex TargetInd = TargetIndex.A;
        TargetArgs Target { get { return this.Task.GetTarget(TargetInd); } }
        protected override IEnumerable<Behavior> GetSteps()
        {
            bool fail() => !this.Actor.Map.Town.FarmingManager.IsTillable(Target.Global);
            //if (this.Task.Tool.HasObject)
            yield return new BehaviorGrabTool().FailOnForbidden(TargetIndex.Tool);//.FailOn(fail);
            yield return new BehaviorGetAtNewNew(TargetInd);//.FailOn(fail);//.While(() => this.IsTargetValid(parent, target.Global));
            yield return new BehaviorInteractionNew(TargetInd, new InteractionTilling());//.FailOn(fail);//.While(() => this.IsTargetValid(parent, target.Global));
        }
       

        protected override bool InitExtraReservations()
        {
            return this.Actor.Reserve(this.Task.TargetA, 1);
        }
        
      
    }
}
