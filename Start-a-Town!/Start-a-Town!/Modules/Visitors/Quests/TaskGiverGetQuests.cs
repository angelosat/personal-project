using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskGiverGetQuests : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            var town = actor.Town;
            var quests = town.QuestManager.GetQuestDefs();
            foreach(var q in quests)
            {
                // TODO check before receiving quest:
                // reward vs difficulty
                // is completable (has access to areas that the required items can be found)
                var giver = q.Giver;
                if (giver == null)
                    continue;
                if (!q.CanGiveQuestTo(actor))
                    continue; 
                if (!actor.CanAcceptQuest(q))
                    continue;
                if (!actor.CanReachNew(giver))
                    continue;
                if (!Decide(actor, q))
                    continue;
                actor.Town.QuestManager.HandleQuestReceiver(actor, q);
                return new AITask(QuestTaskDefOf.AcceptQuest, giver) { Quest = q.ID };
            }
            return null;
        }

        private bool Decide(Actor actor, QuestDef q)
        {
            return q.GetRewardRatio() >= 1;
        }
    }
}
