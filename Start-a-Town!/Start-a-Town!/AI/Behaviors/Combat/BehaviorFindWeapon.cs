using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorFindWeapon : Behavior
    {
        string FoundEquipmentKey;
        public BehaviorFindWeapon(string foundEquipmentKey)
        {
            this.FoundEquipmentKey = foundEquipmentKey;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var items = state.NearbyEntities;
            var weapons = items.Where(i => i.HasComponent<WeaponComponent>());
            GameObject best = null;
            float maxDmg = 0;
            foreach (var weap in weapons)
            {
                var currentDmg = WeaponComponent.GetTotalDamage(weap);
                if (currentDmg >= maxDmg)
                {
                    best = weap;
                    maxDmg = currentDmg;
                }
            }
            if (best != null)
            {
                state[this.FoundEquipmentKey] = new TargetArgs(best);
                return BehaviorState.Success;
            }
            else
                return BehaviorState.Fail;
            //throw new NotImplementedException();
        }
        public override object Clone()
        {
            return new BehaviorFindWeapon(this.FoundEquipmentKey);
        }
    }
}
