using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System;

namespace Start_a_Town_
{
    [Obsolete]
    class BlockEntityCompDeconstructible : BlockEntityComp
    {
        public ItemMaterialAmount[] Materials = new ItemMaterialAmount[1] { new ItemMaterialAmount(RawMaterialDef.Logs, MaterialDefOf.Human, 2) };
        internal override void IsMadeFrom(ItemMaterialAmount[] itemDefMaterialAmounts)
        {
            this.Materials = itemDefMaterialAmounts;
        }
        internal override void Deconstruct(GameObject actor, IntVec3 global)
        {
            return;
            // DO I NEED TO DO THIS HERE? or can i do this at the block base class?
            var map = actor.Map;
            var cell = map.GetCell(global);
            var block = cell.Block;
            //var material = block.GetMaterial(cell.BlockData);
            var material = cell.Material;
            var scraps = RawMaterialDef.Scraps;
            var materialQuantity = block.Ingredient.Amount;
            var obj = scraps.CreateFrom(material).SetStackSize(materialQuantity);
            actor.Net.PopLoot(obj, global, Vector3.Zero);
        }
        internal override void GetSelectionInfo(IUISelection info, MapBase map, IntVec3 vector3)
        {
            foreach (var mat in this.Materials)
                info.AddInfo(new Label(mat));
        }
    }
}
