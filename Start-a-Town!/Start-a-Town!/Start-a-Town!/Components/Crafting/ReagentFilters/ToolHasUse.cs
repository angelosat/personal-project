using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_.Components.Crafting
{
    class ToolHasUse : Reaction.Reagent.ReagentFilter
    {
        Skill Skill; 
        public override string Name
        {
            get
            {
                return "Tool has use";
            }
        }
        public ToolHasUse(Skill skill)
        {
            this.Skill = skill;
        }
        public override bool Condition(GameObject obj)
        {
            return SkillComponent.HasSkill(obj, this.Skill);
        }
        public override string ToString()
        {
            return Name + ": " + this.Skill.Name;
        }
    }
}
