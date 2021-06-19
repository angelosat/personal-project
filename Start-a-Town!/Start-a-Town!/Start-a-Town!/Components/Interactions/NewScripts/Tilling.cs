using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Skills;

namespace Start_a_Town_.Components.Interactions
{
    class Tilling : Interaction
    {
        public Tilling()
            : base(
            "Till",
            .5f
            
            )
        {
            this.Animation = new Graphics.Animations.AnimationTool();
            this.Verb = "Tilling";
            // TODO:
            //this.CancelState = new BlockCheck(b=> b.GetMaterial(, .Type == MaterialType.Soil);
        }
        static readonly TaskConditions conds = new TaskConditions(
                new AllCheck(
                    new AllCheck(
                        new TargetTypeCheck(TargetType.Position),
                        new ScriptTaskCondition("Material", (a, t) => //a.Map.GetBlock(t.Global).Material.Type == MaterialType.Soil)),
                        {
                            //return Block.GetBlockMaterial(a.Map, t.Global).Type == MaterialType.Soil;
                            var block = a.Map.GetBlock(t.Global);
                            var mat = Block.GetBlockMaterial(a.Map, t.Global);
                            return mat.Type == MaterialType.Soil;
                        })),
                        new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                        new SkillCheck(Skill.Argiculture)
                    ));
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        //public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        //{
        //    //return new SkillCheck(Skill.Digging).Condition(actor, target);
        //    return this.Conditions.GetFailedCondition(actor, target) == null;
        //}

        public override void Perform(GameObject a, TargetArgs t)
        {
            a.Net.SetBlock(t.Global, Block.Types.Farmland, 0);
        }

        public override object Clone()
        {
            return new Tilling();
        }
    }
}
