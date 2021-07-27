using System.Collections.Generic;
using System.Linq;

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
            if (actor.Hauled != null)
            {
                return null;
            }
            var items = HaulHelper.GetPotentialItemsNew(actor);
            var task = StockpileAIHelper.FindBestTask(actor, items);
            return task;
        }
        
        public override AITask TryTaskOn(Actor actor, TargetArgs target, bool ignoreOtherReservations = false)
        {
            var item = target.Object as Entity;
            return item != null ? StockpileAIHelper.TryHaulNew(actor, item, ignoreOtherReservations) : null;

        }
       
        private List<GameObject> GetPotentialItems(Actor actor)
        {
            List<GameObject> entitiesToInsert = new List<GameObject>();
            var manager = actor.Map.Town.StockpileManager;
            List<GameObject> allOwnedEntities = new List<GameObject>();
            
            var objs = actor.Map.GetObjects();
            allOwnedEntities = objs.Where(manager.IsItemAtBestStockpile).ToList();
            entitiesToInsert.AddRange(from entity in objs
                                      where entity.IsStockpilable() && entity.Physics.Size != Components.ObjectSize.Immovable && !allOwnedEntities.Contains(entity)
                                      select entity);

            return entitiesToInsert;
        }
    }
}
