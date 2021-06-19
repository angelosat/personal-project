using Start_a_Town_.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_
{
    class TaskGiverDeconstruct : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            if (!actor.HasLabor(JobDefOf.Builder))
                return null;
            var allPositions = actor.Map.Town.DesignationManager.GetDesignations(DesignationDef.Deconstruct);
            foreach(var pos in allPositions)
            {
                if (!actor.CanReserve(pos))
                    continue;
                if (!actor.CanReach(pos))
                    continue;
                if (!actor.Map.IsEmptyNew(pos.Above()))
                    continue;
                var task = new AITask()// AITaskMining()
                {
                    BehaviorType = typeof(TaskBehaviorDeconstruct),
                    //Tool = tool,
                    //TargetA = new TargetArgs(actor.Map, pos)
                };
                task.SetTarget(TaskBehaviorDeconstruct.DeconstructInd, new TargetArgs(actor.Map, pos));
                //FindTool(actor, Skill.Building, task, TaskBehaviorDeconstruct.ToolInd);
                FindTool(actor, task, ToolAbilityDef.Building);

                return task;
            }
            return null;   
        }
    }
}
