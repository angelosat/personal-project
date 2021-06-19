using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AIChase : Behavior
    {
        GameObject Target { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }
        TimeSpan Time { get { return (TimeSpan)this["Time"]; } set { this["Time"] = value; } }

        public override string Name
        {
            get
            {
                return "Chasing " + Target;
            }
        }

        public AIChase(GameObject target)
        {
            this.Target = target;
            this.Time = new TimeSpan(0, 0, 1);
        }

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //if (Child != null)
            //{
            //    if (!Child.Execute(parent, personality, memory))
            //        return false;
            //    Child = null;
            //}
            Vector3 direction = (Target.Global - parent.Global);

            if (direction.Length() <= 1)
            {
               // Child = new AIAttack(Target);// AIIdle(new TimeSpan(0, 0, 1));
                return BehaviorState.Success;
            }

            direction.Normalize();
            direction.Z = 0;
            parent.HandleMessage(Message.Types.Move, parent, direction, 1f);

            

            TimeSpan sub = new TimeSpan(0, 0, 0, 0, (int)(100 * GlobalVars.DeltaTime / 6f));
            Time = Time.Subtract(sub);
            if (Time.TotalMilliseconds <= 0)
            {
             //   Child = new AIIdle(new TimeSpan(0, 0, 1));
                return BehaviorState.Success;
            }

            return BehaviorState.Running;
        }
    }
}
