using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorGetQuest : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var task = this.Task;
            var actor = this.Actor;
            var qgiver = TargetIndex.A;
            var quest = task.Quest;
            yield return BehaviorHelper.MoveTo(qgiver);
            yield return new BehaviorInteractionNew(qgiver, () => new InteractionGetQuest(quest));
            //yield return new BehaviorCustom()
            //{
            //    InitAction = () =>
            //     {
            //         actor.Town.QuestManager.RemoveQuestReceiver(quest);
            //     }
            //};
        }
        public override void CleanUp()
        {
            var actor = this.Actor;
            var task = this.Task;
            actor.Town.QuestManager.RemoveQuestReceiver(task.Quest);
        }
    }
}
