namespace Start_a_Town_
{
    class ItemRoleGear : ItemRole
    {
        public GearType GearType;
        public override object Tag => this.GearType;

        public ItemRoleGear(GearType gtype)
        {
            this.GearType = gtype;
        }

        public override int Score(Actor actor, Entity item)
        {
            var props = item.Def.ApparelProperties;
            if (props?.GearType != this.GearType)
                return 0;
            return props.ArmorValue;
        }
        public override string ToString()
        {
            return $"{this.GetType().Name}:{this.GearType.Name}";
        }
    }
}
