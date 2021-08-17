using System;
using System.Collections.Generic;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_
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
        
        public override IEnumerable<BuildToolDef> GetAvailableTools()
        {
            yield return BuildToolDefOf.SinglePreview;
        }
    }
}
