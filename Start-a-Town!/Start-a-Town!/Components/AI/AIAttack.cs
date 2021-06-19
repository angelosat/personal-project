using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;

namespace Start_a_Town_.AI.Behaviors
{
    class AIAttack : Behavior
    {
        GameObject Target;// { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }

        public override string Name
        {
            get
            {
                return "Attacking " + (Target == PlayerOld.Actor ? "YOU!" : Target.Name);
            }
        }

        public AIAttack(GameObject target = null)
        {
            this.Target = target;
        }

        public override BehaviorState Execute(Actor parent, AIState state)//IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //if (Child != null)
            //{
            //    if (!Child.Execute(parent, personality, memory))
            //        return false;
            //    Child = null;
            //}
            //if ((Target.Global - parent.Global).Length() > 1)
            //    return new AIChase(Target);
            if (Target == null)
                return BehaviorState.Success;
            Vector3 difference = (Target.Global - parent.Global);
            float dist = difference.Length();
            if (dist > 1)
            {
                if (dist > 5)
                {
                    //parent["AI"]["Current"] = new AIIdle();
                    return BehaviorState.Success;
                }
                difference.Normalize();
                difference.Z = 0;
                throw new NotImplementedException();
                //parent.PostMessage(Message.Types.Move, parent, difference, 1f);
                //return BehaviorState.Running;
            }
            throw new NotImplementedException();
            //parent.PostMessage(Message.Types.Begin, parent, Target, Message.Types.Attack);
            //if ((float)Target["Health"]["Value"] > 0)
            //    //     parent["AI"]["Current"] = new AIFindTarget(5, foo => foo.Type == ObjectType.Human); //return new AIIdle();//new TimeSpan(0, 0, 1));
            //    return BehaviorState.Running;
            //return BehaviorState.Success;
        }

        public override object Clone()
        {
            return new AIAttack();
        }
    }
}
