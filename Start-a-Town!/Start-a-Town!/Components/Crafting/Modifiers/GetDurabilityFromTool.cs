using System.Collections.Generic;

namespace Start_a_Town_.Components.Crafting
{
    class GetDurabilityFromTool : Reaction.Product.Modifier
    {
        public override void Apply(Dictionary<string, Entity> materials, GameObject building, Entity product, Entity tool)
        {
            if (tool == null)
                return;
            product.Name = product.Name.Insert(0, "Masterwork ");
            var eq = product.GetComponent<EquipComponent>();
            eq.Durability.Value = eq.Durability.Max += 10;
            //Tokens.TokensComponent.AddToken(product, new TokenMadeWithTools(10));
        }
    }
}
