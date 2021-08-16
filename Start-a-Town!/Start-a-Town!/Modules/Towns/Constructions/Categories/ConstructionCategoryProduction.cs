using System;
using System.Collections.Generic;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_
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
        
        public override ToolBlockBuild GetTool(Func<ProductMaterialPair> itemGetter)
        {
            return new ToolBuildSinglePreview(a => CallBack(itemGetter, a), () => itemGetter().Block);
        }
        public override List<ToolBlockBuild> GetAvailableTools(Func<ProductMaterialPair> itemGetter)
        {
            return new List<ToolBlockBuild>() { this.GetTool(itemGetter) };
        }

    }
}
