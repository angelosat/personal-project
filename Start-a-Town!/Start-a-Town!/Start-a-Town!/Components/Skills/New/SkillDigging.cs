using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Skills.New
{
    class SkillDigging : Skill
    {
        public SkillDigging()
        {
            this.Name = "Digging";
            this.Description = "Gives the ability to remove soil.";
            this.Icon = new Icon(UIManager.Icons32, 12, 32);
        }

        public override object Clone()
        {
            return new SkillDigging();
        }
    }
}
