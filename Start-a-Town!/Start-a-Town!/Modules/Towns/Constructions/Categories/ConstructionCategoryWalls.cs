using System;
using System.Collections.Generic;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ConstructionCategoryWalls : ConstructionCategory
    {
        public override string Name => "Blocks";
        readonly List<ToolBlockBuild> Tools;
        //readonly List<ToolBlockBuildNew> ToolsNew;

        public ConstructionCategoryWalls()
        {
            void callback(ToolBlockBuild.Args a) => CallBack(this.ProductGetter, a);
            //Tools = new()
            //{
            //    new ToolBuildLine(callback),
            //    new ToolBuildWall(callback),
            //    new ToolBuildFloor(callback),
            //    new ToolBuildEnclosure(callback),
            //    new ToolBuildBox(callback),
            //    new ToolBuildBoxFilled(callback),
            //    new ToolBuildPyramid(callback),
            //    new ToolBuildRoof(callback)
            //};
            Tools = new()
            {
                BuildToolDefOf.Single.Create(callback),
                BuildToolDefOf.Line.Create(callback),
                BuildToolDefOf.Wall.Create(callback),
                BuildToolDefOf.Enclosure.Create(callback),
                BuildToolDefOf.Box.Create(callback),
                BuildToolDefOf.BoxFilled.Create(callback),
                BuildToolDefOf.Pyramid.Create(callback),
                BuildToolDefOf.Roof.Create(callback)
            };
        }
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
      
        public override ToolBlockBuild GetTool(Func<ProductMaterialPair> itemGetter)
        {
            return new ToolBuildEnclosure(a => CallBack(itemGetter, a));
        }
        Func<ProductMaterialPair> ProductGetter;
        public override List<ToolBlockBuild> GetAvailableTools(Func<ProductMaterialPair> itemGetter)
        {
            this.ProductGetter = itemGetter;
            return this.Tools;
        }
    }
}
