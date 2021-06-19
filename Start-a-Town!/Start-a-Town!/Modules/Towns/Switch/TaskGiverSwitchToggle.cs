﻿using Start_a_Town_.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverSwitchToggle : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            //var sites = actor.Map.GetBlockEntitiesWithComp<BlockEntityCompSwitchable>();
            var sites = actor.Map.Town.DesignationManager.GetDesignations(DesignationDef.Switch);

            foreach (var site in sites)
            {
                var global = site;
                if (!actor.CanReserve(global) ||
                    !actor.CanReach(global))
                    continue;

                var task = new AITask(typeof(TaskBehaviorSwitchToggle), new TargetArgs(actor.Map, global));
                return task;
            }

            return null;
        }
    }
}
