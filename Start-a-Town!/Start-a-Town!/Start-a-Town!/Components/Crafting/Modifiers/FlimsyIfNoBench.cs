using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class FlimsyIfNoBench : Reaction.Product.Modifier
    {
        public override void Apply(List<GameObjectSlot> materials, GameObject building, GameObject product)
        {
            if (building == null)
                return;
            if (building.GetInfo().ID > 0)
                return;
            product.Name = product.Name.Insert(0, "Flimsy ");
            //product.GetComponent<ResourcesComponent>().Resources[Resource.Types.Durability].Max = 10;
            product.GetComponent<EquipComponent>().Durability.Max = 10;
        }
    }
}
