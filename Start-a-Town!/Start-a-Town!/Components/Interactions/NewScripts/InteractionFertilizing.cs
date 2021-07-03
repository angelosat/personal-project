using System;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Components.Interactions
{
    [Obsolete]
    class InteractionFertilizing : Interaction
    {
        public InteractionFertilizing() : base( "Fertilize",0)
        {
            this.Verb = "Fertilizing";
        }
        static readonly TaskConditions conds = new TaskConditions(
                new AllCheck(
                    new RangeCheck(t => t.Global, Interaction.DefaultRange),
                    new TargetTypeCheck(TargetType.Position),
                    new ScriptTaskCondition("HoldingObject", (a, t) => a.GetComponent<HaulComponent>().Holding.Object != null),
                    new ScriptTaskCondition("ObjectIsFertilizer", (a, t) => a.GetComponent<HaulComponent>().Holding.Object.HasComponent<FertilizerComponent>()),
                    new ScriptTaskCondition("BlockEntityExists", (a, t) => a.Map.GetBlockEntity(t.Global) != null),
                    new ScriptTaskCondition("IsFarmland", (a, t) => a.Map.GetBlock(t.Global) == BlockDefOf.Farmland),
                    new ScriptTaskCondition("IsActive", (a, t) => a.Map.GetBlockEntity(t.Global) is BlockFarmland.BlockFarmlandEntity)
                    ));
        public override TaskConditions Conditions => conds;
        public override void Perform(GameObject a, TargetArgs t)
        {
            var p = a.GetComponent<HaulComponent>().Holding.Object.GetComponent<FertilizerComponent>().Potency;
            BlockFarmland.Fertilize(a.Map, t.Global, p);
            a.GetComponent<HaulComponent>().Holding.Consume(1);
        }

        public override object Clone()
        {
            return new InteractionFertilizing();
        }
    }
}
