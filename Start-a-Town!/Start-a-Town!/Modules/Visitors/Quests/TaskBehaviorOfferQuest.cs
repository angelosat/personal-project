using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    class TaskBehaviorOfferQuest : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var task = this.Task;
            var actor = this.Actor;
            var manager = actor.Town.QuestManager;
            yield return new BehaviorStopMoving();
            yield return new BehaviorWait(() => manager.GetNextQuestReceiver(actor) == null);
        }
    }
}
