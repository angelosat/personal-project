using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Blocks
{
    partial class BlockCampfire : BlockWithEntity
    {
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.LightWood;
        }
        public BlockCampfire()
            : base(Block.Types.Campfire, opaque: false, solid: false)
        {
            this.BuildProperties = new BuildProperties(new Ingredient(item: RawMaterialDef.Logs), 0);
            this.Variations.Add(Block.Atlas.Load("blocks/campfire", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap));

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(),
                    new BlockRecipe.Product(this))
            { WorkAmount = 2 };
            Towns.Constructions.ConstructionsManager.Production.Add(this.Recipe);
        }
        public override LootTable GetLootTable(byte data)
        {
            var table =
                new LootTable(
                    new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Logs, this.GetMaterial(data)))
                    );
            return table;
        }
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockCampfireEntity();
        }

        public override void Place(MapBase map, IntVec3 global, byte data, int variation, int orientation, bool notify = true)
        {
            if (!map.GetBlock(global - IntVec3.UnitZ).Opaque)
                return;
            base.Place(map, global, data, variation, orientation, notify);
        }
        public override bool IsRoomBorder => false;

        internal override IEnumerable<IntVec3> GetOperatingPositions(Cell cell)
        {
            yield return new IntVec3(-1, 0, 0);
            yield return new IntVec3(1, 0, 0);
            yield return new IntVec3(0, -1, 0);
            yield return new IntVec3(0, 1, 0);
        }

        protected override void OnBlockBelowChanged(MapBase map, IntVec3 global)
        {
            map.GetBlock(global.Below, out var cell);
            if (cell.Block == BlockDefOf.Air)
                this.Remove(map, global);
        }
    }
}
