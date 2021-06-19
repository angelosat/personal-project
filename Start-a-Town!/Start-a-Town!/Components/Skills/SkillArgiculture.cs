using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Modules.Construction;

namespace Start_a_Town_
{
    class SkillArgiculture : ToolAbilityDef
    {
        public SkillArgiculture()
            : base("Argiculture", "Helps determine type and growth time of plants.")
        {

        }

        public override Interaction GetInteraction(GameObject a, TargetArgs t)
        {
            if (t.Type != TargetType.Position)
                return null;
            if (Block.GetBlockMaterial(a.Map, t.Global) == MaterialDefOf.Soil)
                return new InteractionTilling();
            return null;
        }
    }
}
