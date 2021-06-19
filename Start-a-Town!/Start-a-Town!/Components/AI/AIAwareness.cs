using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class AIAwareness : Behavior
    {
        float Timer { get; set; }
        float Period { get; set; }
        public AIAwareness()
        {
            this.Timer = 0;
            this.Period = Engine.TicksPerSecond;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.Timer < Period)
            {
                this.Timer++;
                // return fail so we don't block parent selector
                return BehaviorState.Fail;//.Running;
            }
            this.Timer = 0;

            UpdateNearbyObjects(parent, state.Knowledge);

            state.NearbyEntities = (
                from memory in state.Knowledge.Objects.Values
                let obj = memory.Object
                where obj.IsSpawned
                // filter by range in relation to personality (for example, a function of determination and need score?)
                orderby Vector3.Distance(parent.Global, obj.Global)
                select obj).ToList();

            // return fail so we don't block parent selector
            return BehaviorState.Fail;//.Running;
        }

        List<GameObject> UpdateNearbyObjects(GameObject parent, Knowledge knowledge)
        {
            return parent.GetNearbyObjects(
            range: range => range < Chunk.Size,// 16,
            action: obj =>
            {
                Memory mem;
                if (!knowledge.Objects.TryGetValue(obj, out mem))
                {
                    //List<InteractionOld> interactions = new List<InteractionOld>();
                    //obj.Query(parent, interactions);
                    knowledge.Objects[obj] = obj.ToMemory(parent);// new Memory(obj, 100, 100, 1, interactions.Select(i => i.Need).ToArray());
                }
                else
                    mem.Refresh(parent);
            });
        }

        public override object Clone()
        {
            return new AIAwareness();
        }
    }
}
