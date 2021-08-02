using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Blocks
{
    partial class BlockDesignation : Block
    {
        public BlockDesignation()
            : base(Types.Designation, 1, 0, false, false)
        {
            this.Variations.Add(Atlas.Load("blocks/blockblueprint"));
        }
        public override bool IsStandableIn => true;
        public override MaterialDef GetMaterial(byte blockdata)
        {
            return null;
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            var token = this.Variations[0];
            var color = Color.White;
            return canvas.Designations.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, color, sunlight, blocklight, depth, this, blockCoordinates);
        }
       
        public override BlockEntity CreateBlockEntity(IntVec3 originGlobal)
        {
            return new BlockDesignationEntity(originGlobal);
        }
        
        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            var des = actor.Map.GetBlockEntity(target.Global) as BlockDesignationEntity;
            var product = des.Product;
            var global = target.Global;
            amount = amount == -1 ? dropped.StackSize : amount;
            BlockConstructionEntity constr = new(product, global, dropped, amount);
            constr.Children = des.Children;
            var map = actor.Map;
            var positions = des.CellsOccupied;
            bool requiresConstruction = product.Block.RequiresConstruction;
            var isReady = constr.IsReadyToBuild(out _, out _, out _);

            // TODO instead of doing this here, use a function in BlockConstructionEntity
            if (requiresConstruction)
            {
                foreach (var p in positions)
                {
                    map.AddBlockEntity(p, constr);
                    var pcell = map.GetCell(p);
                    map.SetBlock(p, BlockDefOf.Construction.Type, pcell.BlockData, pcell.Variation, pcell.Orientation, false);
                }
            }
            else if(isReady)
            {
                foreach (var p in positions)
                    map.RemoveBlock(p, false);
                var block = product.Block;
                var cell = map.GetCell(global);
                block.Place(map, global, product.Data, 0, cell.Orientation, true);
                map.GetBlockEntity(global)?.IsMadeFrom(new ItemDefMaterialAmount[] { product.Requirement });
            }
           
            if (amount == -1)
                throw new Exception();
            dropped.StackSize -= amount;
        }
        
        internal override bool IsValidHaulDestination(MapBase map, IntVec3 global, GameObject obj)
        {
            var entity = map.GetBlockEntity(global) as BlockDesignationEntity;
            return entity.IsValidHaulDestination(obj.Def);
        }
       
        internal override string GetName(MapBase map, IntVec3 global)
        {
            var e = map.GetBlockEntity<BlockDesignationEntity>(global);
            return string.Format("{0} (Designation)", e.Product.Block.Name);
        }
    }
}
