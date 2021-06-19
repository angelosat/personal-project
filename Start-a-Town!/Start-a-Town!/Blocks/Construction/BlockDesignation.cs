using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Graphics;
using Start_a_Town_.Blocks;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;
using Start_a_Town_.AI;
using Start_a_Town_.Interactions;

namespace Start_a_Town_.Blocks
{
    partial class BlockDesignation : Block
    {
        public BlockDesignation()
            : base(Types.Designation, 1, 0, false, false)
        {
            //this.AssetNames = "highlightfull";
            this.Variations.Add(Block.Atlas.Load("blocks/blockblueprint"));
        }
        public override bool IsStandableIn => true;
        public override Material GetMaterial(byte blockdata)
        {
            return null;// Material.LightWood;
        }
        public override MyVertex[] Draw(Canvas canvas, Chunk chunk, Vector3 blockCoordinates, Camera camera, Vector4 screenBounds, Color sunlight, Vector4 blocklight, Color fog, Color tint, float depth, int variation, int orientation, byte data)
        {
            var token = this.Variations[0];
            var color = Color.White;
            return canvas.Designations.DrawBlock(Block.Atlas.Texture, screenBounds, token, camera.Zoom, fog, color, sunlight, blocklight, depth, this, blockCoordinates);
        }
       
        public override BlockEntity CreateBlockEntity()
        {
            return new BlockDesignationEntity();
        }
        
        
        public override void OnDrop(GameObject actor, GameObject dropped, TargetArgs target, int amount = -1)
        {
            var des = actor.Map.GetBlockEntity(target.Global) as BlockDesignationEntity;
            var product = des.Product;
            var global = target.Global;
            amount = amount == -1 ? dropped.StackSize : amount;
            BlockConstructionEntity constr = new(product, global, dropped, amount);
            constr.Children = des.Children;
            bool ismulti = product.Block.Multi;
            var map = actor.Map;
            //var positions = ismulti ? new List<Vector3> { global } : des.Children;
            var positions = des.CellsOccupied;
            bool requiresConstruction = product.Block.RequiresConstruction;
            var isReady = constr.IsReadyToBuild(out _, out _, out _);

            // TODO instead of doing this here, use a function in BlockConstructionEntity
            if (requiresConstruction)
            {
                //map.AddBlockEntity(global, constr);
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
        
        
        public override void Removed(IMap map, Vector3 global)
        {
            this.Remove(map, global);
        }
       
      
        
        public bool MaterialsPresent(IMap map, Vector3 global)
        {
            var entity = map.GetBlockEntity(global) as BlockDesignationEntity;
            return entity.MaterialsPresent();
            //foreach (var mat in entity.Materials)
            //    if (mat.Remaining > 0)
            //        return false;
            //return true;
        }

        internal override bool IsValidHaulDestination(IMap map, Vector3 global, GameObject obj)
        {
            var entity = map.GetBlockEntity(global) as BlockDesignationEntity;
            return entity.IsValidHaulDestination(obj.Def);
        }

       
        static BlockDesignationEntity GetEntity(IMap map, Vector3 global)
        {
            return map.GetBlockEntity(global) as BlockDesignationEntity;
        }
        internal override string GetName(IMap map, Vector3 global)
        {
            var e = map.GetBlockEntity<BlockDesignationEntity>(global);
            return string.Format("{0} (Designation)", e.Product.Block.Name);
        }
        //internal override void GetSelectionInfo(UISelectedInfo uISelectedInfo, IMap map, Vector3 vector3)
        //{
        //    base.Select(uISelectedInfo, map, vector3);
        //    var e = GetEntity(map, vector3);
        //    uISelectedInfo.AddInfo(new Label()
        //    {
        //        TextFunc = () => string.Format("Build progress: {0}", e.BuildProgress.ToStringPercentage())
        //    });
        //}
        //internal override void GetQuickButtons(UISelectedInfo uISelectedInfo, IMap map, Vector3 vector3)
        //{
        //    base.GetQuickButtons(uISelectedInfo, map, vector3);
        //    //IconCancel.LeftClickAction = () => Cancel(vector3);
        //    //UISelectedInfo.AddButton(IconCancel);
        //    UISelectedInfo.AddButton(Start_a_Town_.Designation.IconCancel, Start_a_Town_.Designation.Cancel, new TargetArgs(vector3));

        //}

    }
}
