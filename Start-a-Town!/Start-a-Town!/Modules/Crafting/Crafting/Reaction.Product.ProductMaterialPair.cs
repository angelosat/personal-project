using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    partial class Reaction
    {
        public partial class Product
        {
            public class ProductMaterialPair
            {
                public GameObject Product;
                public readonly Reaction Reaction;
                public GameObject Tool;
                public Dictionary<string, ObjectAmount> RequirementsNew;
               
                public ProductMaterialPair(Reaction reaction, GameObject product, Dictionary<string, ObjectAmount> ingredients)
                {
                    this.Reaction = reaction;
                    this.Product = product;
                    this.RequirementsNew = ingredients;
                }
               
                public void Write(System.IO.BinaryWriter w)
                {
                    this.Product.Write(w);
                }
               
                public void ConsumeMaterials()
                {
                    foreach (var req in this.RequirementsNew)
                    {
                        var reagentName = req.Key;
                        if (this.Reaction.GetIngredient(reagentName).IsPreserved)
                            continue;
                        var item = req.Value;
                        if (item.Amount > item.Object.StackSize)
                            throw new Exception("required amount is larger than actual amount");
                        item.Object.StackSize -= item.Amount;
                    }
                    if (this.RequirementsNew.Values.Any(o => o.Object.StackSize > 0))
                    {
                        // UNDONE reactions can preserve ingredients (like repairing)
                        //throw new Exception("leftover materials in reaction"); 
                    }
                }

                public int WorkAmount => this.Reaction.GetWorkAmount(this.RequirementsNew);
            }
        }

    }
}
