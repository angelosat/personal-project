using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Crafting
{
    class CraftingIngredients
    {
        public Dictionary<string, Entity> Ingredients;

        public Entity GetIngredient(string index)
        {
            return this.Ingredients[index];
        }
        public Entity GetIngredient(ReactionIngredientIndex index)
        {
            return this.Ingredients[index.Label];
        }

        static public Dictionary<string, Material> GetRandomMaterialsForOld(ItemDef def)
        {
            var dic = new Dictionary<string, Material>();
            foreach (var r in def.CraftingProperties.Reagents)//.CraftingIngredientIndices)
            {
                dic[r.Value.Name] = Material.GetRandom();
            }
            return dic;
        }
        static public Dictionary<string, Material> GetRandomMaterialsFor(ItemDef def)
        {
            var dic = new Dictionary<string, Material>();
            foreach (var r in def.CraftingProperties.Reagents)//.CraftingIngredientIndices)
            {
                var i = r.Value.Ingredient.GetAllValidItemDefs();
                var m = i.SelectMany(i => i.GetValidMaterials());
                var md = m.Distinct();
                var mat = md.ToArray().SelectRandom(Material.Randomizer);
                dic[r.Value.Name] = mat;
                //dic[r.Value.Name] = Material.GetRandom();
            }
            return dic;
        }
    }
}
