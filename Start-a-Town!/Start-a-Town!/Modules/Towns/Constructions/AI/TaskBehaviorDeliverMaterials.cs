﻿using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorDeliverMaterials : BehaviorPerformTask
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
                    {
                        "failed collecting".ToConsole();
                        return true;
                    }
                return false;
            }

            var extractMaterial = BehaviorHelper.ExtractNextTargetAmount(MaterialID);
            yield return extractMaterial;
            yield return new BehaviorGetAtNewNew(MaterialID).FailOn(failCollecting);
            yield return BehaviorHaulHelper.StartCarrying(MaterialID).FailOn(failCollecting);
            yield return BehaviorHelper.JumpIfNextCarryStackable(extractMaterial, MaterialID, MaterialID);
            var extractDestination = BehaviorHelper.ExtractNextTargetAmount(DestinationID);
            yield return extractDestination;
            bool deliverFail()
            {
                var o = this.Actor.GetHauled();
                if (o == null)
                    return false;
                var val = !this.Destination.IsValidHaulDestination(this.Actor.Map, o);
                if (val)
                    "invalid haul destination".ToConsole();
                return val;
            }

            var gotoStorage = new BehaviorGetAtNewNew(DestinationID).FailOn(deliverFail);
            yield return gotoStorage;
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