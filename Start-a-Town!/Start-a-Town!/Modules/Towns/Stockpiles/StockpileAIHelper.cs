using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Towns;

namespace Start_a_Town_
{
    class StockpileAIHelper
    {
        const int BaseSearchRange = 5;
        static public AITask FindBestTask(Actor actor, IEnumerable<Entity> items)
        {
            foreach (var i in items)
            {
                var task = TryHaulNew(actor, i);

                if (task != null)
                    return task;
            }
            return null;
        }
        
        static public AITask HaulToStockpileNew(Actor actor, Entity item)
        {
            var target = StockpileManager.GetBestStoragePlace(actor, item, out int maxAmount);
            maxAmount = item.StackSize;// Math.Min(item.StackSize, maxAmount);
            if (target == null)
                return null;
            var task = new AITask(typeof(TaskBehaviorHaulToStockpileNew));
            //task.Targets.Add(new TargetArgs(item));
            //task.Targets.Add(target);
            task.TargetA = new TargetArgs(item);
            task.TargetB = target;
            task.AmountA = maxAmount;
            return task;
        }

        

        static public AITask TryHaulNew(Actor actor, Entity item, bool ignoreOtherReservations = false)
        {
            var unreservedAmount = actor.GetUnreservedAmount(item);
            if (unreservedAmount == 0)
                return null;
            var targets = StockpileAIHelper.GetAllValidStoragePlacesNoReserveCheckLazy(actor, item);
            if (!targets.Any())
                return null;
            //var ordered = targets.OrderBy(t => Vector3.DistanceSquared(actor.Global, t.Key.Global));
            var enumerator = targets.GetEnumerator();
            enumerator.MoveNext();
            TargetArgs bestTarget = enumerator.Current;
            int bestTargetUnreservedAmount = actor.GetUnreservedAmount(bestTarget.Global);
            while (bestTargetUnreservedAmount == 0 && enumerator.MoveNext())
            {
                bestTarget = enumerator.Current;
                bestTargetUnreservedAmount = actor.GetUnreservedAmount(bestTarget);
            }
            if (bestTarget == null)
                return null;
            if (bestTargetUnreservedAmount == 0)
                return null;
            var maxweight = Math.Min(item.StackMax, actor.GetHaulStackLimitFromEndurance(item));
            var maxamount = Math.Min(maxweight, unreservedAmount);
            //maxamount = Math.Min(maxamount, bestTargetUnreservedAmount);
            var task = new AITask(TaskDefOf.Hauling, new TargetArgs(item), bestTarget) { Count = maxamount };// item.StackMax };
            if (bestTarget.HasObject)
                //task.Count -= bestTarget.Object.StackSize;
                //task.Count = Math.Min(task.Count, Math.Min(bestTargetUnreservedAmount, bestTarget.Object.StackAvailableSpace));
                task.Count = Math.Min(task.Count, bestTarget.Object.StackAvailableSpace);

            if (task.Count < 0)
                throw new Exception();
            while (enumerator.MoveNext() && 0 < task.Count && task.Count < item.StackMax)
            {
                var current = enumerator.Current;
                if (current.HasObject && current.Object != item)
                {
                    task.Count = Math.Min(item.StackMax, task.Count + item.StackMax - current.Object.StackSize);
                    if (task.Count < 0)
                        throw new Exception();
                }
                else
                    task.Count = item.StackMax;// Math.Min(item.StackMax, task.Count);
            }
            //foreach(var target in targets)
            //{

            //}
            if (task.Count < 0)
                throw new Exception();
            if (task.Count == 0)
                return null;// throw new Exception("haul task count was initialized as 0");
             return task;
            
        }

        static public IEnumerable<TargetArgs> GetAllValidHaulDestinationsLazy(GameObject actor, GameObject item)
        {
            Dictionary<TargetArgs, int> all = new Dictionary<TargetArgs, int>();
            var stockpiles = item.Map.Town.StockpileManager.GetStockpilesByPriority();

            foreach (var s in stockpiles)
            {
                if (s.Accepts(item.ID))
                    foreach (var spot in s.GetPotentialHaulTargets(actor, item))
                        yield return spot;
            }
        }
        static public Dictionary<TargetArgs, int> GetAllValidHaulDestinations(GameObject actor, GameObject item, out int maxamount)
        {
            Dictionary<TargetArgs, int> all = new Dictionary<TargetArgs, int>();
            var stockpiles = GetStoragesByPriority(item.Map);// item.Map.Town.StockpileManager.GetStockpilesByPriority();
            maxamount = 0;

            foreach (var s in stockpiles)
            {
                //if (s.Accepts(item.ID))
                if (s.Accepts(item as Entity))
                    foreach (var spot in s.GetPotentialHaulTargets(actor, item, out maxamount))
                        all.Add(spot.Key, spot.Value);
            }
            return all;
        }
        static public Dictionary<TargetArgs, int> GetAllValidHaulDestinationsOld(GameObject actor, GameObject item, out int maxamount)
        {
            Dictionary<TargetArgs, int> all = new Dictionary<TargetArgs,int>();
            var stockpiles = item.Map.Town.StockpileManager.GetStockpilesByPriority();
            maxamount = 0;
           
            foreach (var s in stockpiles)
            {
                //if (s.Accepts(item.ID))
                if (s.Accepts(item as Entity))
                    foreach (var spot in s.GetPotentialHaulTargets(actor, item, out maxamount))
                        all.Add(spot.Key, spot.Value);
            }
            return all;
        }
        static public TargetArgs GetBestHaulDestination(GameObject actor, GameObject item)
        {
            int maxAmount;
            var all = GetAllValidHaulDestinations(actor, item, out maxAmount).Keys.Select(k => new TargetArgs(actor.Map, k.Global + Vector3.UnitZ));
            var closest = all.OrderBy(t => Vector3.DistanceSquared(t.Global, actor.Global)).FirstOrDefault();
            return closest == null ? TargetArgs.Null : closest;
        }
        static bool IsItemStockpiled(GameObject obj)
        {
            return obj.Map.Town.StockpileManager.IsItemAtBestStockpile(obj);
            //foreach (var s in obj.Map.Town.StockpileManager.Stockpiles.Values)
            //    if (s.Contains(obj))
            //        return true;
            //return false;
        }
        static public bool IsItemAtBestStockpile(GameObject item)
        {
            var stockpiles = item.Map.Town.ZoneManager.GetZones<Stockpile>();
            var currentStockpile = stockpiles.FirstOrDefault(s => s.Contains(item));
            if (currentStockpile == null)
                return false;
            var betterStockpile = stockpiles
                .Where(s => 
                    s!= currentStockpile && 
                    s.CanAccept(item) && 
                    s.Priority > currentStockpile.Priority)
                .OrderByDescending(s => s.Priority)
                .FirstOrDefault();
            return betterStockpile == null;
        }

        static public IEnumerable<IStorage> GetStoragesByPriority(IMap map)
        {
            //var containers = this.Storages.Select(g => this.Town.Map.GetBlockEntity(g) as IStorage);
            IEnumerable<Stockpile> stockpiles = GetStockpiles(map);
            return stockpiles.OrderByDescending(i => i.Priority);
        }

        public static IEnumerable<Stockpile> GetStockpiles(IMap map)
        {
            return map.Town.ZoneManager.GetZones<Stockpile>();
        }

        static public IEnumerable<TargetArgs> GetAllValidStoragePlacesNoReserveCheckLazy(Actor actor, Entity item)
        {
            //var storages = item.Map.Town.StockpileManager.GetStoragesByPriority();
            //var storages = item.Map.Town.StockpileManager.GetStockpilesByPriority();
            var storages = GetStoragesByPriority(actor.Map);
            foreach (var s in storages)
            {
                if (s.Accepts(item))
                    foreach (var spot in s.GetPotentialHaulTargets(actor, item))
                        yield return spot;
            }
        }

        static public bool IsValidStorage(GameObject item, TargetArgs destination)
        {
            if (destination.HasObject && (destination.Object == null || !destination.Object.IsSpawned || destination.Object.Full))
                return false;
            var global = destination.Global;
            var targetStockpile = destination.Map.Town.ZoneManager.GetZoneAt<Stockpile>(global - Vector3.UnitZ);
            if (targetStockpile == null)
                return false;
            return targetStockpile.IsValidStorage(item as Entity, new TargetArgs(global - Vector3.UnitZ), item.StackSize);

            //return targetStockpile.IsValidStorage(itemID, global - Vector3.UnitZ);
        }
    }
}
