using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Start_a_Town_.Crafting;
using Start_a_Town_.Graphics;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.AI;
using Start_a_Town_.Tokens;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class BlockKitchen : BlockWithEntity, IBlockWorkstation//Workstation
    {
        AtlasDepthNormals.Node.Token[] Orientations = Block.TexturesCounter;
        public BlockKitchen()
            : base(Block.Types.Kitchen, opaque: false, solid: true)
        {
            this.Variations.Add(this.Orientations.First());
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfMaterialType(MaterialType.Wood),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);
            this.Ingredient = new Ingredient(amount: 4).IsBuildingMaterial();
        }
       
        public override Graphics.AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            return this.Orientations[orientation];
        }
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockKitchenEntity();
           
        }

        public override BlockRecipe GetRecipe()
        {
            return this.Recipe;
        }
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Database[blockdata];
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            var vars = (from mat in Material.Database.Values
                        where mat.Type == MaterialType.Wood
                        select (byte)mat.ID);
            return vars;
        }
        public override Vector4 GetColorVector(byte data)
        {
            return this.GetColorFromMaterial(data);
            
        }
       
    }
}
