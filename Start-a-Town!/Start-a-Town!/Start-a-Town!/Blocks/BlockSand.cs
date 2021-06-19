using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Tokens;

namespace Start_a_Town_.Blocks
{
    class BlockSand : Block
    {
        //new Block(Block.Types.Sand, GameObject.Types.Sand) { Material = Material.Sand, AssetNames = "sand1" };
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Sand;
        }
        public BlockSand()
            : base(Block.Types.Sand, GameObject.Types.Sand)
        {
            //this.Material = Material.Sand;
            this.AssetNames = "sand1";

            this.Recipe = new BlockConstruction(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Sand), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockConstruction.Product(this)
                );

            var reag = Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Sand), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks)));
            var prod = Reaction.Product.Create(new Reaction.Product(mats => BlockEntityPacked.Create(Block.Sand, 0)));
            Reaction crafted = new Reaction(
                "Sand",
                //Reaction.CanBeMadeAt(ItemTemplate.Workbench.ID),
                Reaction.CanBeMadeAt(IsWorkstation.Types.Workbench),
                reag,
                //Reaction.Product.Create(new Reaction.Product(MaterialType.Bars))
                prod
            );

        }
        public override List<byte> GetVariations()
        {
            return new List<byte>() { 0 };
        }
        public override Components.Particles.ParticleEmitterSphere GetEmitter()
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
            return this.Create(Material.Sand);
        }
        GameObject Create(Material mat)
        {
            GameObject obj = new GameObject();
            obj.AddComponent<GeneralComponent>().Initialize(Block.EntityIDRange + (int)this.Type, ObjectType.Block, this.GetName());
            obj.AddComponent<PhysicsComponent>().Initialize(size: 1);
            obj.AddComponent<BlockComponent>().Initialize(this);
            obj.AddComponent<SpriteComponent>().Initialize(new Sprite(this.Variations.First().Name, Map.BlockDepthMap)
            {
                Origin = Block.OriginCenter,
                Joint = Block.Joint,
                MouseMap = BlockMouseMap,
                Overlays = new Dictionary<string, Sprite>() { { "Body", new Sprite(this.Variations.First().Name, Map.BlockDepthMap) { Tint = mat.Color } } }
            });
            obj.AddComponent<GuiComponent>().Initialize(new Icon(obj.GetSprite()));
            obj.AddComponent<MaterialsComponent>().Initialize(new PartMaterialPair("Body", mat));
            return obj;
        }
    }
}
