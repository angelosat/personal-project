using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class Quantity : Reaction.Product.Modifier
    {
        //public MaterialName(string localMaterialName):base(loc)
        //{
        //    this.LocalMaterialName = localMaterialName;
        //}
        int Value = 1;
        public Quantity(int quantity)
        {
            this.Value = quantity;
        }

        public override void Apply(Dictionary<string, Entity> materials, GameObject building, Entity product, Entity tool)
        {
            product.StackSize = this.Value;
        }
    }
}
