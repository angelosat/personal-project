using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.Components.Crafting
{
    partial class Reaction
    {
        public int Complexity { get; private set; }

        public partial class Product
        {
            public enum Types { Tools, Workbenches, Furniture, Blocks }
            public Func<Dictionary<string, Entity>, Entity> ObjectCreator;
            readonly List<Action<Dictionary<string, Entity>, Entity>> ModifierActions = new();
            public int Quantity = 1;
            public Product(string ingredientCopy)
            {
                this.ObjectCreator = dic => dic[ingredientCopy];
            }
            public Product(Func<Dictionary<string, Entity>, Entity> creator, int quantity = 1)
            {
                this.ObjectCreator = creator;
                this.Quantity = quantity;
            }
            public Product(ItemDef def, int quantity = 1)
            {
                this.ObjectCreator = mats=> def.Factory(def);
                this.Quantity = quantity;
            }
            public Product GetMaterialFromIngredient(string reagentName)
            {
                this.ModifierActions.Add((mats, e) => e.SetMaterial(mats[reagentName].PrimaryMaterial));
                return this;
            }
            public ProductMaterialPair Make(Actor actor, Reaction reaction, List<ObjectAmount> materials)
            {
                Dictionary<string, ObjectAmount> ingrs = CalculateIngredients(reaction, materials);
                return this.Make(actor, reaction, ingrs);
            }
            public ProductMaterialPair Make(Actor actor, Reaction reaction, Dictionary<string, ObjectAmount> ingrs)
            {
                if (actor.Net is Net.Client)
                    return new ProductMaterialPair(reaction, null, ingrs);
                var dic = ingrs.ToDictionary(o => o.Key, o => o.Value.Object as Entity);
                var product = this.ObjectCreator(dic);
                foreach (var a in this.ModifierActions)
                    a(dic, product);
                product.SetQuality(DetermineQuality(actor, reaction));
                if (ingrs.Values.All(i => i.Object.Def.StackDimension == 1)) // TODO refactor the way product quantity is determined from ingredient dimensions
                    product.SetStackSize(this.Quantity);
                var prodpair = new ProductMaterialPair(reaction, product, ingrs);
                return prodpair;
            }

            private static Quality DetermineQuality(Actor actor, Reaction reaction)
            {
                var craftingLvl = actor.GetSkill(reaction.CraftSkill).Level;
                int complexity = reaction.Complexity;
                var masteryRatio = craftingLvl / (float)complexity;
                var quality = Quality.GetRandom(actor.Map.World.Random, masteryRatio);
                return quality;
            }

            private static Dictionary<string, ObjectAmount> CalculateIngredients(Reaction parent, List<ObjectAmount> materials)
            {
                var ingrs = new Dictionary<string, ObjectAmount>();
                foreach (var reagent in parent.Reagents)
                {
                    foreach (var mat in materials)
                    {
                        var amountRequired = reagent.Quantity * mat.Object.Def.StackDimension;
                        if (mat.Amount < amountRequired)
                            continue;
                        if (!reagent.Filter(mat.Object))
                            continue;
                        var name = reagent.Name;
                        if (ingrs.TryGetValue(name, out var existing))
                            existing.Amount += amountRequired;
                        else
                            ingrs[name] = new ObjectAmount(mat.Object, amountRequired);
                        mat.Amount -= amountRequired;
                        if (mat.Amount < 0)
                            throw new Exception();
                        var amountFound = ingrs[name].Amount;
                        if (amountFound > amountRequired)
                            throw new Exception();
                        else if (amountFound == amountRequired)
                        {
                            break;
                        }
                    }
                }

                //check if all mats found
                //foreach (var reagent in parent.Reagents)
                //{
                //}

                return ingrs;
            }
           
            static public List<Product> Create(params Product[] product)
            {
                return new List<Product>(product);
            }

            public Product RestoreDurability()
            {
                this.ModifierActions.Add((mats, prod) => prod.GetResource(ResourceDef.Durability).Percentage = 1);
                return this;
            }
        }
    }
}
