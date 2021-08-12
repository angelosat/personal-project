using System;
using System.Collections.Generic;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ConstructionCategoryFurniture : ConstructionCategory
    {
        public override string Name => "Furniture";
        
        public override IconButton GetButton()
        {
            IconButton btn_ConstructFurniture = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => this.Window.ToggleSmart(),
                HoverFunc = () => "Furniture"
            };
            return btn_ConstructFurniture;
        }
        
        public override ToolDrawing GetTool(Func<ProductMaterialPair> itemGetter)
        {
            return new ToolDrawingSinglePreview(a => CallBack(itemGetter, a), ()=>itemGetter().Block);
        }
        public override List<ToolDrawing> GetAvailableTools(Func<ProductMaterialPair> itemGetter)
        {
            return new List<ToolDrawing>() { this.GetTool(itemGetter) };
        }
    }
}
