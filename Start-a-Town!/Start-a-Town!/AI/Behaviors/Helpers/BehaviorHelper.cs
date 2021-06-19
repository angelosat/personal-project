using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    class BehaviorHelper
    {
        //static public BehaviorCustom ExtractNextTarget(int index)
        //{
        //    return ExtractNextTarget((TargetIndex)index);
        //}
        static public BehaviorCustom ExtractNextTarget(TargetIndex index)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
              {
                  var actor = bhav.Actor;
                  if (!actor.CurrentTask.NextTarget(index))
                      throw new Exception();
                  //if (!actor.CurrentTask.NextAmount(index))
                  //    throw new Exception();
              };
            return bhav;
        }
        //internal static Behavior SetTarget(TargetIndex a, Func<GameObject> targetGetter)
        //{
        //    var bhav = new BehaviorCustom();
        //    bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(a, targetGetter());
        //    return bhav;
        //}
        internal static Behavior SetTarget(TargetIndex a, GameObject value)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(a, value);
            return bhav;
        }
        internal static Behavior SetTarget(TargetIndex a, IntVec3 value)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(a, value);
            return bhav;
        }
        internal static Behavior SetTarget(TargetIndex a, (IMap map, IntVec3 value) position)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(a, new TargetArgs(position.map, position.value));
            return bhav;
        }
        internal static Behavior SetTarget(TargetIndex a, Func<TargetArgs> targetGetter)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(a, targetGetter());
            return bhav;
        }
        internal static Behavior CarryFromInventory(TargetIndex item)
        {
            return new BehaviorInteractionNew(item, () => new InteractionHaul());
        }
        internal static Behavior CarryFromInventoryAndReplaceTarget(TargetIndex item)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () => bhav.Actor.CurrentTask.SetTarget(item, bhav.Actor.Carried);
            return new BehaviorSequence(
                new BehaviorInteractionNew(item, () => new InteractionHaul()),
                bhav);
        }

        static public BehaviorCustom ExtractNextTargetAmount(TargetIndex index)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                var actor = bhav.Actor;
                if (!actor.CurrentTask.NextTarget(index))
                    throw new Exception();
                if (!actor.CurrentTask.NextAmount(index))
                    throw new Exception();
            };
            return bhav;
        }
        [Obsolete]
        static public BehaviorCustom JumpIfMoreTargets(Behavior jumpBhav, int index)
        {
            return JumpIfMoreTargets(jumpBhav, (TargetIndex)index);
        }
        static public BehaviorCustom JumpIfMoreTargets(Behavior jumpBhav, TargetIndex index)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                var actor = bhav.Actor;
                var task = actor.CurrentTask;
                if (task.GetTargetQueue(index)?.Any() ?? false)
                    actor.CurrentTaskBehavior.JumpTo(jumpBhav);
            };
            return bhav;
        }
        static public BehaviorCustom JumpIfTrue(Behavior jumpBhav, Func<bool> predicate)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                var actor = bhav.Actor;
                if(predicate())
                    actor.CurrentTaskBehavior.JumpTo(jumpBhav);
            };
            return bhav;
        }
        static public BehaviorCustom ToolNotRequired()
        {
            var bhav = new BehaviorCustom()
            {
                FailCondition = a => a.CurrentTask.Tool.HasObject,
                Label = "ToolNotRequired"
            };
            //bhav.FailCondition = a =>
            //{
            //    var result = a.CurrentTask.Tool.HasObject;
            //    if (result)
            //        a.NetNew.SyncReport($"{bhav} failed because {a.CurrentTask} doesn't require a tool");
            //    return result;
            //};
            return bhav;
        }
        [Obsolete]
        static public Behavior JumpIfNextCarryStackable(Behavior jumpBhav, int ind)
        {
            return JumpIfNextCarryStackable(jumpBhav, (TargetIndex)ind);
        }
        static public Behavior JumpIfNextCarryStackable(Behavior jumpBhav, TargetIndex ind)
        {
            var bhav = new BehaviorCustom
            {
                InitAction = () =>
                {
                    var actor = jumpBhav.Actor;
                    var carried = actor.Carried;
                    var task = actor.CurrentTask;
                    var targets = task.GetTargetQueue(ind);
                    if (!targets?.Any() ?? false)
                        return;
                    var nextTarget = targets[0].Object;// task.GetTarget(ind).Object;
                if (nextTarget.CanAbsorb(carried))
                        actor.CurrentTaskBehavior.JumpTo(jumpBhav);
                }
            };
            return bhav;
        }
        static public Behavior JumpIfNextCarryStackable(Behavior jumpBhav, TargetIndex ind, TargetIndex amountInd)
        {
            var bhav = new BehaviorCustom
            {
                InitAction = () =>
                {
                    var actor = jumpBhav.Actor;
                    var carried = actor.Carried;
                    var task = actor.CurrentTask;
                    var targets = task.GetTargetQueue(ind);
                    if (!targets?.Any() ?? false)
                        return;
                    var nextTarget = targets[0].Object;// task.GetTarget(ind).Object;
                var amounts = task.GetAmountQueue(amountInd);
                    var nextAmount = amounts[0];
                    if (carried.CanAbsorb(nextTarget, nextAmount))
                        actor.CurrentTaskBehavior.JumpTo(jumpBhav);
                //if (nextTarget.CanStackWith(carried, nextAmount))
                //    actor.CurrentTaskBehavior.JumpTo(jumpBhav);
            }
            };
            return bhav;
        }
        static public Behavior StartCarrying(TargetIndex index)
        {
            return new BehaviorInteractionNew(index, () => new InteractionHaul()).FailOnUnavailableTarget(index);
        }
        static public Behavior StartCarrying(AITask task, TargetIndex index, TargetIndex amountIndex)
        {
            return new BehaviorInteractionNew(index, () => new InteractionHaul(task.GetAmount(amountIndex))).FailOnUnavailableTarget(index);
            //var bhav = new BehaviorInteractionNew(index);///.FailOnUnavailableTarget(index);
            //bhav.InteractionFactory = () => new Haul(bhav.Task.GetAmount(amountIndex));
        }
        static public Behavior StartCarrying(TargetIndex index, TargetIndex amountIndex)
        {
            var bhav = new BehaviorInteractionNew(index);///.FailOnUnavailableTarget(index);
            bhav.InteractionFactory = () => new InteractionHaul(bhav.Actor.CurrentTask.GetAmount(amountIndex));
            return bhav;
        }
        static public Behavior PlaceCarried(TargetIndex index)
        {
            return new BehaviorInteractionNew(index, ()=> new UseHauledOnTarget());
        }

        static public Behavior MoveTo(TargetIndex targetIndex)
        {
            return new BehaviorGetAtNewNew(targetIndex);
        }
        static public Behavior MoveTo(TargetIndex targetIndex, PathingSync.FinishMode mode)
        {
            return new BehaviorGetAtNewNew(targetIndex, mode);
        }
        //static public Behavior MoveTo(TargetIndex targetIndex)//, bool urgent)
        //{
        //    return new BehaviorGetAtNewNew(targetIndex);//, urgent: urgent);
        //}
        //static public Behavior MoveTo(TargetIndex targetIndex, int range)
        //{
        //    return new BehaviorGetAtNewNew(targetIndex, range: range);
        //}
        //static public Behavior MoveTo(TargetIndex targetIndex, int range, bool urgent)
        //{
        //    return new BehaviorGetAtNewNew(targetIndex, range, urgent);
        //}

        /// <summary>
        /// Waits until an item that satisfies the conditions exists at the target location, then assigns that item as a task target with the specified index
        /// </summary>
        /// <param name="targetIndex"></param>
        /// <param name="global"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        static public Behavior WaitForItem(TargetIndex targetIndex, IntVec3 global, Func<GameObject, bool> condition)
        {
            var bhav = new BehaviorWait();
            bhav.EndCondition = () =>
            {
                var actor = bhav.Actor;
                var item = actor.Map.GetObjects(global).FirstOrDefault(condition);
                if (item == null)
                    return false;
                actor.CurrentTask.SetTarget(targetIndex, item);
                return true;
            };
            return bhav;
        }

        static public Behavior InteractInInventoryOrWorld(TargetIndex itemIndex, Func<Interaction> interactionFactory)
        {
            var bhav = new BehaviorSelector
            {
                Children = new List<Behavior>()
            {
                //AI.BehaviorHelper.ToolNotRequired(),
                //new BehaviorCustom()
                //{
                //    SuccessCondition = (a) =>
                //    {
                //        var tool = a.CurrentTask.GetTarget(itemIndex);
                //        var result = tool.Object != null && a.GetEquipmentSlot(GearType.Mainhand).Object == tool.Object; ;
                //        return result;
                //    },
                //    FailCondition = (a) => {
                //        var tool = a.CurrentTask.GetTarget(itemIndex);
                //        return tool.Object != null;
                //    }
                //},
                new BehaviorSequence(
                    new BehaviorSelector(
                        new BehaviorItemIsInInventory(itemIndex),
                        new BehaviorGetAtNewNew(itemIndex)),
                    new BehaviorInteractionNew(itemIndex, interactionFactory())//,
                                                                                //BehaviorReserve.Release(tool)
                    )
            }
            };
            return bhav;
        }

        //static public void GetOpportunisticNextTask(Behavior bhav, Designation desType)
        //{
        //    GetOpportunisticNextTask(bhav, desType);
        //}
        static public void GetOpportunisticNextTask(Behavior bhav, DesignationDef desType)
        {
            var actor = bhav.Actor;
            //var manager = actor.Map.Town.DiggingManager;
            //var jobs = manager.GetDiggableBy(actor)
            //    .Where(actor.CanReserve)
            //    .OrderByReachableRegionDistance(actor);

            var jobs = actor.Map.Town.DesignationManager.GetDesignations(desType)
                .Where(t => actor.CanReserve(t))
                .Where(t => actor.CanReach(t));

            if (!jobs.Any())
                return;
            var job = jobs.First();
            var newtarget = new TargetArgs(actor.Map, job);
            actor.CurrentTask.SetTarget(TaskBehaviorDiggingNewNew.MineInd, newtarget);
            actor.Reserve(newtarget, 1);
            actor.CurrentTaskBehavior.JumpTo(bhav);
        }
        
    }
}
