﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

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
                return BehaviorState.Fail;
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
            return BehaviorState.Fail;
        }

        List<GameObject> UpdateNearbyObjects(GameObject parent, Knowledge knowledge)
        {
            return parent.GetNearbyObjects(
            range: range => range < Chunk.Size,
            action: obj =>
            {
                Memory mem;
                if (!knowledge.Objects.TryGetValue(obj, out mem))
                {
                    knowledge.Objects[obj] = obj.ToMemory(parent);
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
