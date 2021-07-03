using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Vegetation;

namespace Start_a_Town_.Towns.Forestry
{
    class InteractionPlantTree : Interaction
    {
        public InteractionPlantTree()
            : base("PlantTree", 2)
        {
        }

        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                RangeCheck.One,
                new BlockCheck(b => b == BlockDefOf.Soil || b == BlockDefOf.Grass),
                new IsHauling(foo => { if (foo != null)return foo.HasComponent<SaplingComponent>(); else return false; })
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
            var slot = PersonalInventoryComponent.GetHauling(a);
            slot.Consume();
            BlockDefOf.Sapling.Place(a.Map, t.Global + Vector3.UnitZ, 0, 0, 0);
        }

        public override object Clone()
        {
            return new InteractionPlantTree();
        }
    }
}
