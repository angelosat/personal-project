using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Components.Interactions
{
    class Planting : Interaction
    {
        public Planting()
            : base(
            "Plant",
            0
            )
        {
            this.Verb = "Planting";
        }
        static readonly TaskConditions conds = new TaskConditions(
                new AllCheck(
                    new RangeCheck(t => t.Global, Interaction.DefaultRange),
                    new TargetTypeCheck(TargetType.Position),
            //new SkillCheck(Skill.Planting),
            //new ScriptTaskCondition("HoldingObject", (a, t) => a.GetComponent<GearComponent>().Holding.Object != null),
            //new ScriptTaskCondition("ObjectIsSeed", (a, t) => a.GetComponent<GearComponent>().Holding.Object.HasComponent<SeedComponent>()),
                                        new ScriptTaskCondition("HoldingObject", (a, t) => a.GetComponent<HaulComponent>().Holding.Object != null),
                    new ScriptTaskCondition("ObjectIsSeed", (a, t) => a.GetComponent<HaulComponent>().Holding.Object.HasComponent<SeedComponent>()),
            //new ScriptTaskCondition("BlockEntityExists", (a, t) => t.Global.GetBlockEntity(a.Map) != null),
            //new ScriptTaskCondition("IsFarmland", (a, t) => t.Global.GetBlock(a.Map) == Block.Farmland)))
                    new ScriptTaskCondition("BlockEntityExists", (a, t) => a.Map.GetBlockEntity(t.Global) != null),
                    new IsBlockType(Block.Types.Farmland)
            //new ScriptTaskCondition("IsFarmland", (a, t) => a.Map.GetBlock(t.Global) == Block.Farmland)))
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
            // old
            //a.Net.SetBlock(t.Global, Block.Types.Farmland, 0);
            //var farmland = t.Global.GetBlockEntity(a.Map) as Farmland.Entity; // TODO: send it a message instead of manipulating it here?

            // newer
            //var farmland = a.Map.GetBlockEntity(t.Global) as BlockFarmland.Entity; // TODO: send it a message instead of manipulating it here?
            //farmland.Plant(a.GetComponent<GearComponent>().Holding.Object);

            //BlockFarmland.Plant(a.Map, t.Global, a.GetComponent<GearComponent>().Holding.Object);
            //a.GetComponent<GearComponent>().Holding.Consume(1);
            var item = a.GetComponent<HaulComponent>().Holding.Object;
            BlockFarmland.Plant(a.Map, t.Global, a.GetComponent<HaulComponent>().Holding.Object);
            a.GetComponent<HaulComponent>().Holding.Consume(1);
            a.Net.EventOccured(Message.Types.ItemLost, a, item, 1);
            // TODO: consume object
        }

        public override object Clone()
        {
            return new Planting();
        }
    }
}
