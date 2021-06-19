using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Blocks
{
    class BlockStool : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return Material.LightWood;
        }

        public BlockStool()
            : base(Block.Types.Stool, opaque: false)
        {
            //this.MaterialType = MaterialType.Wood;
            //this.AssetNames = "furniture/stool";
            this.Variations.Add(Block.Atlas.Load("blocks/furniture/stool", Map.BlockDepthMap, Block.NormalMap));
            //this.Material = Material.LightWood;

            this.Recipe = new BlockConstruction(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                //Reaction.Reagent.IsOfMaterialType(MaterialType.Wood), 
                        Reaction.Reagent.IsOfSubType(ItemSubType.Planks),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockConstruction.Product(this),
                    Components.Skills.Skill.Building);
        }
        //public override float GetHeight(float x, float y)
        //{
        //    return 0.5f;
        //}
    }
}