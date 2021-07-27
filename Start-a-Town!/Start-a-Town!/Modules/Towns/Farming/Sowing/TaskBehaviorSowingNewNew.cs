using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorSowingNewNew : BehaviorPerformTask
    {
        TargetArgs Material { get { return this.Task.GetTarget(MaterialID); } }
        TargetArgs Destination { get { return this.Task.GetTarget(DestinationID); } }
        public const TargetIndex MaterialID = TargetIndex.A, DestinationID = TargetIndex.B;
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnForbidden(MaterialID);
            bool failCollecting()
            {
                var map = this.Actor.Map;
                var o = Material.Object;
                foreach (var d in this.Task.GetTargetQueue(DestinationID))
                    if (!d.IsValidHaulDestination(map, Material.Object))
                        return true;
                return false;
            }
            var extractHaulable =  BehaviorHelper.ExtractNextTargetAmount(MaterialID);
            yield return extractHaulable;
            yield return new BehaviorGetAtNewNew(MaterialID).FailOn(failCollecting);
            yield return new BehaviorInteractionNew(MaterialID, () =>
                new InteractionHaul(this.Actor.CurrentTask.GetAmount(MaterialID))
            ).FailOn(failCollecting);
            yield return BehaviorHelper.JumpIfNextCarryStackable(extractHaulable, MaterialID);

            var extractDestination = BehaviorHelper.ExtractNextTargetAmount(DestinationID);
            yield return extractDestination;
            bool deliverFail()
            {
                var o = this.Actor.Hauled;
                if (o == null)
                    return false;
                return !this.Destination.IsValidHaulDestination(this.Actor.Map, o);
            }
            yield return new BehaviorGetAtNewNew(DestinationID).FailOn(deliverFail);
            yield return new BehaviorInteractionNew(DestinationID, () =>
                new UseHauledOnTarget(this.Actor.CurrentTask.GetAmount(DestinationID))
            ).FailOn(deliverFail);
            yield return BehaviorHelper.JumpIfMoreTargets(extractDestination, DestinationID);
        }
        protected override bool InitExtraReservations()
        {
            return
                this.Task.ReserveAll(this.Actor, MaterialID) &&
                this.Task.GetTargetQueue(DestinationID).All(t => this.Actor.Reserve(t, 1));
        }
    }
}
