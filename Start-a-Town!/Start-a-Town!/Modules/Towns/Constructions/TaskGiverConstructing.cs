using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    class TaskGiverConstructing : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Builder))
                return null;
            var manager = actor.Map.Town.ConstructionsManager;
            // TODO: if was previously building, continue building other available nearby unfinished constructions, instead of stopping and delivering materials

            var all = manager.GetAllBuildableCurrently();
            var allOrdered = all
                .OrderByReachableRegionDistance(actor);
            var lastBehav = actor.GetLastBehavior();
            var preferBuild = lastBehav != null && lastBehav is TaskBehaviorConstruct;

            foreach (var closest in allOrdered)
            {
                if (!actor.CanReserve(closest)) // filter them here or before ordering them by distance?
                    continue;

                if (actor.Map.GetBlockEntity(closest) is not IConstructible blockEntity)
                    continue; // because the list contains other block types as well


                if (blockEntity.IsReadyToBuild(out ItemDef def, out Material mat, out int amount))
                {
                    var buildtask = TryBuild(actor, closest, blockEntity);
                    if (buildtask != null)
                    {
                        return buildtask;
                    }
                    else
                        continue;
                }
                if (preferBuild)
                {
                    continue;
                }

                var deliverTask = TryDeliverMaterialNewNew(actor, closest, all, def, mat);//, amount);
                if (deliverTask != null)
                    return deliverTask;
                else
                    continue;
            }
           
            return null;
        }
        

        static public bool IsOperatable(Actor actor, Vector3 global)
        {
            var map = actor.Map;
            var nodes = map.Regions.GetPotentialNodesAroundDestination(actor.Physics.Reach, global);
            var any = nodes.Any(n => map.IsStandableOn(n.Global));
            return any;
        }
        AITask TryBuild(Actor actor, Vector3 global, IConstructible cachedBlockEntity )
        {
            if (!actor.CanReserve(global))
                return null;
            if (!IsOperatable(actor, global))
                return null;
           
            ///move aside any obstructing items
            var items = actor.Map.GetObjects(global);
            foreach(var i in items)
            {
                if (i is not Entity ientity)
                    continue;
                var haulAsideTask = TaskHelper.TryHaulAside(actor, ientity);
                if (haulAsideTask != null)
                    return haulAsideTask;
            }
            if (items.Any()) // return null if failure to return haul aside task
                return null;

            var buildtask = new AITask(TaskDefOf.Construct);
            buildtask.SetTarget(TaskBehaviorConstruct.ConstructionsID, new TargetArgs(actor.Map, global));

            var construction = cachedBlockEntity as BlockConstructionEntity;
            if (construction.Product.Block.BuildProperties.ToolSensitivity > 0)
                FindTool(actor, buildtask, ToolAbilityDef.Building);

            return buildtask;
        }
        
        AITask TryDeliverMaterialNewNew(Actor actor, Vector3 origin, IEnumerable<IntVec3> allConstructions, ItemDef ingredientDef, Material ingredientMat)
        {
            if (!IsOperatable(actor, origin))
                return null;
            var task = new AITask(TaskDefOf.DeliverMaterials);
            var allObjects = actor.Map.GetObjectsLazy();
            var enduranceLimit = Math.Min(actor.GetHaulStackLimitFromEndurance(ingredientDef), ingredientDef.StackCapacity);
            var maxDeliverable = 0;
            var similarNearbyConstructions = GetNearbyConstructionsWithSameMaterialNewNewInclusive(actor, allConstructions, origin, ingredientDef);
            var constrEnum = similarNearbyConstructions.GetEnumerator();
            while (maxDeliverable < enduranceLimit && constrEnum.MoveNext())
            {
                var n = constrEnum.Current;
                var constr = actor.Map.GetBlockEntity(n) as IConstructible;
                maxDeliverable += constr.GetMissingAmount(ingredientDef);
            }

            var remaining = Math.Min(enduranceLimit, maxDeliverable);
            var found = 0;

            var objenum = allObjects.GetEnumerator();
            while (objenum.MoveNext() && remaining > 0)
            {
                var o = objenum.Current;
                if (ingredientDef != o.Def)
                    continue;
                if (o.PrimaryMaterial != ingredientMat)
                    continue;
                var unreservedAmount = actor.GetUnreservedAmount(o);
                if (unreservedAmount == 0)
                    continue;
                if (!actor.CanReach(o.Global))
                    continue;
                var amountToPick = Math.Min(remaining, unreservedAmount);
                found += amountToPick;
                remaining -= amountToPick;
                if (remaining < 0)
                    throw new Exception();
                task.AddTarget(TaskBehaviorDeliverMaterials.MaterialID, new TargetArgs(o), amountToPick);
            }
            if (found == 0)
                return null;
            remaining = found;
            constrEnum = similarNearbyConstructions.GetEnumerator();
            while (constrEnum.MoveNext() && remaining > 0)
            {
                var currentDelivery = constrEnum.Current;
                var missingAmount = (actor.Map.GetBlockEntity(currentDelivery) as IConstructible).GetMissingAmount(ingredientDef);
                var toDrop = Math.Min(remaining, missingAmount);

                if (toDrop == 0)
                    continue;
                task.AddTarget(TaskBehaviorDeliverMaterials.DestinationID, new TargetArgs(actor.Map, currentDelivery), toDrop);
                remaining -= toDrop;
            }
            
            return task;
        }
        
        static IEnumerable<Vector3> GetNearbyConstructionsWithSameMaterialNewNewInclusive(Actor actor, IEnumerable<IntVec3> allBuildable, Vector3 origin, ItemDef def)
        {
            yield return origin;
            var map = actor.Map;
            var currentBlockEntity = map.GetBlockEntity(origin);
            var maxRangeSquared = 25;
            var distinctDesignations = new HashSet<BlockEntity>
            {
                currentBlockEntity
            };

            foreach (var designation in allBuildable)
            {
                if (!actor.CanReserve(designation))
                    continue;
                var entity = map.GetBlockEntity(designation);
                if (entity == null)
                    throw new Exception();
                if (distinctDesignations.Contains(entity))
                    continue;
                if (!(entity as IConstructible)?.IsValidHaulDestination(def) ?? false)
                    continue;
                if (!actor.CanReach(designation))
                    continue;
                if (Vector3.DistanceSquared(designation, origin) > maxRangeSquared)
                    continue;
                if (!IsOperatable(actor, designation))
                    continue;
                yield return designation;
            }
            
        }

    }
}
