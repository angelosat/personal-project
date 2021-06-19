using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Combat;

namespace Start_a_Town_.Components.AI
{
    class AICombat : Behavior
    {
        float DelayBetweenAttacks = Engine.TargetFps * 3;
        float Delay;
        float BlockDuration = Engine.TargetFps * 3;
        float BlockCooldown = Engine.TargetFps * 3, BlockCooldownTimer;
        float BlockTimer;
        float ChargeThreshold = .5f;
        AttackComponent Attack;
        MobileComponent Movement;
        public override string Name
        {
            get
            {
                return "Combat";
            }
        }
        public override object Clone()
        {
            return new AICombat();
        }
        public override Behavior Initialize(GameObject parent)
        {
            this.Attack = parent.GetComponent<AttackComponent>();
            this.Movement = parent.GetComponent<MobileComponent>();
            return this;
        }

        GameObject Target;
        float Timer;

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            Personality personality = state.Personality;
            Knowledge knowledge = state.Knowledge;
            var server = parent.Net as Net.Server;

            if (this.BlockTimer > 0)
            {
                this.BlockTimer--;
                if (this.BlockTimer == 0)
                    StopBlocking(parent);
                else
                    return BehaviorState.Success;
            }
            if (this.BlockCooldownTimer > 0)
                this.BlockCooldownTimer--;

            if (Target == null)
            {
                //foreach (GameObject obj in state.Knowledge.Objects.Keys)
                //{
                //    if (!obj.Exists)
                //        continue;
                //    if (state.Personality.Hatelist.Contains(obj.Type))
                //    {
                //        float aggroRange = 5;
                //        if (Vector3.Distance(parent.Global, obj.Global) <= aggroRange)
                //            Target = obj;
                //    }
                //}
                //if (Target == null)
                    return BehaviorState.Fail;
            }

            Vector3 difference = (Target.Global - parent.Global);
            float dist = difference.Length();
            if (dist > 5) // if distance greater than value, stop chasing target
            {
                Target = null;
                server.AIHandler.AIStartMove(parent);
                server.AIHandler.AICancelAttack(parent);
                return BehaviorState.Success;
            }

            Vector3 direction;
            Vector3.Normalize(ref difference, out direction);
            //parent.Direction = direction;
            server.AIHandler.AIChangeDirection(parent, direction);

            var attack = parent.GetComponent<AttackComponent>();
            var mobile = parent.GetComponent<MobileComponent>();

            if (attack.State == Components.Attack.States.Ready)
            {
                this.Delay--;
                if (Delay <= 0)
                    server.AIHandler.AIStartAttack(parent);
            }

            if (dist > Components.Attack.DefaultRange) // if distance greater then value, move towards target
            {
                if (!mobile.Moving)
                    server.AIHandler.AIStartMove(parent);
                return BehaviorState.Success; // because we need to stop parent selector from traversing next nodes
                //return BehaviorState.Running;
            }
            else // else, stop moving and deliver attack (if already started charging it)
            {
                if (mobile.Moving)
                    server.AIHandler.AIStopMove(parent);
                if (attack.State == Components.Attack.States.Charging)
                {
                    if (attack.ChargeFunc() < this.ChargeThreshold)
                        return BehaviorState.Success;
                    this.Delay = this.DelayBetweenAttacks;
                    var dir = (this.Target.Global - parent.Global).Normalized();
                    server.AIHandler.AIFinishAttack(parent, dir);
                }
            }
            return BehaviorState.Success; // because we need to stop parent selector from traversing next nodes
            //return BehaviorState.Running;

            //if ((float)Target["Health"]["Value"] > 0)
            //    return BehaviorState.Running;
            //Target = null;
            //return BehaviorState.Success;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Attacked:
                case Message.Types.Attack:
                    //Target = e.Sender as GameObject;
                    var attacker = e.Parameters[0] as GameObject;
                    SetTarget(parent, attacker);
                    return false;// true;
                case Message.Types.Aggro:
                    SetTarget(parent, e.Sender as GameObject);
                    return true;
                case Message.Types.SetTarget:
                    SetTarget(parent, e.Parameters[0] as GameObject);
                    return true;


                case Message.Types.BlockCollision:
                    var obstacle = (Vector3)e.Parameters[0];
                    if (!e.Network.Map.IsSolid(obstacle + Vector3.UnitZ))
                        parent.GetComponent<MobileComponent>().Jump(parent);
                    //if (!e.Network.Map.IsSolid(parent.Global + parent.Direction + Vector3.UnitZ))
                    //    parent.GetComponent<MobileComponent>().Jump(parent);
                    return true;

                case Message.Types.AttackTelegraph:
                    attacker = e.Parameters[0] as GameObject;
                    //parent.Direction = (attacker.Transform.Global - parent.Global).Normalized();
                    (parent.Net as Net.Server).AIHandler.AIChangeDirection(parent, parent.Global.DirectionTo(attacker.Transform.Global));
                    this.StartBlocking(parent);
                    return true;

                default: return false;
            }
        }

        private void StartBlocking(GameObject parent)
        {
            if (this.BlockCooldownTimer > 0)
                return;
            this.BlockTimer = this.BlockDuration;
            //parent.GetComponent<BlockingComponent>().Start(parent);
            (parent.Net as Net.Server).AIHandler.AIStartBlock(parent);
        }
        private void StopBlocking(GameObject parent)
        {
            this.BlockCooldownTimer = this.BlockCooldown;
            //parent.GetComponent<BlockingComponent>().Stop(parent);
            (parent.Net as Net.Server).AIHandler.AIStopBlock(parent);
        }
        void SetTarget(GameObject parent, GameObject target)
        {
            if (Target == target)
                return;
            Target = target;
            //if (target != null)
            //    throw new NotImplementedException();
            //target.PostMessage(Message.Types.Aggro, parent);
        }
    }
}
