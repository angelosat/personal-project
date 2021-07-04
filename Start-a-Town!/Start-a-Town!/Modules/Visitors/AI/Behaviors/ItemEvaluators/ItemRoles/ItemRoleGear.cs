using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class ItemRoleGear : ItemRole
    {
        public GearType GearType;
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
            return string.Format("{0}: {1}", base.ToString(), this.GearType.Name);
        }
    }
}
