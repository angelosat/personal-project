using System;
using System.Collections.Generic;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    public class ConstructionCategoryDoors : ConstructionCategory
    {
        public override string Name => "Doors";
        
        public override UI.IconButton GetButton()
        {
            IconButton btn_ConstructDoors = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => this.Window.ToggleSmart(), 

                HoverFunc = () => "Doors"
            };
            return btn_ConstructDoors;
        }
        
        public override ToolDrawing GetTool(Func<ProductMaterialPair> itemGetter)
        {
            return new ToolDrawingSinglePreview(a => CallBack(itemGetter, a), () => itemGetter().Block);
        }
        public override List<ToolDrawing> GetAvailableTools(Func<ProductMaterialPair> itemGetter)
        {
            return new List<ToolDrawing>() { this.GetTool(itemGetter) };
        }
    }
}
