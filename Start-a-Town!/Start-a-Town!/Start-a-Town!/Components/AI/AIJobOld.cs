using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.AI
{
    class AIJobOld : Behavior
    {
        GameObjectSlot JobSlot { get { return (GameObjectSlot)this["JobSlot"]; } set { this["JobSlot"] = value; } }
        Stack<InteractionOld> Interactions { get { return (Stack<InteractionOld>)this["Interactions"]; } set { this["Interactions"] = value; } }

        public override string Name
        {
            get
            {
                return "Job";
            }
        }

        public AIJobOld()
        {
            this.JobSlot = GameObjectSlot.Empty;
            this.Interactions = new Stack<InteractionOld>();
        }

        public override BehaviorState Execute(GameObject parent, AIState state)//Net.IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            
            return BehaviorState.Running;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Give:
                    GameObjectSlot objSlot = e.Parameters[0] as GameObjectSlot;
                    if (!objSlot.HasValue)
                        return true;
                    JobComponent jobComp;
                    if (!objSlot.Object.TryGetComponent<JobComponent>("Job", out jobComp))
                        return true;
                    Interactions = new Stack<InteractionOld>(jobComp.Tasks);

                    return true;

                //case Message.Types.Query:
                //    List<Interaction> list = e.Parameters[0] as List<Interaction>;

                //    return true;
                default:
                    return false;
            }
        }

        public override object Clone()
        {
            return new AICoop();
        }
    }
}
