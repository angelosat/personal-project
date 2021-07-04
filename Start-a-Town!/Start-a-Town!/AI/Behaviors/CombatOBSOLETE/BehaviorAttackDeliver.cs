using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorAttackDeliver : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var attack = parent.GetComponent<AttackComponent>();
            if (attack.State == Components.Attack.States.Charging)
            {
                var chargeValue = attack.ChargeFunc();
                if (chargeValue == 1)
            //if (attack.State == Components.Attack.States.Charged)

                {
                    var server = parent.Net as Server;
                    var dir = (state.Threats.First().Entity.Global - parent.Global).Normalized();
                    server.AIHandler.AIFinishAttack(parent, dir);
                    return BehaviorState.Success;
                }
            }
            return BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorAttackDeliver();
        }
    }
}
