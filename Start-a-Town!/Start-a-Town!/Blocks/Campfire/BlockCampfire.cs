using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace Start_a_Town_.Blocks
{
    partial class BlockCampfire : BlockWithEntity
    {
        //public override MaterialDef GetMaterial(byte blockdata)
        //{
        //    return MaterialDefOf.LightWood;
        //}
        public BlockCampfire()
            : base("Campfire", opaque: false, solid: false)
        {
            this.BuildProperties = new BuildProperties(new Ingredient(item: RawMaterialDef.Logs), 0);
            this.Variations.Add(Block.Atlas.Load("blocks/campfire", Block.HalfBlockDepthMap, Block.HalfBlockNormalMap));
            this.BuildProperties.WorkAmount = 2;
            this.ToggleConstructionCategory(ConstructionsManager.Production, true);
            this.Ingredient = new Ingredient().SetAllow(RawMaterialDef.Logs, true);
        }
        public override LootTable GetLootTable(byte data)
        {
            var table =
                new LootTable(
                    new Loot(() => ItemFactory.CreateFrom(RawMaterialDef.Logs, MaterialDefOf.Human))// this.GetMaterial(data)))
                    );
            return table;
        }
        public override BlockEntity CreateBlockEntity(IntVec3 originGlobal)
        {
            return new BlockCampfireEntity(originGlobal);
        }

        public override void Place(MapBase map, IntVec3 global, MaterialDef material, byte data, int variation, int orientation, bool notify = true)
        {
            if (!map.GetBlock(global - IntVec3.UnitZ).Opaque)
                return;
            base.Place(map, global, material, data, variation, orientation, notify);
        }
        public override bool IsRoomBorder => false;
        public override bool IsDeconstructible => true;
        protected override void OnDeconstruct(GameObject actor, Vector3 global)
        {
        }
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
                map.RemoveBlock(global);
        }
    }
}
