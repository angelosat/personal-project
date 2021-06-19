using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    class TaskBehaviorDeconstruct : BehaviorPerformTask
    {
        public const TargetIndex DeconstructInd = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            //this.FailOnForbidden(DeconstructInd);
            this.FailOnNoDesignation(DeconstructInd, DesignationDef.Deconstruct);
            this.FailOnCellStandedOn(DeconstructInd);

            yield return new BehaviorGrabTool();

            //var reserve = BehaviorReserve.Reserve(DeconstructInd);
            //yield return reserve;
            yield return new BehaviorGetAtNewNew(DeconstructInd);//.While(() => this.IsTargetValid(parent, target.Global));
            yield return new BehaviorInteractionNew(DeconstructInd, ()=>new InteractionDeconstruct());//.While(() => this.IsTargetValid(parent, target.Global));
            //yield return new BehaviorCustom()
            //{
            //    InitAction = () =>
            //    {
            //        //TaskGiverDigging.GetOpportunisticNextTask(reserve, DeconstructInd);
            //        BehaviorHelper.GetOpportunisticNextTask(reserve, DeconstructInd, Designation.Deconstruct);
            //    }
            //};
        }
        protected override bool InitExtraReservations()
        {
            return this.Actor.Reserve(this.Task.GetTarget(DeconstructInd), 1);
        }
        //public override bool InitReservations()
        //{
        //    //if (!this.Actor.Reserve(this.Task.TargetA))
        //    //    return false;
        //    base.InitReservations();
        //    var t = this.Task.GetTarget(ToolInd);
        //    if (t != null && t.HasObject)
        //        if (!this.Actor.Reserve(t))
        //            return false;
        //    return true;
        //}
    }
}
