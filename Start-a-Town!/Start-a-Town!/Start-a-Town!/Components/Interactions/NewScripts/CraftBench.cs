using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Components.Interactions
{
    class CraftBench : Interaction
    {
        //GameObject Bench;
        Reaction.Product.ProductMaterialPair Product;

        //public Craft(GameObject bench)
        public CraftBench(Reaction.Product.ProductMaterialPair product)
            : base(
            "Craft",
            1,
            new TaskConditions(
                new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                new TargetTypeCheck(TargetType.Entity))
            )
        {
            //this.Bench = bench;
            this.Product = product;
        }

        public override void Perform(GameObject actor, TargetArgs target)
        {
            var benchEntity = target.Object;
            WorkbenchReactionComponent bench = benchEntity.GetComponent<WorkbenchReactionComponent>();
            //var product = comp.SelectedReaction.Products.First().GetProduct(comp.SelectedReaction, Player.Actor, comp.Slots);

            // check materials and consume them if they exist, otherwise return
            foreach (var mat in this.Product.Requirements)
                if (!bench.Slots.Check(mat.ObjectID, mat.Amount))
                    return;
            foreach (var mat in this.Product.Requirements)
                //bench.Slots.Remove(mat.ObjectID, mat.Amount);
                bench.Slots.Consume(mat.ObjectID, mat.Max);


            bench.Craft(benchEntity, this.Product);
        }

        public override object Clone()
        {
            //return new Craft(this.Bench);
            return new CraftBench(this.Product);
        }
    }
}
