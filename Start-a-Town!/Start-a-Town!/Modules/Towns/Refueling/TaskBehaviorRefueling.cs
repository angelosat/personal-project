using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class TaskBehaviorRefueling : BehaviorPerformTask
    {
        Vector3 RefualableGlobal => this.Task.GetTarget(DestinationIndex).Global;
        Entity Fuel => this.Task.GetTarget(SourceIndex).Object as Entity;
        public const TargetIndex DestinationIndex = TargetIndex.B, SourceIndex = TargetIndex.A;
        protected override IEnumerable<Behavior> GetSteps()
        {
            bool failOnInvalidRefuelable()
            {
                return !this.Actor.Map.GetBlockEntity(this.RefualableGlobal)?.GetComp<EntityCompRefuelable>()?.Accepts(this.Fuel) ?? true;
            };
            var extract = BehaviorHelper.ExtractNextTargetAmount(SourceIndex);
            yield return extract;
            //yield return BehaviorHelper.InteractInInventoryOrWorld(SourceIndex, () => new Haul());
            yield return new BehaviorGetAtNewNew(SourceIndex).FailOn(failOnInvalidRefuelable).FailOnForbidden(SourceIndex);
            yield return BehaviorHelper.StartCarrying(SourceIndex, SourceIndex).FailOn(failOnInvalidRefuelable).FailOnForbidden(SourceIndex);
            yield return BehaviorHelper.JumpIfMoreTargets(extract, SourceIndex);
            yield return new BehaviorGetAtNewNew(DestinationIndex).FailOnNotCarrying().FailOn(failOnInvalidRefuelable);
            yield return new BehaviorInteractionNew(DestinationIndex,  () => new UseHauledOnTarget()).FailOnNotCarrying().FailOn(failOnInvalidRefuelable);
            /*yield return BehaviorHelper.JumpIfMoreTargets(extract, SourceIndex); */// i jump here because the taskgiver finds items of multiple types. do i want this? answer: NO

            /// old with only one fuel item set in the task
            //yield return BehaviorHelper.InteractInInventoryOrWorld(TargetIndex.A, () => new Haul());
            //yield return new BehaviorGetAtNewNew(TargetIndex.B, 1);
            //yield return new BehaviorInteractionNew(TargetIndex.B, new UseHauledOnTarget());
        }
        protected override bool InitExtraReservations()
        {
            return
                this.Task.ReserveAll(this.Actor, SourceIndex) &&
                this.Task.Reserve(this.Actor, DestinationIndex);// &&
        }
    }
}
