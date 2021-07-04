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
            var props = item.Def.ToolProperties;
            if (props?.Ability.Def != this.ToolAbility)
                return 0;
            return props.Ability.Efficiency;
        }
        public override string ToString()
        {
            return string.Format("{0}: {1}", base.ToString(), this.ToolAbility.Name);
        }
    }
}
