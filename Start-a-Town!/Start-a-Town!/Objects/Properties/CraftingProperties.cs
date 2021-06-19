using Start_a_Town_.Components.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class CraftingProperties
    {
        //public ReactionClass ReactionClass;
        public Dictionary<BoneDef, Reaction.Reagent> Reagents = new Dictionary<BoneDef, Reaction.Reagent>();
        public List<Ingredient> Ingredients = new();
        public bool IsBuildingMaterial;
        public bool IsCraftingMaterial;
        readonly List<ItemCategory> MadeFrom = new();

        public CraftingProperties MakeableFrom(params ItemCategory[] cat)
        {
            this.MadeFrom.AddRange(cat);
            return this;
        }
        public bool CanBeMadeFrom(ItemDef def)
        {
            return this.MadeFrom.Contains(def.Category);
        }
        public CraftingProperties AddIngredient(Ingredient ing)
        {
            this.Ingredients.Add(ing);
            return this;
        }
    }
}
