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
    class TaskBehaviorHaulToStockpileNew : BehaviorPerformTask
    {
        public const TargetIndex ItemInd = TargetIndex.A, StorageInd = TargetIndex.B;
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnForbidden(ItemInd);

            bool failCollecting()
            {
                var map = this.Actor.Map;
                var o = this.Task.GetTarget(ItemInd).Object;
                foreach (var destination in this.Task.GetTargetQueue(StorageInd))
                    if (!destination.IsValidStorage(map, o))
                        return true;
                return false;
            }

            var gotohaul = new BehaviorGetAtNewNew(ItemInd).FailOn(failCollecting);
            yield return gotohaul;
            //yield return new BehaviorInteractionNew(ItemInd, () =>
            //    new Haul(this.Actor.CurrentTask.GetAmount(ItemInd))
            //).FailOn(failCollecting);
            yield return BehaviorHaulHelper.StartCarrying(ItemInd);
            yield return BehaviorHaulHelper.FindNearbyHaulOpportunity(gotohaul, ItemInd).FailOnNotCarrying();


            bool deliverFail()
            {
                var o = this.Actor.GetHauled();
                if (o == null)
                    return true;
                var fail = !HaulHelper.IsValidStorage(this.Task.GetTarget(StorageInd), this.Actor.Map, o);
                if (fail)
                {
                    this.Actor.StopPathing();
                }
                return fail;
            }
            //var gotoStorage = new BehaviorGetAtNewNew(StorageInd, 1).FailOn(deliverFail);
            //yield return gotoStorage;
            //yield return BehaviorHaulHelper.DropInStorage(gotoStorage, StorageInd).FailOn(deliverFail);

            var gotoStorage = new BehaviorGetAtNewNew(StorageInd);//.FailOn(deliverFail);
            var findNextStorage = BehaviorHaulHelper.JumpIfNextStorageFound(gotoStorage, StorageInd);
            gotoStorage.JumpIf(deliverFail, findNextStorage);

            yield return gotoStorage;
            yield return BehaviorHaulHelper.DropInStorage(StorageInd).JumpIf(deliverFail, findNextStorage);
            yield return findNextStorage;
        }

        //protected IEnumerable<Behavior> GetStepsOld()
        //{
        //    this.FailOnForbidden(ItemInd);

        //    bool failCollecting()
        //    {
        //        var map = this.Actor.Map;
        //        var o = this.Task.GetTarget(ItemInd).Object;
        //        foreach (var destination in this.Task.GetTargetQueue(StorageInd))
        //            if (!destination.IsValidStorage(map, o))
        //                return true;
        //        return false;
        //    }

        //    var extractHaulable = BehaviorHelper.ExtractNextTargetAmount(ItemInd);
        //    yield return extractHaulable;
        //    yield return new BehaviorGetAtNewNew(ItemInd).FailOn(failCollecting);//.While(IsValid);
        //    yield return new BehaviorInteractionNew(ItemInd, () =>
        //        new InteractionHaul(this.Actor.CurrentTask.GetAmount(ItemInd))
        //    ).FailOn(failCollecting);//.While(IsValid);
        //    yield return BehaviorHelper.JumpIfNextCarryStackable(extractHaulable, ItemInd, ItemInd);

        //    var extractDestination = BehaviorHelper.ExtractNextTargetAmount(StorageInd);

        //    Func<bool> deliverFail = () =>
        //    {
        //        var o = this.Actor.GetHauled();
        //        if (o == null)
        //            return false;
        //        return !HaulHelper.IsValidStorage(this.Task.GetTarget(StorageInd), this.Actor.Map, o);
        //    };
        //    yield return extractDestination;
        //    yield return new BehaviorGetAtNewNew(StorageInd).FailOn(deliverFail).FailOnNotCarrying();//.While(() => this.IsValid(position, amount));
        //    yield return new BehaviorInteractionNew(StorageInd, () =>
        //        new UseHauledOnTarget(this.Actor.CurrentTask.GetAmount(StorageInd))//(this.Actor.CurrentTask.AmountA)
        //    ).FailOn(deliverFail).FailOnNotCarrying();//.While(() => this.IsValid(position, amount));

        //    yield return BehaviorHelper.JumpIfMoreTargets(extractDestination, StorageInd);
        //}



        protected override bool InitExtraReservations()
        {
            //if (!base.InitBaseReservations())
            //    return false;
            var task = this.Task;
            var item = task.GetTarget(ItemInd);
            var reserveAmount = Math.Min(item.Object.StackSize, task.Count);
            var storageTarget = task.GetTarget(StorageInd);
            return
                //this.Actor.Reserve(item, reserveAmount) &&
                this.Actor.ReserveAsManyAsPossible(item, task.Count) &&
                this.Actor.Reserve(storageTarget, 1);// storageTarget.HasObject ? storageTarget.Object.StackSize : 1);


            //return this.ReserveAll();
          
        }

        private bool ReserveAll()
        {
            var task = this.Task;
            var actor = this.Actor;
            return
                task.ReserveAll(actor, TargetIndex.A) &&
                task.ReserveAll(actor, TargetIndex.B) &&
                task.ReserveAll(actor, TargetIndex.C);
            //foreach (var t in this.Task.TargetsA)
            //    if (!this.Actor.Reserve(this.Actor.CurrentTask, t))
            //        return false;
            //foreach (var t in this.Task.TargetsB)
            //    if (!this.Actor.Reserve(this.Actor.CurrentTask, t))
            //        return false;
            //return true;
        }

        
        //public override void CleanUp()
        //{
        //    this.Actor.Map.Town.StockpileManager.Unreserve(this.Actor);
        //}
    }
}
