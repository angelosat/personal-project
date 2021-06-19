using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Components.Interactions
{
    class InteractionFertilizing : Interaction
    {
        FertilizerComponent Comp;
        //public InteractionFertilizing(FertilizerComponent comp)
        public InteractionFertilizing()
            : base(
            "Fertilize",
            0
            
            )
        {
            this.Verb = "Fertilizing";
            //this.Comp = comp;
        }
        static readonly TaskConditions conds = new TaskConditions(
                new AllCheck(
                    new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                    new TargetTypeCheck(TargetType.Position),
            //new SkillCheck(Skill.Fertilizing),
            //new ScriptTaskCondition("HoldingObject", (a, t) => a.GetComponent<GearComponent>().Holding.Object != null),
            //new ScriptTaskCondition("ObjectIsFertilizer", (a, t) => a.GetComponent<GearComponent>().Holding.Object.HasComponent<FertilizerComponent>()),
                    new ScriptTaskCondition("HoldingObject", (a, t) => a.GetComponent<HaulComponent>().Holding.Object != null),
                    new ScriptTaskCondition("ObjectIsFertilizer", (a, t) => a.GetComponent<HaulComponent>().Holding.Object.HasComponent<FertilizerComponent>()),
                    new ScriptTaskCondition("BlockEntityExists", (a, t) => a.Map.GetBlockEntity(t.Global) != null),
                    new ScriptTaskCondition("IsFarmland", (a, t) => a.Map.GetBlock(t.Global) == Block.Farmland),
                    new ScriptTaskCondition("IsActive", (a, t) => a.Map.GetBlockEntity(t.Global) is BlockFarmland.Entity)
                    ));
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            //var p = a.GetComponent<GearComponent>().Holding.Object.GetComponent<FertilizerComponent>().Potency;
            var p = a.GetComponent<HaulComponent>().Holding.Object.GetComponent<FertilizerComponent>().Potency;

            BlockFarmland.Fertilize(a.Map, t.Global, p);
            //a.GetComponent<GearComponent>().Holding.Consume(1);
            a.GetComponent<HaulComponent>().Holding.Consume(1);


            //BlockFarmland.Fertilize(a.Map, t.Global, this.Comp.Potency);
        }

        public override object Clone()
        {
            //return new InteractionFertilizing(this.Comp);
            return new InteractionFertilizing();
        }
    }
}
