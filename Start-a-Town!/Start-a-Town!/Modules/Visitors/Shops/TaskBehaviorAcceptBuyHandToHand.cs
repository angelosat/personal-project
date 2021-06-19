using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorAcceptBuyHandToHand : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            var actor = this.Actor;
            var state = this.Actor.GetState();
            var tradingpartner = state.TradingPartner;
            //yield return new BehaviorCustom()
            //{
            //    InitAction = () =>{ this.Task.TargetB = new TargetArgs(tradingpartner)
            //};
            yield return new BehaviorStopMoving();
            yield return new BehaviorWait(() =>
            {
                if (tradingpartner.Carried != null)
                {
                    //this.Task.TargetA = new TargetArgs(actor.GetMoney());
                    this.Task.TargetA = actor.GetMoney();
                    this.Task.AmountA = tradingpartner.Carried.GetValueTotal();
                    return true;
                }
                return false;
            });
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionHaul(this.Task.AmountA));
            //yield return new BehaviorInteractionNew(TargetIndex.B, () => new InteractionGiveItem(true));
            yield return new BehaviorWait(() => state.TradingPartner == null);
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionStoreHauled());
        }
    }
}
