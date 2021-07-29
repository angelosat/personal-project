namespace Start_a_Town_
{
    class ItemRoleTool : ItemRole
    {
        public ToolAbilityDef ToolAbility;
        public ItemRoleTool(ToolAbilityDef ttype)
        {
            this.ToolAbility = ttype;
        }

        public override int Score(Actor actor, Entity item)
        {
            //var props = item.Def.ToolProperties;
            //if (props?.Ability.Def != this.ToolAbility)
            //    return 0;
            //return props.Ability.Efficiency;

            var ability = item.ToolComponent?.Props?.Ability;
            //var ability = item.Def.ToolProperties?.Ability ?? item.ToolComponent?.Props?.Ability;
            if (!ability.HasValue)
                return -1;
            if (ability.Value.Def != this.ToolAbility)
                return -1;
            return ability.Value.Efficiency;
        }
        public override string ToString()
        {
            return string.Format("{0}: {1}", base.ToString(), this.ToolAbility.Name);
        }
    }
}
