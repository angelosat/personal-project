using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Crafting;
using Start_a_Town_.AI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Blocks
{
    class BlockCarpentryBench : BlockWithEntity//Workstation
    {
        readonly AtlasDepthNormals.Node.Token[] Orientations = Block.TexturesCounter;// new AtlasDepthNormals.Node.Token[4];

        public BlockCarpentryBench()
            : base(Block.Types.CarpenterBench, opaque: false, solid: true)
        {
            this.Ingredient = new Ingredient(amount: 2).IsBuildingMaterial();
            this.Variations.Add(this.Orientations.First());
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfMaterialType(MaterialType.Wood),
                        Reaction.Reagent.IsOfSubType(ItemSubType.Logs),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);
        }
        public override Graphics.AtlasDepthNormals.Node.Token GetToken(int variation, int orientation, int cameraRotation, byte data)
        {
            //return this.Variations[orientation];
            return Orientations[orientation];
        }
        //public override AILabor Labor { get { return AILabor.Carpenter; } }
        //public override bool IsDeconstructable => true;
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Registry[blockdata];
        }

        public override BlockRecipe GetRecipe()
        {
            return this.Recipe;
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            var vars = (from mat in Material.Registry.Values
                        where mat.Type == MaterialType.Wood
                        select (byte)mat.ID);
            return vars;
        }
        public override Vector4 GetColorVector(byte data)
        {
            return this.GetColorFromMaterial(data);
            //var mat = Components.Materials.Material.Templates[data];
            //var c = mat.ColorVector;
            //return c;
        }
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockCarpentryEntity();
        }
        //public override BlockEntityWorkstation CreateWorkstationBlockEntity()
        //{
        //    return new BlockCarpentryEntity();
        //}
    }
}
