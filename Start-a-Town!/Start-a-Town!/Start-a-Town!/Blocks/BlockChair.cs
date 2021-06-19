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
    class BlockChair : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return Material.LightWood;
        }
        public BlockChair():base(Block.Types.Chair, opaque: false)
        {
            //this.MaterialType = MaterialType.Wood;
            //this.AssetNames = "furniture/chair, furniture/chairback, furniture/chairback2, furniture/chair2";
            this.Variations.Add(Block.Atlas.Load("blocks/furniture/chair", Block.HalfBlockDepthMap, Block.NormalMap));
            this.Variations.Add(Block.Atlas.Load("blocks/furniture/chairback", Block.HalfBlockDepthMap, Block.NormalMap));
            this.Variations.Add(Block.Atlas.Load("blocks/furniture/chairback2", Block.HalfBlockDepthMap, Block.NormalMap));
            this.Variations.Add(Block.Atlas.Load("blocks/furniture/chair2", Block.HalfBlockDepthMap, Block.NormalMap));

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
        public override float GetHeight(float x, float y)
        {
            return 0.5f;
        }
    }
}
