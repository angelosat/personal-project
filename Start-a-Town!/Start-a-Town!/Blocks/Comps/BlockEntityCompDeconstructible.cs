using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class BlockEntityCompDeconstructible : BlockEntityComp
    {
        public ItemDefMaterialAmount[] Materials = new ItemDefMaterialAmount[1] { new ItemDefMaterialAmount(RawMaterialDef.Logs, MaterialDefOf.Human, 2) };
        internal override void IsMadeFrom(ItemDefMaterialAmount[] itemDefMaterialAmounts)
        {
            this.Materials = itemDefMaterialAmounts;
        }
        internal override void Deconstruct(GameObject actor, Vector3 global)
        {
            // DO I NEED TO DO THIS HERE? or can i do this at the block base class?
            var map = actor.Map;
            var cell = map.GetCell(global);
            var block = cell.Block;
            var material = block.GetMaterial(cell.BlockData);
            var scraps = RawMaterialDef.Scraps;
            var materialQuantity = block.Ingredient.Amount;
            var obj = scraps.CreateFrom(material).SetStackSize(materialQuantity);
            actor.Net.PopLoot(obj, global, Vector3.Zero);
            return;
            foreach (var mat in this.Materials)
            {
                actor.Net.PopLoot(mat.Create(), global, Vector3.Zero);
            }
        }
        internal override void GetSelectionInfo(IUISelection info, IMap map, Vector3 vector3)
        {
            foreach (var mat in this.Materials)
                info.AddInfo(new Label(mat));
        }
    }
}
