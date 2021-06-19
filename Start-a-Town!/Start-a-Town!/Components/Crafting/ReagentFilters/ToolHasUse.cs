using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_.Components.Crafting
{
    class ToolHasUse : Reaction.Reagent.ReagentFilter
    {
        ToolAbilityDef Skill; 
        public override string Name
        {
            get
            {
                return "Tool has use";
            }
        }
        public ToolHasUse(ToolAbilityDef skill)
        {
            this.Skill = skill;
        }
        public override bool Condition(Entity obj)
        {
            return ToolAbilityComponent.HasSkill(obj, this.Skill);
        }
        public override string ToString()
        {
            return Name + ": " + this.Skill.Name;
        }
    }
}
