using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    class BlockStone : Block
    {

        public BlockStone()
            : base(Block.Types.Cobblestone, GameObject.Types.CobblestoneItem, 0, 1, true, true)
        {
            //this.Material = Material.Stone;
            //this.MaterialType = MaterialType.Mineral;
            this.LootTable = new LootTable(new Loot(GameObject.Types.Stone, 0.75f, 4));
            //this.Reagents.Add(Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks));//   GameObject.Types.Stone);
            this.Reagents.Add(new Reaction.Reagent("Base", Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks), Reaction.Reagent.IsOfMaterial(Material.Stone)));
            this.AssetNames = "stone5height19";// "stone1, stone2, stone3, stone4";//sand1";//stone1, stone2, stone3, stone4";


            this.Recipe = new BlockConstruction(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterial(Material.Stone), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockConstruction.Product(this)
                );
        }
        public override Components.Particles.ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }

        public override void OnMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Mine:
                    this.Break(parent.Map, parent.Global);
                    //e.Network.PopLoot(GameObject.Create(GameObject.Types.WoodenPlank), parent.Global, parent.Velocity);
                    //e.Network.Despawn(parent);
                    //e.Network.DisposeObject(parent);
                    return;

                default:
                    break;
            }
        }

        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                new Mining()
            };
        }
        public override ContextAction GetRightClickAction(Vector3 global)
        {
            return new ContextAction(() => "Mine", () => { Net.Client.PlayerInteract(new TargetArgs(global)); });
        }
        public override Material GetMaterial(byte data)
        {
            return Material.Stone;
        }

        protected override GameObject ToObject()
        {
            return this.Create(Material.Stone);
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
