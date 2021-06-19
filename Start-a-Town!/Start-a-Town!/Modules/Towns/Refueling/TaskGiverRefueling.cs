using Start_a_Town_.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverRefueling : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var refuelables = actor.Town.GetRefuelablesNew();
            foreach (var destination in refuelables)
            {
                if (!actor.CanReserve(destination.Key))
                    continue;
                var refComp = destination.Value.GetComp<EntityCompRefuelable>();
                if (refComp?.Fuel.Percentage > .5f)
                    continue;
                var fuelProgress = refComp.Fuel;
                var fuelMissing = fuelProgress.Max - fuelProgress.Value;
                var allObjects = actor.Map.GetObjectsLazy(); //.OrderByReachableRegionDistance(actor).ToList();//.OrderByDistanceTo(actor);

                //var testlist = allObjects.Where(t => t.IsHaulable && refComp.CanAccept(t) && t.Material?.Fuel.Value > 0).ToList();
                //var testlist2 = testlist.OrderByDistanceTo(actor).ToList();
                //var ordered = testlist2.OrderByReachableRegionDistance(actor).ToList();
                var handled = new HashSet<GameObject>();
                foreach (var fuel in allObjects)//)
                //foreach (var fuel in ordered)//allObjects)
                {
                    handled.Add(fuel);
                    if (!fuel.IsHaulable)
                        continue;
                    if (!refComp.Accepts(fuel as Entity))
                        continue;
                    if (fuel.Material?.Fuel.Value > 0)
                    {
                        //var task = new AITask(typeof(TaskBehaviorRefueling), new TargetArgs(fuel), new TargetArgs(actor.Map, destination.Key));
                        //return task;

                        var task = new AITask(typeof(TaskBehaviorRefueling));//, new TargetArgs(fuel), new TargetArgs(actor.Map, destination.Key));
                        task.SetTarget(TaskBehaviorRefueling.DestinationIndex, new TargetArgs(actor.Map, destination.Key));
                        foreach (var similar in CollectUntilFull(actor, refComp, fuel, fuelMissing, handled))
                        {
                            task.AddTarget(TaskBehaviorRefueling.SourceIndex, new TargetArgs(similar.Key), similar.Value);
                        }
                        return task;
                    }
                }
            }
            return null;
        }
        static IEnumerable<KeyValuePair<GameObject, int>> CollectUntilFull(Actor actor, EntityCompRefuelable refComp, GameObject center, float fuelMissing, HashSet<GameObject> handled)
        {
            //var similarNearby = center.GetNearbyObjectsNew(r => r <= 5, f => f.IsFuel);
            var similarNearby = center.Map.GetNearbyObjectsNew(center.Global, r => r <= 5, f => f.IsFuel);
            int stackEnduranceLimit = actor.GetHaulStackLimitFromEndurance(center);

            float currentFuelValue = 0;
            int totalAmountToCollect = 0;
            var enumerator = similarNearby.GetEnumerator();
            while (
                totalAmountToCollect < center.StackMax &&
                currentFuelValue < fuelMissing &&
                totalAmountToCollect + 1 <= stackEnduranceLimit && // we're just below encumberance limit
                enumerator.MoveNext())
            {
                var fuelItem = enumerator.Current;
                if (handled.Contains(fuelItem) && fuelItem != center)
                    continue;
                handled.Add(fuelItem);
                if (!fuelItem.IsHaulable)
                    continue;
                //if (center.ID != fuelItem.ID)
                //    continue;
                if (fuelItem != center && !center.CanAbsorb(fuelItem))
                    continue;
                if (!refComp.Accepts(fuelItem as Entity))
                    continue;

                var fuelValue = fuelItem.Fuel;
                var desiredAmountToCollectByFuel = (int)(fuelMissing / fuelValue);
                var desiredAmountToCollectByWeight = (int)Math.Min(stackEnduranceLimit - totalAmountToCollect, desiredAmountToCollectByFuel);
                //var actualAmountToCollect = (int)Math.Min(fuelItem.StackSize, desiredAmountToCollectByFuel);
                var actualAmountToCollect = (int)Math.Min(fuelItem.StackSize, desiredAmountToCollectByWeight);
                var fuelValueToCollect = actualAmountToCollect * fuelValue;
                currentFuelValue += fuelValueToCollect;
                totalAmountToCollect += actualAmountToCollect;
                //stackEnduranceLimit -= actualAmountToCollect;
                yield return new KeyValuePair<GameObject, int>(fuelItem, actualAmountToCollect);
            }
        }

        

        /// GOOD ONE WITHOUT WEIGHT 
        //static IEnumerable<KeyValuePair<GameObject, int>> CollectUntilFull(Entity actor, EntityCompRefuelable refComp, GameObject center, float fuelMissing, HashSet<GameObject> handled)
        //{
        //    //var similarNearby = center.GetNearbyObjectsNew(r => r <= 5, f => f.IsFuel);
        //    var similarNearby = center.Map.GetNearbyObjectsNew(center.Global, r => r <= 5, f => f.IsFuel);
        //    //var currentFuelValue = center.Fuel;
        //    //if (currentFuelValue == 0)
        //    //    throw new Exception();
        //    float currentFuelValue = 0;
        //    int totalAmountToCollect = 0;
        //    var enumerator = similarNearby.GetEnumerator();
        //    while (
        //        totalAmountToCollect < center.StackMax &&
        //        currentFuelValue < fuelMissing &&
        //        enumerator.MoveNext())
        //    {
        //        var fuelItem = enumerator.Current;
        //        if (handled.Contains(fuelItem))
        //            continue;
        //        handled.Add(fuelItem);
        //        if (!fuelItem.IsHaulable)
        //            continue;
        //        if (center.ID != fuelItem.ID)
        //            continue;
        //        if (!refComp.CanAccept(fuelItem))
        //            continue;
        //        var fuelValue = fuelItem.Fuel;
        //        var desiredCountToCollectByFuel = (int)(fuelMissing / fuelValue);
        //        var actualCountToCollectByFuel = (int)Math.Min(fuelItem.StackSize, desiredCountToCollectByFuel);
        //        var fuelValueToCollect = actualCountToCollectByFuel * fuelValue;
        //        currentFuelValue += fuelValueToCollect;
        //        totalAmountToCollect += actualCountToCollectByFuel;
        //        yield return new KeyValuePair<GameObject, int>(fuelItem, actualCountToCollectByFuel);
        //    }
        //}




        //protected override AITask TryAssignTask(Entity actor)
        //{
        //    var refuelables = actor.Town.GetRefuelablesNew();
        //    var actuallyRefuelables = refuelables.Where(t => t.Value.GetComp<EntityCompRefuelable>()?.Fuel.Percentage <= .5f);
        //    if (!actuallyRefuelables.Any())
        //        return null;
        //    var destination = actuallyRefuelables.First().Key;
        //    var fuels = actor.Map.GetObjects().Where(o => o.Material?.Fuel > 0);

        //    var fuel = fuels.FirstOrDefault();

        //    if (fuel == null)
        //        return null;
        //    var task = new AITask(typeof(TaskBehaviorRefueling), new TargetArgs(fuel), new TargetArgs(actor.Map, destination));
        //    return task;
        //}
    }
}
