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
       
        public override IEnumerable<BuildToolDef> GetAvailableTools()
        {
            yield return BuildToolDefOf.SinglePreview;
        }
    }
}
