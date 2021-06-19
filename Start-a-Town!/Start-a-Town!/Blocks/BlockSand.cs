using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Tokens;
using Start_a_Town_.Particles;

namespace Start_a_Town_.Blocks
{
    class BlockSand : Block
    {
        public override bool IsMinable => true;
        //new Block(Block.Types.Sand, GameObject.Types.Sand) { Material = Material.Sand, AssetNames = "sand1" };
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Sand;
        }
        public BlockSand()
            : base(Block.Types.Sand, GameObject.Types.Sand)
        {
            //this.Material = Material.Sand;
            this.AssetNames = "sand1";

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Soil), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockRecipe.Product(this)
                );

            var reag = Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Soil), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks)));
            var prod = Reaction.Product.Create(new Reaction.Product(mats => BlockEntityPacked.Create(BlockDefOf.Sand, 0)));
            //Reaction crafted = new Reaction(
            //    "Sand",
            //    //Reaction.CanBeMadeAt(ItemTemplate.Workbench.ID),
            //    Reaction.CanBeMadeAt(IsWorkstation.Types.Workbench),
            //    reag,
            //    //Reaction.Product.Create(new Reaction.Product(MaterialType.Bars))
            //    prod
            //);
            this.Ingredient = new Ingredient(RawMaterialDef.Bags, MaterialDefOf.Sand, null, 1);
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfMaterial(MaterialDefOf.Sand)//.IsOfSubType(ItemSubType.Planks, ItemSubType.Bars)
                        )),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Walls.Add(this.Recipe);
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            yield return 0;
            //return new List<byte>() { 0 };
        }
        public override ParticleEmitterSphere GetEmitter()
        {
            //var e = base.GetDirtEmitter();
            var e = base.GetDustEmitter();

            e.ColorBegin = e.ColorEnd = Color.Gold;
            //e.ParticleWeight = 0;
            //e.HasPhysics = false;
            return e;
        }
        protected override GameObject ToObject()
        {
            return this.Create(MaterialDefOf.Sand);
        }
        GameObject Create(Material mat)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<DefComponent>().Initialize(Block.EntityIDRange + (int)this.Type, ObjectType.Block, this.GetName());
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            //obj.AddComponent<BlockComponent>().Initialize(this);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite(this.Variations.First().Name, Map.BlockDepthMap)
            {
                OriginGround = Block.OriginCenter,
                Joint = Block.Joint,
                MouseMap = BlockMouseMap,
                Overlays = new Dictionary<string, Sprite>() { { "Body", new Sprite(this.Variations.First().Name, Map.BlockDepthMap) { Tint = mat.Color } } }
            });
            //obj.AddComponent<GuiComponent>().Initialize(new UI.Icon(obj.GetSprite()));
            obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", mat));
            return obj;
        }
    }
}
