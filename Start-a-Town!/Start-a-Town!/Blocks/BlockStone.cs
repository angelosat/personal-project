using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.GameModes;
using Start_a_Town_.Towns.Constructions;

namespace Start_a_Town_
{
    class BlockStone : Block
    {
        public override bool IsMinable => true;

        public BlockStone()
            : base(Block.Types.Cobblestone, GameObject.Types.CobblestoneItem, 0, 1, true, true)
        {
            //this.Material = Material.Stone;
            //this.MaterialType = MaterialType.Mineral;
            this.LootTable = new LootTable(new Loot(GameObject.Types.Stone, 0.75f, 4));
            //this.Reagents.Add(Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks));//   GameObject.Types.Stone);
            this.Reagents.Add(new Reaction.Reagent("Base", Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks), Reaction.Reagent.IsOfMaterial(MaterialDefOf.Stone)));
            this.AssetNames = "stone5height19";// "stone1, stone2, stone3, stone4";//sand1";//stone1, stone2, stone3, stone4";

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterial(MaterialDefOf.Stone), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockRecipe.Product(this)
                );
            this.Ingredient = new Ingredient(RawMaterialDef.Boulders, MaterialDefOf.Stone, null, 1);
            ConstructionsManager.Walls.Add(this.Recipe);
        }
        public override Particles.ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }


        public override List<Interaction> GetAvailableTasks(IMap map, Vector3 global)
        {
            return new List<Interaction>(){
                new InteractionMining()
            };
        }
        public override ContextAction GetRightClickAction(Vector3 global)
        {
            return new ContextAction(() => "Mine", () => { Net.Client.PlayerInteract(new TargetArgs(global)); });
        }
        public override Material GetMaterial(byte data)
        {
            return MaterialDefOf.Stone;
        }

        
    }
}
