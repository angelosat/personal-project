using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.AI
{
    class TaskGiverEat : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            //return null;
            var hunger = actor.GetNeed(NeedDef.Hunger);
            //if (hunger.Value > 50)
            //{// if not currently hungry, pick up food and store in inventory

            //    return null;
            //}
            //var foods = from obj in actor.Map.GetObjects()
            //            where obj.HasComponent<ConsumableComponent>()
            //            where actor.GetUnreservedAmount(obj) > 0
            //            //orderby Vector3.DistanceSquared(obj.Global, actor.Global)
            //            let target = new TargetArgs(obj)
            //            select target;
            //var food = foods.FirstOrDefault();
            var isHungry = hunger.IsBelowThreshold;// hunger.Value < 98;// hunger.Threshold;// 25;

            if (isHungry)
            {
                //check inventory for meals
                var foodInInventory = actor.InventoryFirst(i => i.IsFood);
                if (foodInInventory != null)
                {
                    IOrderedEnumerable<Vector3> eatingPlaces = FindEatingPlaces(actor);
                    //if (eatingPlaces.Any())
                    //    eatingplace = new TargetArgs(actor.Map, eatingPlaces.First());
                    //var bhavType = eatingPlaces.Any() ? typeof(TaskBehaviorWithTable) : typeof(TaskBehaviorWithoutTable);
                    if (eatingPlaces.Any())
                    {
                        return new AITask() { BehaviorType = typeof(TaskBehaviorEatWithTable) }
                            .SetTarget(TaskBehaviorEating.FoodInd, foodInInventory, 1)
                            .SetTarget(TaskBehaviorEating.EatingSurfaceInd, new TargetArgs(eatingPlaces.First()));
                    }
                    else
                    {
                        return new AITask() { BehaviorType = typeof(TaskBehaviorEatWithoutTable) }
                            .SetTarget(TaskBehaviorEating.FoodInd, foodInInventory, 1);
                    }
                    //return new AITask() { BehaviorType = typeof(TaskBehaviorEating) }
                    //    .SetTarget(TaskBehaviorEating.FoodInd, foodInInventory, 1)
                    //    .SetTarget(TaskBehaviorEating.EatingSurfaceInd, eatingPlaces.Any() ? new TargetArgs(eatingPlaces.First()) : TargetArgs.Null);
                }
            }
            //return null;
            var food = actor.Map.GetObjects()
                .Where(obj => obj.HasComponent<ConsumableComponent>() && actor.GetUnreservedAmount(obj) > 0)
                .Select(o => new TargetArgs(o))
                .OrderByReachableRegionDistance(actor)
                .FirstOrDefault();
            if (food == null)
                return null;
            if (food.Type == TargetType.Null)
                return null;
            var unreserved = actor.GetUnreservedAmount(food);

            if (!isHungry)
            {// if not currently hungry, and not currently having a food item in inventory, pick up food and store in inventory
                if (!actor.InventoryContains(i => i.IsFood))
                {
                    return null;
                    return new AITask(typeof(TaskBehaviorStoreInInventory))
                    {
                        TargetA = food.Clone(),// new TargetArgs(food), // WHY CLONE???
                        AmountA = 1 // TODO: pack food until a certain amount of nutrition instead of just a specific stack size
                    };// new TaskPickUp(item);
                }
                return null;
            }
            var map = actor.Map;
            TargetArgs eatingplace = TargetArgs.Null;
            var belowFood = food.Global - Vector3.UnitZ;
            var cellBelowFood = actor.Map.GetCell(belowFood);
            //if (cellBelowFood.Block != Block.Stool)
            //if (!cellBelowFood.Block.HasUsage(BlockUsageDefOf.EatingSurface))
            if (!map.Town.HasUtility(belowFood, Utility.Types.Eating))
            {
                    IOrderedEnumerable<Vector3> eatingPlaces = FindEatingPlaces(actor);
                if (eatingPlaces.Any())
                    eatingplace = new TargetArgs(actor.Map, eatingPlaces.First());
            }
            //var task = new TaskEating(food, eatingplace);
            if(eatingplace != TargetArgs.Null)
                return new AITask(typeof(TaskBehaviorEatWithTable)).SetTarget(TaskBehaviorEating.FoodInd, food, 1).SetTarget(TaskBehaviorEating.EatingSurfaceInd, eatingplace);
            else
                return new AITask(typeof(TaskBehaviorEatWithoutTable)).SetTarget(TaskBehaviorEating.FoodInd, food, 1);

            //var task = new AITask(typeof(TaskBehaviorEating)).SetTarget(TaskBehaviorEating.FoodInd, food, 1).SetTarget(TaskBehaviorEating.EatingSurfaceInd, eatingplace);
            //return task;
        }

        private static IOrderedEnumerable<Vector3> FindEatingPlaces(Actor actor)
        {
            //if (actor.Map.Town.TownUtilities.TryGetValue(Block.Types.Stool, out tables))
            //    eatingplace = new TargetArgs(tables.First() + Vector3.UnitZ);
            return actor.Map.Town.GetUtilities(Utility.Types.Eating).Where(p => actor.CanReserve(p)).OrderBy(p => Vector3.DistanceSquared(p, actor.Global));
        }
    }
}
