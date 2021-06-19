using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class GetMaterialFromReagent : Reaction.Product.Modifier
    {
        //public MaterialName(string localMaterialName):base(loc)
        //{
        //    this.LocalMaterialName = localMaterialName;
        //}
        public GetMaterialFromReagent(string localMaterialName)
            : base(localMaterialName)
        {

        }
        public override void Apply(Entity material, Entity product)
        {
            Material mat = material.GetComponent<ItemCraftingComponent>().Material;
            product.Name = product.Name.Insert(0, mat.Prefix + " ");
            product.GetComponent<ItemCraftingComponent>().Material = mat;
        }
    }
}
