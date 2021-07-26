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

        public override void OnUpdate(Actor a, TargetArgs t)
        {
            GearComponent.EquipToggle(a, t.Object as Entity);
            this.Finish(a, t);
        }

        public override object Clone()
        {
            return new Equip();
        }
    }
}
