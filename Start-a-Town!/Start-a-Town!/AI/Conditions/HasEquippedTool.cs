using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class HasEquippedTool : BehaviorCondition
    {
        string SkillKey;
        public HasEquippedTool(string skillKey)
        {
            this.SkillKey = skillKey;
        }
        public override bool Evaluate(GameObject agent, AIState state)
        {
            var equip = GearComponent.GetSlot(agent, GearType.Mainhand);
            if (equip.Object == null)
                return false;
            var skillid = (int)state[this.SkillKey];
            //var skill = Components.Skills.Skill.Dictionary[skillid];
            return ToolAbilityComponent.HasSkill(equip.Object, skillid);
        }
    }
}
