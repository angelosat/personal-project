using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.Towns.Digging;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class TaskGiverDigging : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Digger))
                return null;
            var map = actor.Map;
            
            var jobs = actor.Map.Town.DesignationManager.GetDesignations(DesignationDef.Mine);

            var mainhand = actor.GetEquipmentSlot(GearType.Mainhand);

            if (!jobs.Any())
                return TaskHelper.TryStoreEquipped(actor, GearType.Mainhand);

            //var minDistance = float.MaxValue;

            foreach (var job in jobs) // TODO: check if another npc is standing on the target block to be digged
            {
                if (!actor.CanReserve(job))
                    continue;
                if (!actor.CanReach(job))
                    continue;
           
                if(TaskHelper.TryHaulAside(actor, job.Above(), out var haulAsideTask))
                {
                    if (haulAsideTask != null)
                        return haulAsideTask;
                }
                else
                    continue;

                var block = map.GetBlock(job);
                var material = map.GetBlockMaterial(job);
                var skill = material.GetSkillToExtract();

                if (skill == null)
                    throw new Exception();

                var task = new AITask(TaskDefOf.Digging, new TargetArgs(actor.Map, job));
                FindTool(actor, task, skill);

                return task;
            }
            return null;
        }

        static public bool TryGetTask(Actor actor, TargetArgs target, out AITask task)
        {
            task = null;
            var global = target.Global;
            var block = target.GetBlock();
            if (!block.IsMinable)
                return false;
            if (!actor.CanReserve(target))
                return false;
            if (!actor.CanReach(target))
                return false;
            var material = actor.Map.GetBlockMaterial(global);
            var skill = material.GetSkillToExtract();

            if (skill == null)
                throw new Exception();
           
            task = new AITask()
            {
                BehaviorType = typeof(TaskBehaviorDiggingNewNew),
            };
            task.SetTarget(TaskBehaviorDiggingNewNew.MineInd, target);
            FindTool(actor, task, skill);
            return true;
        }
    }
}
