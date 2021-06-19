using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Skills
{
    class SkillArgicultureOld : SkillOld
    {
        public SkillArgicultureOld()
            : base()
        {
            this.ID = Types.Argiculture;
            this.Name = "Argiculture";
            this.IconID = 4;
        }

        public override string Description
        {
            get
            {
                return "Helps determine type and growth time of plants.";
            }
        }


    }
}
