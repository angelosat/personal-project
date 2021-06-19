using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.Towns.Digging;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    class TaskBehaviorDiggingNewNew : BehaviorPerformTask
    {
        //const int ToolInd = 1;
        public const TargetIndex MineInd = TargetIndex.A;// 0;
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoDesignation(MineInd, DesignationDef.Mine);
            this.FailOnCellStandedOn(MineInd);
            yield return new BehaviorGrabTool();
            //var reserve = BehaviorReserve.Reserve(MineInd);
            //yield return reserve;

            var gotomine = new BehaviorGetAtNewNew(MineInd);//.While(() => this.IsTargetValid(parent, target.Global));
            yield return gotomine;
            // TODO: check if another npc is standing on the target block to be digged
            yield return new BehaviorInteractionNew(MineInd, () => this.Actor.Map.GetBlockMaterial(this.Task.GetTarget(0).Global).GetSkillToExtract().GetInteraction());//.While(() => this.IsTargetValid(parent, target.Global));
        
            // no need to find next task here, just finish and let taskgiver give next one
            //yield return new BehaviorCustom()
            //{
            //    InitAction = () => BehaviorHelper.GetOpportunisticNextTask(gotomine, MineInd, Designation.Mine)
            //};
        }

        

        protected override bool InitExtraReservations()
        {
            //if (!this.Actor.Reserve(this.Task.TargetA))
            //    return false;
            var global = this.Task.GetTarget(MineInd);
            return this.Actor.Reserve(global, 1);

            //if(this.Task.TargetB != null && this.Task.TargetB.HasObject)
            //    if (!this.Actor.Reserve(this.Task.TargetB))
            //        return false;
            //return true;
        }

    }
}
