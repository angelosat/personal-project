using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components.Crafting
{
    static class CraftingIngredients
    {
        static public Dictionary<string, Material> GetRandomMaterialsFor(ItemDef def)
        {
            var dic = new Dictionary<string, Material>();
            foreach (var r in def.CraftingProperties.Reagents)
            {
                var i = r.Value.Ingredient.GetAllValidItemDefs();
                var m = i.SelectMany(i => i.GetValidMaterials());
                var md = m.Distinct();
                var mat = md.ToArray().SelectRandom(Material.Randomizer);
                dic[r.Value.Name] = mat;
            }
            return dic;
        }
    }
}
