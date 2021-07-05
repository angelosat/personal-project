using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Blocks;
using Start_a_Town_.Crafting;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class BlockPlantProcessing : BlockWithEntity//Workstation
    {
        //public override AILabor Labor => AILabor.Farmer;
        AtlasDepthNormals.Node.Token[] Orientations = Block.TexturesCounter;
        //public override bool IsDeconstructable => true;

        public BlockPlantProcessing()
            : base(Types.PlantProcessing, opaque: false, solid: true)
        {
            //this.Orientations[0] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter1").ToGrayscale(), "counter1grayscale");
            //this.Orientations[1] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter4").ToGrayscale(), "counter4grayscale");
            //this.Orientations[2] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter3").ToGrayscale(), "counter3grayscale");
            //this.Orientations[3] = Block.Atlas.Load(Game1.Instance.Content.Load<Texture2D>("graphics/items/blocks/counters/counter2").ToGrayscale(), "counter2grayscale");
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();

            this.Variations.Add(this.Orientations.First());

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfMaterialType(MaterialType.Wood),
                        //Reaction.Reagent.IsOfSubType(ItemSubType.Planks),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);

        }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Registry[blockdata];
        }
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockPlantProcessingEntity();
        }
        //public override BlockEntity GetBlockEntity(IMap map, Vector3 global)
        //{
        //    return new BlockWorkbenchEntity();
        //}
        //public override BlockEntityWorkstation CreateWorkstationBlockEntity()
        //{
        //    return new Entity();
        //}

        internal override IEnumerable<Vector3> GetOperatingPositions(Cell cell)
        {
            //foreach (var p this.Front(cell))
            //    yield return p;
            yield return Front(cell);
        }
    }
}
