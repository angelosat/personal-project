using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskGiverQuestComplete : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var props = actor.GetVisitorProperties();
            var quests = props.GetQuests();
            foreach(var q in quests)
            {
                if (!q.IsCompleted(actor))
                    continue;
                var qgiver = q.Giver;
                actor.Town.QuestManager.HandleQuestReceiver(actor, q);
                return new AITask(typeof(TaskBehaviorQuestComplete), qgiver) { Quest = q.ID };
            }
            return null;
        }
    }
}
