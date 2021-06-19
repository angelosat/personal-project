using Start_a_Town_.Components.Crafting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class RecipeProperties
    {
        //public List<Ingredient> Ingredients = new();

        //public RecipeProperties AddIngredient(Ingredient ing)
        //{
        //    this.Ingredients.Add(ing);
        //    return this;
        //}

        //Dictionary<string, Func<ItemDef, string, Ingredient>> IngredientMakers = new();
        //Dictionary<string, Func<ItemDef, Reaction.Product>> ProductMakers = new();
        List<Func<ItemDef, Ingredient>> IngredientMakers = new();
        List<Func<ItemDef, Reaction.Product>> ProductMakers = new();

        //List<Func<ItemDef, Ingredient>> IngredientMakers = new();
        List<IsWorkstation.Types> Workstations = new();
        public ItemCategory IngredientCategory;
        public string Verb;
        public string IngredientName;
        public JobDef Job;

        public RecipeProperties(string verb, ItemCategory ingCat)
        {
            this.Verb = verb;
            this.IngredientCategory = ingCat;
        }
        public RecipeProperties(string verb)
        {
            this.Verb = verb;
        }
        //public RecipeProperties AddIngredientMaker(string name, Func<ItemDef, string, Ingredient> maker)
        //{
        //    this.IngredientMakers.Add(name, maker);
        //    return this;
        //}
        public RecipeProperties AddIngredientMaker(Func<ItemDef, Ingredient> maker)
        {
            this.IngredientMakers.Add(maker);
            return this;
        }
        //public RecipeProperties AddProductMaker(string name, Func<ItemDef, Reaction.Product> productMaker)
        //{
        //    this.ProductMakers.Add(name, productMaker);
        //    return this;
        //}
        public RecipeProperties AddProductMaker(Func<ItemDef, Reaction.Product> productMaker)
        {
            this.ProductMakers.Add(productMaker);
            return this;
        }
        public RecipeProperties AddWorkstation(IsWorkstation.Types station)
        {
            this.Workstations.Add(station);
            return this;
        }
        public IEnumerable<Ingredient> MakeIngredients(ItemDef def)
        {
            //foreach (var maker in this.IngredientMakers)
            //    yield return maker(def);
            foreach (var maker in this.IngredientMakers)
                yield return maker(def);
                //yield return maker.Value(def, maker.Key);
        }
        public IEnumerable<Reaction.Product> MakeProducts(ItemDef def)
        {
            //foreach (var maker in this.IngredientMakers)
            //    yield return maker(def);
            foreach (var maker in this.ProductMakers)
                yield return maker(def);
                //yield return maker.Value(def);

        }
        public Reaction CreateRecipe(ItemDef def)
        {
            return new Reaction($"{this.Verb} {def.Label}", this.Job, this.Workstations.ToArray())
                .AddIngredients(this.MakeIngredients(def))
                .AddProduct(this.MakeProducts(def));


            //return new Reaction($"{this.Verb} {def.Label}", this.Job, this.Workstations.ToArray())
            //        .AddIngredient(
            //            new Ingredient(this.IngredientName)
            //            .SetAllow(def.MadeFrom, true)
            //            .SetAllow(this.IngredientCategory, true))
            //        .AddProduct(
            //            new Reaction.Product(def)
            //            .GetMaterialFromIngredient(this.IngredientName)
            //        );
        }
    }
}
