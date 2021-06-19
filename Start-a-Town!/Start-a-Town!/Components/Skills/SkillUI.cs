using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Skills
{
    class SkillUI : GroupBox
    {
        public SkillUI(GameObject item)
        {
            var comp = item.GetComponent<SkillsComponent>();

        }
    }
}
