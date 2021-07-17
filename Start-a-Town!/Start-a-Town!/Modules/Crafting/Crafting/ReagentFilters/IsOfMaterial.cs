namespace Start_a_Town_.Components.Crafting
{
    class IsOfMaterial : Reaction.Reagent.ReagentFilter
    {
        Material Material;
        public override string Name => "Is of material";
        
        public IsOfMaterial(Material material)
        {
            this.Material = material;
        }
        public override bool Condition(Entity obj)
        {
            return obj.Body.Material == this.Material;
        }
        public override string ToString()
        {
            return Name + ": " + this.Material.ToString();
        }
    }
}
