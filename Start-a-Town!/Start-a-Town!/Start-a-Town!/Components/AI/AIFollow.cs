using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AIFollow : Behavior
    {
        GameObject Leader { get { return (GameObject)this["Target"]; } set { this["Target"] = value; } }

        public override string Name
        {
            get
            {
                return "Following: " + Leader;
            }
        }

        public override Behavior Initialize(GameObject parent)
        {
            parent.HandleMessage(Message.Types.Drop);
            Leader.HandleMessage(Message.Types.Followed, parent);
            return this;
        }

        public override Behavior Finalize(GameObject parent)
        {
            Leader.HandleMessage(Message.Types.Unfollowed, parent);
            return new AIIdle();
        }

        public AIFollow(GameObject target)
        {
            this.Leader = target;
       //     this.Time = new TimeSpan(0, 0, 1);
        }

        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            //if (Child != null)
            //{
            //    if (!Child.Execute(parent, personality, memory))
            //        return false;
            //    Child = null;
            //}
            Vector3 direction = (Leader.Global - parent.Global);

            if (direction.Length() <= 1)
                return BehaviorState.Success;// AIIdle(new TimeSpan(0, 0, 1));

            direction.Normalize();
            direction.Z = 0;
            parent.HandleMessage(Message.Types.Move, parent, direction, 1f);

            //TimeSpan sub = new TimeSpan(0, 0, 0, 0, (int)(100 * GlobalVars.DeltaTime / 6f));
            //Time = Time.Subtract(sub);
            //if (Time.TotalMilliseconds <= 0)
            //    return new AIIdle(new TimeSpan(0, 0, 1));

            return BehaviorState.Running;
        }
    }
}
