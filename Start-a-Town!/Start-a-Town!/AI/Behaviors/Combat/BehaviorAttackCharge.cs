using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorAttackCharge : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var attack = parent.GetComponent<AttackComponent>();
            if (attack.State == Components.Attack.States.Ready)
            {
                var server = parent.Net as Server;
                server.AIHandler.AIStartAttack(parent);
            }
            else if (attack.State == Components.Attack.States.Charged)
                return BehaviorState.Fail;
            return BehaviorState.Fail;
            //throw new NotImplementedException();
        }
        public override object Clone()
        {
            return new BehaviorAttackCharge();
        }
    }
}
