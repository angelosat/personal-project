using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class GetDurabilityFromTool : Reaction.Product.Modifier
    {
        public override void Apply(List<GameObjectSlot> materials, GameObject building, GameObject product, GameObject tool)
        {
            if (tool == null)
                return;
            product.Name = product.Name.Insert(0, "Masterwork ");
            var eq = product.GetComponent<EquipComponent>();
            eq.Durability.Value = eq.Durability.Max += 10;
            Tokens.TokensComponent.AddToken(product, new TokenMadeWithTools(10));
        }
    }
}
