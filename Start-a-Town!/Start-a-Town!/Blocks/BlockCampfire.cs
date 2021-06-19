using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Particles;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Crafting;

namespace Start_a_Town_.Blocks
{
    partial class BlockCampfire : BlockWithEntity//Workstation
    {
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.LightWood;
        }
        //public override byte Luminance => 15;
        //public override bool IsDeconstructable => true;
        //public override AILabor Labor => AILabor.Cook;
        //public override bool IsDeconstructible => true;
        public BlockCampfire()
            : base(Block.Types.Campfire, opaque: false, solid: false)
        {
            //this.Ingredient = new Ingredient(item: RawMaterialDef.Logs);
            this.BuildProperties = new BuildProperties(new Ingredient(item: RawMaterialDef.Logs), 0);
            //this.MaterialType = MaterialType.Wood;
            //this.Material = Material.LightWood;
            this.Variations.Add(Block.Atlas.Load("blocks/campfire", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap));

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfSubType(ItemSubType.Logs)
                        )),
                    new BlockRecipe.Product(this))
            { WorkAmount = 2 };// 20 };
            Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);
        }
        public override LootTable GetLootTable(byte data)
        {
            var table =
                new LootTable(
                    //new Loot(() => MaterialType.RawMaterial.Logs.CreateFrom(this.GetMaterial(data)))
                    new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Logs, this.GetMaterial(data)))
                    );
            return table;
        }
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockCampfireEntity();
        }

        public override void Place(IMap map, Vector3 global, byte data, int variation, int orientation, bool notify = true)
        {
            if (!map.GetBlock(global - Vector3.UnitZ).Opaque)
                return;
            base.Place(map, global, data, variation, orientation, notify);
        }
        public override bool IsSwitchable => true;
        public override bool IsRoomBorder => false;

        internal override IEnumerable<Vector3> GetOperatingPositions(Cell cell)
        {
            yield return new Vector3(-1, 0, 0);
            yield return new Vector3(1, 0, 0);
            yield return new Vector3(0, -1, 0);
            yield return new Vector3(0, 1, 0);
        }

        protected override void OnBlockBelowChanged(IMap map, Vector3 global)
        {
            map.GetBlock(global.Below(), out var cell);
            if (cell.Block == BlockDefOf.Air)
                this.Remove(map, global);
        }
    }
}
