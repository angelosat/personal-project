using System;
using System.Collections.Generic;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    public class ConstructionCategoryWalls : ConstructionCategory
    {
        public override string Name => "Blocks";

        public override IconButton GetButton()
        {
            IconButton btn_Construct = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => this.Window.ToggleSmart(),
                HoverFunc = () => "Construct [" + GlobalVars.KeyBindings.Build + "]"
            };
            return btn_Construct;
        }
      
        public override ToolDrawing GetTool(Func<ProductMaterialPair> itemGetter)
        {
            return new ToolDrawingEnclosure(a => CallBack(itemGetter, a));
        }
        public override List<ToolDrawing> GetAvailableTools(Func<ProductMaterialPair> itemGetter)
        {
            var item = itemGetter();

            return new List<ToolDrawing>()
            {
                new ToolDrawingSingle(a => CallBack(itemGetter, a)),
                new ToolDrawingLine(a => CallBack(itemGetter, a)),
                new ToolDrawingWall(a => CallBack(itemGetter, a)),
                new ToolDrawingEnclosure(a => CallBack(itemGetter, a)),
                new ToolDrawingBox(a => CallBack(itemGetter, a)),
                new ToolDrawingBoxFilled(a => CallBack(itemGetter, a)),
                new ToolDrawingPyramid(a => CallBack(itemGetter, a)),
                new ToolDrawingRoof(a => CallBack(itemGetter, a))
            };
        }
    }
}
