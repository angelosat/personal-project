﻿using System;
using System.Collections.Generic;

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
                var refComp = destination.Value.GetComp<BlockEntityCompRefuelable>();
                if (refComp?.Fuel.Percentage > .5f)
                    continue;
                var fuelProgress = refComp.Fuel;
                var fuelMissing = fuelProgress.Max - fuelProgress.Value;
                var allObjects = actor.Map.GetObjectsLazy();
                var handled = new HashSet<GameObject>();
                foreach (var fuel in allObjects)
                {
                    handled.Add(fuel);
                    if (!fuel.IsHaulable)
                        continue;
                    if (!refComp.Accepts(fuel as Entity))
                        continue;
                    if (fuel.Material?.Fuel.Value > 0)
                    {
                        var task = new AITask(typeof(TaskBehaviorRefueling));
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
        static IEnumerable<KeyValuePair<GameObject, int>> CollectUntilFull(Actor actor, BlockEntityCompRefuelable refComp, GameObject center, float fuelMissing, HashSet<GameObject> handled)
        {
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
           
                if (fuelItem != center && !center.CanAbsorb(fuelItem))
                    continue;
                if (!refComp.Accepts(fuelItem as Entity))
                    continue;

                var fuelValue = fuelItem.Fuel;
                var desiredAmountToCollectByFuel = (int)(fuelMissing / fuelValue);
                var desiredAmountToCollectByWeight = (int)Math.Min(stackEnduranceLimit - totalAmountToCollect, desiredAmountToCollectByFuel);
                var actualAmountToCollect = (int)Math.Min(fuelItem.StackSize, desiredAmountToCollectByWeight);
                var fuelValueToCollect = actualAmountToCollect * fuelValue;
                currentFuelValue += fuelValueToCollect;
                totalAmountToCollect += actualAmountToCollect;
                yield return new KeyValuePair<GameObject, int>(fuelItem, actualAmountToCollect);
            }
        }
    }
}
