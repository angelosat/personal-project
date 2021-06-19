using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Microsoft.Xna.Framework;
using Start_a_Town_.Towns;

namespace Start_a_Town_
{
    class TaskGiverHaulToStockpileNew : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Hauler))
                return null;
            if (actor.IsTooTiredToWork)
                return null;
            if (actor.Carried != null)
            {
                //return StockpileAIHelper.TryHaulNew(actor, actor.Carried as Entity);
                return null;
            }
            var items = HaulHelper.GetPotentialItemsNew(actor);
            var task = StockpileAIHelper.FindBestTask(actor, items);// items);
            return task;
            //var list = items.ToList();
            //return StockpileAIHelper.FindBestTask(actor, list);// items);
        }
        
        public override AITask TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false)
        {
            var item = target.Object as Entity;
            return item != null ? StockpileAIHelper.TryHaulNew(actor, item, ignoreOtherReservations) : null;
            //return item != null ? StockpileAIHelper.TryHaul(actor, item, ignoreOtherReservations) : null;

        }
        IEnumerable<GameObject> GetHaulables(Actor actor)
        {
            var items =
                this.GetPotentialItems(actor)
                .Where(i=>actor.CanReserve(i))
                .OrderByReachableRegionDistance(actor);
            //.OrderBy(i => Vector3.DistanceSquared(i.Global, actor.Global));
            return items;
        }
        private List<GameObject> GetPotentialItems(Actor actor)
        {
            List<GameObject> entitiesToInsert = new List<GameObject>();
            var manager = actor.Map.Town.StockpileManager;
            List<GameObject> allOwnedEntities = new List<GameObject>();
            //foreach (var stockpile in manager.Stockpiles)
            //    allOwnedEntities.AddRange(stockpile.Value.GetContents());

            // get all entities in area around stockpile who aren't already owned by a stockpile
            // or get entities in town owned chunks?
            //var chunks = this.Town.GetOwnedChunks();
            //foreach (var chunk in chunks)
            //{
            var objs = actor.Map.GetObjects();// chunk.GetObjects();
            allOwnedEntities = objs.Where(manager.IsItemAtBestStockpile).ToList();
            entitiesToInsert.AddRange(from entity in objs
                                      where entity.IsStockpilable() && entity.Physics.Size != Components.ObjectSize.Immovable && !allOwnedEntities.Contains(entity)
                                      select entity);

            return entitiesToInsert;
        }
        GameObject GetClosestHaulable(Actor actor)
        {
            var items = 
                this.GetPotentialItems(actor)
                .Where(i => actor.CanReserve(i))
                .OrderByReachableRegionDistance(actor);
                //.OrderBy(i => Vector3.DistanceSquared(i.Global, actor.Global));
            return items.FirstOrDefault();
        }
        
        bool IsItemStockpiled(Actor obj)
        {
            foreach (var s in obj.Map.Town.StockpileManager.Stockpiles.Values)
                if (s.Contains(obj))
                    return true;
            return false;
        }
    }
}
