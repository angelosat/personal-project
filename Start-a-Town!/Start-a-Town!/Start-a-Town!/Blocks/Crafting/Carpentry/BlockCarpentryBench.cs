using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Tokens;
using Start_a_Town_.Crafting;
using Start_a_Town_.AI;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Blocks
{
    partial class BlockCarpentryBench : BlockWorkstation
    {
        AtlasDepthNormals.Node.Token[] Orientations = new AtlasDepthNormals.Node.Token[4];

        public BlockCarpentryBench()
            : base(Block.Types.CarpenterBench)
        {
            this.Orientations[0] = Block.LoadTexture("counter1grayscale", "/counters/counter1");
            this.Orientations[1] = Block.LoadTexture("counter4grayscale", "/counters/counter4");
            this.Orientations[2] = Block.LoadTexture("counter3grayscale", "/counters/counter3");
            this.Orientations[3] = Block.LoadTexture("counter2grayscale", "/counters/counter2");

            this.Variations.Add(this.Orientations.First());
            this.Recipe = new BlockConstruction(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfMaterialType(MaterialType.Wood), 
                        //Reaction.Reagent.IsOfSubType(ItemSubType.Planks),
                        Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                    new BlockConstruction.Product(this),
                    Components.Skills.Skill.Building);
        }

        public override AILabor Labor { get { return AILabor.Carpenter; } }

        public override Material GetMaterial(byte blockdata)
        {
            return Material.Templates[blockdata];
        }

        public override BlockConstruction GetRecipe()
        {
            return this.Recipe;
        }
        public override List<byte> GetVariations()
        {
            var vars = (from mat in Material.Templates.Values
                        where mat.Type == MaterialType.Wood
                        select (byte)mat.ID).ToList();
            return vars;
        }
        public override Vector4 GetColorVector(byte data)
        {
            var mat = Components.Materials.Material.Templates[data];
            var c = mat.ColorVector;
            return c;
        }
        public override BlockEntityWorkstation GetWorkstationBlockEntity()
        {
            return new Entity();
        }
    }
}
