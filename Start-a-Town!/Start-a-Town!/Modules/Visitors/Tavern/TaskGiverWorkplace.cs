using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverWorkplace : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            //if (actor.Workplace is not Tavern tavern)
            //    return null;
            //return tavern.GetTask(actor);
            return actor.Workplace?.GetTask(actor);
        }
    }
}
