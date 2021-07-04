using Start_a_Town_.Components;
using Start_a_Town_.Blocks;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Towns.Farming
{
    class InteractionPlantSeed : Interaction
    {
        public InteractionPlantSeed()
            : base("Plant",.4f)
        {
        }

        public override void Start(GameObject a, TargetArgs t)
        {
            this.Animation = new Animation(AnimationDef.TouchItem);
            var haul = a.GetComponent<HaulComponent>();
            haul.AnimationHaul.FadeOut(this.Seconds / 2f);// 1f);
            a.AddAnimation(this.Animation);
        }

        public override void Perform(GameObject a, TargetArgs t)
        {
            var itemSlot = PersonalInventoryComponent.GetHauling(a);
            var item = itemSlot.Object;
            BlockFarmland.Plant(a.Map, t.Global, item);
            a.Net.EventOccured(Message.Types.ItemLost, a, item, 1);
            this.Finish(a, t);
        }

        public override object Clone()
        {
            return new InteractionPlantSeed();
        }
    }
}
