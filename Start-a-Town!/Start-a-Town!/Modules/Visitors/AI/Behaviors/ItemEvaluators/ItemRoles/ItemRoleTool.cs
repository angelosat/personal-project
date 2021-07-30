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
            var ability = item.ToolComponent?.Props?.Ability;
            if (!ability.HasValue)
                return -1;
            if (ability.Value.Def != this.ToolAbility)
                return -1;
            return ability.Value.Efficiency;
        }
        public override string ToString()
        {
            return $"{this.GetType().Name}:{this.ToolAbility.Name}";
        }
    }
}
