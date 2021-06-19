using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components
{
    class ToolComponentOld : Component
    {
        public override string ComponentName
        {
            get { return "Tool"; }
        }
        public override object Clone()
        {
            return new ToolComponentOld() { Skill = this.Skill };
        }

        public SkillOld.Types Skill { get { return (SkillOld.Types)this["Skill"]; } set { this["Skill"] = value; } }

        public ToolComponentOld()
        {

        }
        public ToolComponentOld Initialize(SkillOld.Types skill)
        {
            this.Skill = skill;
            return this;
        }
    }
}
