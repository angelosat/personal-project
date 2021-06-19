using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.AI.Behaviors;
namespace Start_a_Town_
{
    static class TaskHelper
    {
        static public AITask TryEquip(Actor actor, Func<GameObject, bool> condition)
        {
            var item = (from obj in actor.Map.GetObjects()
                        where condition(obj)
                        where actor.CanReserve(obj)
                        orderby Vector3.DistanceSquared(obj.Global, actor.Global)
                        select obj).FirstOrDefault();
            if (item == null)
                return null;

            return new AITask(typeof(BehaviorEquipItemNew)) { TargetA = new TargetArgs(item) };
        }
        static public AITask TryStoreEquipped(Actor actor, GearType.Types type)
        {
            return TryStoreEquipped(actor, GearType.Dictionary[type]);
        }
        static public AITask TryStoreEquipped(Actor actor, GearType type)
        {
            var mainhand = actor.GetEquipmentSlot(type);

            if (mainhand != null)
            {
                var possesions = actor.GetPossesions();
                if (possesions.Contains(mainhand))
                {
                    return new AITask(typeof(BehaviorUnequip)) { TargetA = new TargetArgs(mainhand) };
                }
                else
                {
                    //var haul = Stockpiles.StockpileAIHelper.HaulToStockpile(actor, mainhand.Object);
                    // TODO: make a single drop item behavior that accepts a target item parameters and checks both inventory and equipment slots to find it and drop it
                    return new AITask(typeof(BehaviorCarryItem)) { TargetA = new TargetArgs(mainhand) };

                    //return new AITask(typeof(BehaviorDropEquippedNew)) { Target = new TargetArgs(mainhand.Object) };
                }
            }
            else
            {
                var hauling = actor.GetHauled();
                if (hauling != null)
                {
                    var destination = StockpileAIHelper.GetBestHaulDestination(actor, hauling);
                    if (destination.Type != TargetType.Null)
                    {
                        // TODO: make a single drop item behavior that accepts a target item parameters and checks both inventory and equipment slots to find it and drop it
                        return new AITask(typeof(BehaviorDropCarried)) { TargetA = destination };
                    }
                }
            }

            return null;
        }
        static public AITask TryHaulAside(Actor actor, Entity item)
        {
            if (!item.IsHaulable)
                return null;
            //var unreservedCount = actor.GetUnreservedAmount(item);
            //if (unreservedCount == 0)
            //    return null;
            if (!actor.TryGetUnreservedAmount(item, out var unreservedCount))
                return null;
            if (!HaulHelper.TryFindNearbyPlace(actor, item, out var place))
                return null;

            var count = Math.Min(unreservedCount, actor.GetHaulStackLimitFromEndurance(item));

            return new AITask(TaskDefOf.HaulAside, new TargetArgs(item), place) { Count = count };
        }
        static public bool TryHaulAside(Actor actor, Vector3 global, out AITask task)
        {
            task = null;
            var map = actor.Map;
            var objectsAbove = map.GetObjects(global);
            if (!objectsAbove.Any())
                return true;
            if (objectsAbove.SingleOrDefault(o => o == actor) != null)
                return true;
            foreach (var objAbove in objectsAbove)
            {
                if (objAbove == actor)
                    continue;
                var haulAsideTask = TaskHelper.TryHaulAside(actor, objAbove as Entity);
                if (haulAsideTask != null)
                {
                    task = haulAsideTask;
                    return true;
                }
            }
            return false; // if there are any items above the cell and a haulasidetask hasn't been returned, it means that there are immovable objects
        }
        /// <summary>
        /// Looks in actor's inventory or in map for item that satisfies provided condition
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="condition"></param>
        /// <returns></returns>
        static public TargetArgs FindItemAnywhere(Actor actor, Func<GameObject, bool> condition)
        {
            var inventoryitem = actor.InventoryFirst(condition);
            if (inventoryitem != null)
            {
                var invitemtarget = new TargetArgs(inventoryitem);
                return invitemtarget;
            }
            else
            {
                var nearbyItems = actor.Map.GetObjects();
                //var item = (from i in nearbyItems
                //            where condition(i)
                //            where actor.CanReserve(i)
                //            orderby Vector3.DistanceSquared(actor.Global, i.Global)
                //            select i).FirstOrDefault();
                var item = nearbyItems
                    .Where(i => condition(i) && actor.CanReserve(i))
                    .OrderByReachableRegionDistance(actor)
                    .FirstOrDefault();
                    
                if (item == null)
                    return TargetArgs.Null;
                return new TargetArgs(item);
            }
        }
        static public bool TryFindItemAnywhere(Actor actor, Func<GameObject, bool> condition, out TargetArgs target)
        {
            target = FindItemAnywhere(actor, condition);
            return target != TargetArgs.Null;
        }

        //[Obsolete]
        //static public Behavior FailOnDisposed(this Behavior bhav, int targetInd)
        //{
        //    //var target = bhav.Task.GetTarget(targetInd);
        //    bhav.AddEndCondition(() =>
        //    {
        //        var task = bhav.Actor.CurrentTask;
        //        if (task.GetTarget(targetInd).Object.IsDisposed)
        //            return true;
        //        if (task.GetTargetQueue(targetInd).Any(t => t.Object.IsDisposed))
        //            return true;
        //        return false;
        //    });
        //    return bhav;
        //}
        static public Behavior FailOnNotCarrying(this Behavior bhav)
        {
            bhav.FailOn(() => bhav.Actor.Carried == null);
            return bhav;
        }
        static public Behavior FailOnForbidden(this Behavior bhav, TargetIndex targetInd)
        {
            bhav.FailOn(() =>
            {
                var task = bhav.Actor.CurrentTask;
                if (task.GetTarget(targetInd).IsForbidden)
                    return true;

                return false;
            });
            return bhav;
        }
        static public Behavior FailOnForbiddenQueue(this Behavior bhav, TargetIndex targetInd)
        {
            bhav.FailOn(() =>
            {
                var task = bhav.Actor.CurrentTask;
                
                if (task.GetTargetQueue(targetInd).Any(t => t.IsForbidden))
                    return true;
                return false;
            });
            return bhav;
        }
        //[Obsolete]
        //static public Behavior FailOnForbidden(this Behavior bhav, int targetInd)
        //{
        //    return FailOnForbidden(bhav, (TargetIndex)targetInd);
        //}
        static public Behavior FailOnUnavailableTarget(this Behavior bhav, TargetIndex targetInd)
        {
            bhav.FailOn(() =>
            {
                var task = bhav.Actor.CurrentTask;
                var t = task.GetTarget(targetInd);
                if (t.Object.IsDisposed)
                    return true;
                if (t.IsForbidden)
                    return true;
                return false;
            });
            return bhav;
        }
        [Obsolete]
        static public Behavior FailOnUnavailableTarget(this Behavior bhav, int targetInd)
        {
            return bhav.FailOnUnavailableTarget((TargetIndex)targetInd);
        }
        static public Behavior FailOnUnavailablePlacedItems(this Behavior bhav)
        {
            bhav.FailOn(() =>
            {
                var task = bhav.Actor.CurrentTask;
                foreach (var t in task.PlacedObjects)
                {
                    if (t.Object.IsDisposed)
                        return true;
                    if (t.Object.IsForbidden)
                        return true;
                }
                return false;
            });
            return bhav;
        }
        static public Behavior FailOnRanOutOfPatienceWaiting(this BehaviorPerformTask bhav, Action failAction = null)
        {
            var actor = bhav.Actor;
            var patienceTrait = actor.GetTrait(TraitDefOf.Patience);
            var patienceBase = Engine.TicksPerSecond * TimeSpan.FromMinutes(1).TotalSeconds;
            var patience = patienceBase * (1 + patienceTrait.Percentage * .5f);
            bhav.FailOn(() =>
            {
                if (bhav.Task.TicksWaited < patience)
                    return false;
                failAction?.Invoke();
                return true;
            });
            return bhav;
        }
        static public void FailOnNoDesignation(this BehaviorPerformTask bhav, TargetIndex targetInd, DesignationDef designation)
        {
            bhav.FailOnNoDesignation((int)targetInd, designation);
        }
        static public void FailOnNoDesignation(this BehaviorPerformTask bhav, int targetInd, DesignationDef designation)
        {
            bhav.FailOn(() =>
            {
                var global = bhav.Task.GetTarget(targetInd).Global;
                return !bhav.Actor.Town.DesignationManager.IsDesignation(global, designation);
            });
        }
        static public void FailOnCellStandedOn(this BehaviorPerformTask bhav, TargetIndex targetInd)
        {
            bhav.FailOn(() =>
            {
                var global = bhav.Task.GetTarget(targetInd).Global;
                var actor = bhav.Actor;
                var objects = actor.Map.GetObjects(global.Above());
                return objects.Any() && (objects.SingleOrDefault(o => o == actor) != actor); //objects.SingleOrDefault() != actor;
                //return !bhav.Actor.Map.IsEmptyNew(global.Above());
            });
        }
        static public BehaviorCustom NextTarget(Behavior bhavRoot, TargetIndex index)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                if (bhav.Actor.CurrentTask.NextTarget(index))
                    if(bhavRoot!=null)
                        bhav.Actor.CurrentTaskBehavior.JumpTo(bhavRoot);
            };
            return bhav;
        }
        static public BehaviorCustom NextTargetAmount(Behavior bhavRoot, TargetIndex index)
        {
            var bhav = new BehaviorCustom();
            bhav.InitAction = () =>
            {
                if (bhav.Actor.CurrentTask.NextTarget(index) && bhav.Actor.CurrentTask.NextAmount(index))
                    if (bhavRoot != null)
                        bhav.Actor.CurrentTaskBehavior.JumpTo(bhavRoot);
            };
            return bhav;
        }
        [Obsolete]
        static public BehaviorCustom NextTargetAmount(Behavior bhavRoot, int index)
        {
            return NextTargetAmount(bhavRoot, (TargetIndex)index);
        }
    }
}
