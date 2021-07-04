using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorAttackCharge : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var attack = parent.GetComponent<AttackComponent>();
            if (attack.State == Attack.States.Ready)
            {
                var server = parent.Net as Server;
                server.AIHandler.AIStartAttack(parent);
            }
            else if (attack.State == Attack.States.Charged)
                return BehaviorState.Fail;
            return BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorAttackCharge();
        }
    }
}
