using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Modules.Construction;

namespace Start_a_Town_.Components.Skills
{
    class SkillArgiculture : Skill
    {
        public SkillArgiculture()
            : base("Argiculture", "Helps determine type and growth time of plants.")
        {

        }

        public override Interactions.Interaction GetWork(GameObject a, TargetArgs t)
        {
            if (t.Type != TargetType.Position)
                return null;
            if (Block.GetBlockMaterial(a.Map, t.Global) == Material.Soil)
                return new Tilling();
            return null;
        }
    }
}
