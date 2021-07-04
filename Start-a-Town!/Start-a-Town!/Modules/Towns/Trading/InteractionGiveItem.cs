using System.IO;
using Start_a_Town_.Animations;
namespace Start_a_Town_
{
    class InteractionGiveItem : Interaction
    {
        bool Trade;
        public override object Clone()
        {
            return new InteractionGiveItem(this.Trade);
        }
        public InteractionGiveItem(bool trade = false) : base("GiveItem", seconds: .4f)
        {
            this.Trade = trade;
        }
        public InteractionGiveItem() : base("GiveItem", seconds: .4f)
        {
        }
        public override void Start(GameObject a, TargetArgs t)
        {
            this.Animation = new Animation(AnimationDef.TouchItem);
            a.CrossFade(this.Animation, false, 25);
        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            var actor = a as Actor;
            var item = a.Carried as Entity;
            var seller = t.Object as Actor;
            var sellerCarriedItem = seller.Carried as Entity;
            seller.Carry(item);
            if(this.Trade)
                actor.Carry(sellerCarriedItem);
        }
        protected override void WriteExtra(BinaryWriter w)
        {
            w.Write(this.Trade);
        }
        protected override void ReadExtra(BinaryReader r)
        {
            this.Trade = r.ReadBoolean();
        }
    }
}
