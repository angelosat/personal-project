using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

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
            //this.FailOnDisposed(MaterialID);
            bool failCollecting()
            {
                //return false;
                var map = this.Actor.Map;
                var o = Material.Object;
                foreach (var d in this.Task.GetTargetQueue(DestinationID))
                    if (!d.IsValidHaulDestination(map, Material.Object))
                        return true;
                return false;
            }
            var extractHaulable =  BehaviorHelper.ExtractNextTargetAmount(MaterialID);
            yield return extractHaulable;
            yield return new BehaviorGetAtNewNew(MaterialID).FailOn(failCollecting);//.While(IsValid);
            yield return new BehaviorInteractionNew(MaterialID, () =>
                new InteractionHaul(this.Actor.CurrentTask.GetAmount(MaterialID))
            ).FailOn(failCollecting);//.While(IsValid);
            yield return BehaviorHelper.JumpIfNextCarryStackable(extractHaulable, MaterialID);

            var extractDestination = BehaviorHelper.ExtractNextTargetAmount(DestinationID);
            yield return extractDestination;
            bool deliverFail()
            {
                var o = this.Actor.GetHauled();
                if (o == null)
                    return false;
                //return !HaulHelper.IsValidStorage(this.Task.GetTarget(DestinationID), this.Actor.Map, o);
                return !this.Destination.IsValidHaulDestination(this.Actor.Map, o);
            }
            yield return new BehaviorGetAtNewNew(DestinationID).FailOn(deliverFail);//.While(() => this.IsValid(position, amount));
            yield return new BehaviorInteractionNew(DestinationID, () =>
                new UseHauledOnTarget(this.Actor.CurrentTask.GetAmount(DestinationID))//(this.Actor.CurrentTask.AmountA)
            ).FailOn(deliverFail);//.While(() => this.IsValid(position, amount));
            yield return BehaviorHelper.JumpIfMoreTargets(extractDestination, DestinationID);
        }
        protected override bool InitExtraReservations()
        {
            return
                this.Task.ReserveAll(this.Actor, MaterialID) &&
                this.Task.GetTargetQueue(DestinationID).All(t => this.Actor.Reserve(t, 1));// this.Task.ReserveAll(this.Actor, DestinationID);

        }
        //protected override IEnumerable<Behavior> GetSteps()
        //{
        //    this.FailOnForbidden(MaterialID);
        //    //this.FailOnDisposed(MaterialID);
        //    bool failCollecting()
        //    {
        //        //return false;
        //        var map = this.Actor.Map;
        //        var o = Material.Object;
        //        foreach (var d in this.Task.GetTargetQueue(DestinationID))
        //            if (!d.IsValidHaulDestination(map, Material.Object))
        //                return true;
        //        return false;
        //    }
        //    yield return TaskHelper.NextTargetAmount(null, MaterialID);
        //    var gotoHaulable = new BehaviorGetAtNewNew(MaterialID, 1).FailOn(failCollecting);//.While(IsValid);
        //    yield return gotoHaulable;
        //    yield return new BehaviorInteractionNew(MaterialID, () =>
        //        new Haul(this.Actor.CurrentTask.GetAmount(MaterialID))
        //    ).FailOn(failCollecting);//.While(IsValid);

        //    var getNextTarget = TaskHelper.NextTargetAmount(gotoHaulable, MaterialID); 
        //    yield return getNextTarget;

        //    yield return TaskHelper.NextTargetAmount(null, DestinationID);
        //    bool deliverFail()
        //    {
        //        var o = this.Actor.GetCarried();
        //        if (o == null)
        //            return false;
        //        //return !HaulHelper.IsValidStorage(this.Task.GetTarget(DestinationID), this.Actor.Map, o);
        //        return !this.Destination.IsValidHaulDestination(this.Actor.Map, o);
        //    }
        //    var gotoDestination = new BehaviorGetAtNewNew(DestinationID, 1).FailOn(deliverFail);//.While(() => this.IsValid(position, amount));
        //    yield return gotoDestination;
        //    yield return new BehaviorInteractionNew(DestinationID, () =>
        //        new UseHauledOnTarget(this.Actor.CurrentTask.GetAmount(DestinationID))//(this.Actor.CurrentTask.AmountA)
        //    ).FailOn(deliverFail);//.While(() => this.IsValid(position, amount));
        //    yield return TaskHelper.NextTargetAmount(gotoDestination, DestinationID);
        //}

        //protected IEnumerable<Behavior> GetStepsOld()
        //{
        //    var seedcount = this.Task.TargetQueues[0].Count;
        //    for (int i = 0; i < seedcount; i++)
        //    {
        //        var seed = this.Task.TargetQueues[0][i];
        //        var amount = this.Task.AmountQueues[0][i];
        //        yield return new BehaviorGetAtNewNew(seed, 1);
        //        yield return new BehaviorInteractionNew(seed, new Haul(amount));
        //    }

        //    var sowcount = this.Task.TargetQueues[1].Count;
        //    for (int i = 0; i < sowcount; i++)
        //    {
        //        var sow = this.Task.TargetQueues[1][i];
        //        var amount = this.Task.AmountQueues[1][i];
        //        yield return new BehaviorGetAtNewNew(sow, 1);
        //        yield return new BehaviorInteractionNew(sow, new UseHauledOnTarget(1));
        //    }
        //}

        bool IsTargetValid(GameObject actor, Vector3 target)
        {
            var manager = actor.Map.Town.FarmingManager;
            return manager.IsSowable(target);
        }
    }
}
