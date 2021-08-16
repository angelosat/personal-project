﻿using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    partial class BlockDesignation : Block
    {
        public BlockDesignation()
            : base("Designation", 1, 0, false, false)
        {
            this.Variations.Add(Atlas.Load("blocks/blockblueprint"));
            this.DrawMaterialColor = false;
        }

        public override bool IsStandableIn => true;
       
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data, MaterialDef mat)
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
                    map.SetBlock(p, BlockDefOf.Construction, product.Material, pcell.BlockData, pcell.Variation, pcell.Orientation, false);
                }
                map.NotifyBlocksChanged(positions);
            }
            else if(isReady)
            {
                foreach (var p in positions)
                    map.RemoveBlock(p, false);
                var block = product.Block;
                var cell = map.GetCell(global);
                block.Place(map, global, product.Material, product.Data, 0, cell.Orientation, true);
                map.GetBlockEntity(global)?.IsMadeFrom(new ItemMaterialAmount[] { product.Requirement });
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
            return $"{e.Product.Block.Name} (Designation)";
        }
    }
}
