using Start_a_Town_.Components;

namespace Start_a_Town_.Blocks.Bed
{
    class InteractionGetOutOfBed : Interaction
    {
        public InteractionGetOutOfBed()
            : base("Get out of bed")
        {

        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            a.GetComponent<SpriteComponent>().Body = null;          
        }
        public override object Clone()
        {
            return new InteractionGetOutOfBed();
        }
    }
}
