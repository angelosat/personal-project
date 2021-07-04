using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Constructions
{
    public class ConstructionCategoryProduction : ConstructionCategory
    {
        public override string Name => "Workbenches";
        
        public override IconButton GetButton()
        {
            IconButton btn_ConstructProduction = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => this.Window.ToggleSmart(),
                HoverFunc = () => "Production"
            };
            return btn_ConstructProduction;
        }
        
        public override ToolDrawing GetTool(Func<BlockRecipe.ProductMaterialPair> itemGetter)
        {
            return new ToolDrawingSinglePreview(a => CallBack(itemGetter, a), () => itemGetter().Block);
        }
        public override List<ToolDrawing> GetAvailableTools(Func<BlockRecipe.ProductMaterialPair> itemGetter)
        {
            return new List<ToolDrawing>() { this.GetTool(itemGetter) };
        }

        internal void ShowQuickButtons()
        {
            foreach(var bl in this.List)
            {
                var slot = new IconButton();
                var block = bl.BlockProduct.Block;
                slot.Tag = block;

                slot.IsToggledFunc = () => { var drawing = ToolManager.Instance.ActiveTool as ToolDrawing; return drawing != null ? drawing.Block == block : false; };
                slot.PaintAction = () =>
                {
                    bl.BlockProduct.Block.PaintIcon(slot.Width, slot.Height, block.Recipe.GetVariants().First().Data);
                };
                slot.LeftClickAction = () =>
                {
                    BlockRecipe.ProductMaterialPair product = block.Recipe.GetVariants().First();
                };
                QuickButtonBar.AddItem(slot);
            }
        }
    }
}
