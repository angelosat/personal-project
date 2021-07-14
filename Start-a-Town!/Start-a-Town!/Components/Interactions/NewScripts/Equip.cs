namespace Start_a_Town_
{
    class Equip : InteractionPerpetual
    {
        static public int ID = "Equip".GetHashCode();

        public Equip()
            : base("Equip")
        {
        }

        public override void Start(Actor a, TargetArgs t)
        {
            a.CrossFade(this.Animation, false, 25);
        }

        static readonly TaskConditions conds = new(new AllCheck(new RangeCheck(t => t.Global, Interaction.DefaultRange)));
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override void OnUpdate(Actor a, TargetArgs t)
        {
            GearComponent.EquipToggle(a as Actor, t.Object as Entity);
            this.Finish(a, t);
        }

        public override object Clone()
        {
            return new Equip();
        }
    }
}
