namespace Start_a_Town_
{
    class Tool : Entity
    {
        //ToolAbilityComponent _toolComponent;
        //public ToolAbilityComponent ToolComponent => this._toolComponent ??= this.GetComponent<ToolAbilityComponent>();
        
        public Tool(ItemDef def)
            : base(def)
        {
            this.AddComponent(new ResourcesComponent(ResourceDef.Durability));
            this.AddComponent(new OwnershipComponent());
            this.AddComponent(new ToolAbilityComponent());
        }
        public override GameObject Create()
        {
            return new Tool(this.Def);
        }
    }
}
