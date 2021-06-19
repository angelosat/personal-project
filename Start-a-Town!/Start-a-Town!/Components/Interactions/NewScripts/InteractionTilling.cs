using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_.Components.Interactions
{
    class InteractionTilling : InteractionPerpetual
    {
        public InteractionTilling()
            : base(
            "Till"
            )
        {
          
        }

        //static readonly TaskConditions conds = new TaskConditions(
        //        new AllCheck(
        //            new AllCheck(
        //                new TargetTypeCheck(TargetType.Position),
        //                new ScriptTaskCondition("Material", (a, t) => //a.Map.GetBlock(t.Global).Material.Type == MaterialType.Soil)),
        //                {
        //                    //return Block.GetBlockMaterial(a.Map, t.Global).Type == MaterialType.Soil;
        //                    var block = a.Map.GetBlock(t.Global);
        //                    var mat = Block.GetBlockMaterial(a.Map, t.Global);
        //                    return mat.Type == MaterialType.Soil;
        //                })),
        //                new RangeCheck(t => t.Global + Vector3.UnitZ, RangeCheck.DefaultRange),//InteractionOld.DefaultRange),
        //                new SkillCheck(ToolAbilityDef.Argiculture)
        //            ));
        
        public override void OnUpdate(GameObject a, TargetArgs t)
        {
            a.Map.SetBlock(t.Global, Block.Types.Farmland, 0);
            this.Finish(a, t);
        }

        public override object Clone()
        {
            return new InteractionTilling();
        }
    }
}
