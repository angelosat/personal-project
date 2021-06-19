using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskGiverOfferQuest : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var manager = actor.Map.Town.QuestManager;
            //var questsOffered = manager.GetQuestDefs(actor);
            //foreach()
            var nextQuestReceiver = manager.GetNextQuestReceiver(actor);
            if (nextQuestReceiver == null)
                return null;
            return new AITask(typeof(TaskBehaviorOfferQuest), nextQuestReceiver);
        }
    }
}
