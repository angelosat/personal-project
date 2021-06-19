using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI;

namespace Start_a_Town_
{
    class TaskBehaviorSleepOnGround : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            //yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionSleepOnGround());

            yield return new BehaviorCustom()
            {
                Mode = BehaviorCustom.Modes.Continuous,
                Init = (a, s) => this.Actor.Interact(new InteractionSleepOnGround()),
                SuccessCondition = a => this.Actor.GetNeed(NeedDef.Energy).Percentage == 1
            };
            yield return new BehaviorCustom() { Init = (a, t) => AIManager.EndInteraction(this.Actor, true) };

        }
    }
}
