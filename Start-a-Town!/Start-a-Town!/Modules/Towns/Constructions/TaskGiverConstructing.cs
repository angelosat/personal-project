using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Blocks;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Skills;

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
            //var all = manager.GetAllBuildable().Where(actor.CanReach).OrderByRegionDistance(actor);// .OrderByDistanceTo(actor);//.GetConstructions().Where(i => manager.IsBuildable(i)); //.Where(actor.CanReach)

            var all = manager.GetAllBuildableCurrently();
            //.Where(actor.CanReserve).ToList();
            var allOrdered = all
                .OrderByReachableRegionDistance(actor);// .OrderByDistanceTo(actor);//.GetConstructions().Where(i => manager.IsBuildable(i)); //.Where(actor.CanReach)
            var lastBehav = actor.GetLastBehavior();
            var preferBuild = lastBehav != null && lastBehav is TaskBehaviorConstruct;
            //preferBuild = false;

            //if (!allOrdered.Any())
            //    return null;

            //var toRetry = new List<Vector3>();

            foreach (var closest in allOrdered)
            {
                //if (!actor.CanReachNew(closest))
                //    continue;
                if (!actor.CanReserve(closest)) // filter them here or before ordering them by distance?
                    continue;

                // BlockDesignation.BlockDesignationEntity;
                if (actor.Map.GetBlockEntity(closest) is not IConstructible blockEntity)
                    continue; // because the list contains other block types as well


                //if (blockEntity.IsReadyToBuild(out ItemRequirement missing))
                if (blockEntity.IsReadyToBuild(out ItemDef def, out Material mat, out int amount))
                {
                    var buildtask = TryBuild(actor, closest, blockEntity);
                    if (buildtask != null)
                    {
                        //var construction = blockEntity as BlockConstructionEntity;
                        //if (construction.Product.Block.BuildProperties.ToolSensitivity > 0)
                        //    FindTool(actor, buildtask, ToolAbilityDef.Building);
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
            //if(toRetry.Any())
            //{
            //    foreach(var pos in toRetry)
            //    {
            //        var deliverTask = TryDeliverMaterial(actor, pos, all, mat);
            //        if (deliverTask != null)
            //            return deliverTask;
            //        else
            //            continue;
            //    }
            //}
            return null;
        }
        

        static public bool IsOperatable(Actor actor, Vector3 global)
        {
            var map = actor.Map;
            //var nodes = map.Regions.GetPotentialNodesAroundDestination((int)actor.Physics.Height, global);
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

           
            ///move aside any obstruting items
            var items = actor.Map.GetObjects(global);
            foreach(var i in items)
            {
                if (i is not Entity ientity)
                    continue;
                var haulAsideTask = TaskHelper.TryHaulAside(actor, ientity);
                if (haulAsideTask != null)
                    return haulAsideTask;
                //if (!i.IsHaulable)
                //    continue;
                //if (!actor.CanReserve(i))
                //    continue;
                //if (HaulHelper.TryFindNearbyPlace(actor, i, global, out var place))
                //{
                //    buildtask.AddTarget(TaskBehaviorConstruct.ObstructingItemsID, i);
                //    buildtask.AddTarget(TaskBehaviorConstruct.ObstructingItemsDestinationsID, place);
                //}
            }
            if (items.Any()) // return null if failure to return haul aside task
                return null;

            var buildtask = new AITask(TaskDefOf.Construct);
            buildtask.SetTarget(TaskBehaviorConstruct.ConstructionsID, new TargetArgs(actor.Map, global));
            //FindTool(actor, buildtask, ToolAbilityDef.Building);


            var construction = cachedBlockEntity as BlockConstructionEntity;
            if (construction.Product.Block.BuildProperties.ToolSensitivity > 0)
                FindTool(actor, buildtask, ToolAbilityDef.Building);

            return buildtask;
        }
        
        AITask TryDeliverMaterialNewNew(Actor actor, Vector3 origin, IEnumerable<IntVec3> allConstructions, ItemDef ingredientDef, Material ingredientMat)//, int amount)
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
                var constr = actor.Map.GetBlockEntity(n) as IConstructible;// BlockDesignation.BlockDesignationEntity;
                maxDeliverable += constr.GetMissingAmount(ingredientDef);// constr.Materials.Find(i => i.ObjectID == mat.ObjectID).Remaining;
            }

            var remaining = Math.Min(enduranceLimit, maxDeliverable);
            var found = 0;

            var objenum = allObjects.GetEnumerator();
            //foreach(var o in allObjects)
            while (objenum.MoveNext() && remaining > 0)
            {
                var o = objenum.Current;
                if (ingredientDef != o.Def)
                    continue;
                if (o.PrimaryMaterial != ingredientMat)
                    continue;
                var unreservedAmount = actor.GetUnreservedAmount(o);
                //if (!actor.CanReserve(o))
                //    continue;
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
            //found.ToConsole();
            if (found == 0)
                return null;
            remaining = found;
            constrEnum = similarNearbyConstructions.GetEnumerator();
            while (constrEnum.MoveNext() && remaining > 0)
            {
                var currentDelivery = constrEnum.Current;
                var missingAmount = (actor.Map.GetBlockEntity(currentDelivery) as IConstructible).GetMissingAmount(ingredientDef);
                var toDrop = Math.Min(remaining, missingAmount);//.Materials.First().Remaining;

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
                //if (!(entity as BlockDesignation.BlockDesignationEntity).IsValidHaulDestination(def))
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

        

        private bool IsValid(IMap map, Vector3 closest)
        {
            return map.Town.ConstructionsManager.IsDesignatedConstruction(closest);
        }
        
    }
}
