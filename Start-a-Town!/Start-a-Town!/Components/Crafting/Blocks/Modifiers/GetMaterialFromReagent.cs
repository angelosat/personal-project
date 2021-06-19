using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting.Blocks
{
    class GetMaterialFromReagent : BlockRecipe.Product.Modifier
    {
        string LocalMaterial { get; set; }
        Action<Material> Action { get; set; }
        Func<Material, byte> Function { get; set; }
        public GetMaterialFromReagent(string localMaterialName) : base(localMaterialName) { }
        public GetMaterialFromReagent(string localMaterialName, Func<Material, byte> function)
            : base(localMaterialName)
        {
            this.Function = function;
        }
        //public GetMaterialFromReagent(string localMaterialName, Action<Material> action)
        //    : base(localMaterialName)
        //{
        //    this.Action = action;
        //}
        public Material Apply(GameObject reagent)
        {
            return reagent.GetComponent<ItemCraftingComponent>().Material;
        }
        //public void Apply(GameObject reagent, Action<Material> action)
        //{
        //    action(reagent.GetComponent<MaterialComponent>().Material);
        //}
        public void Apply(GameObject reagent, Action<Material> action)
        {
            action(reagent.GetComponent<ItemCraftingComponent>().Material);
        }
        public override void Apply(GameObject reagent, ref byte data)
        {

        }
    }
}
