using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.Construction.UI;

namespace Start_a_Town_.Towns.Constructions
{
    public class ConstructionCategoryWalls : ConstructionCategory
    {
        public override string Name
        {
            get { return "Blocks"; }
        }

        //TerrainWindowNew _Window;
        //TerrainWindowNew Window
        //{
        //    get
        //    {
        //        if (this._Window == null)
        //            this._Window = new TerrainWindowNew(this);// new ConstructionsWindowNewNew(this);
        //        return this._Window;
        //    }
        //}
        public override IconButton GetButton()
        {
            IconButton btn_Construct = new IconButton()
            {
                //Location = Btn_Structures.TopRight,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                LeftClickAction = () => this.Window.ToggleSmart(),// new Modules.Construction.UI.ConstructionsWindowNewNew(this).Show(),
                //LeftClickAction = () => Modules.Construction.UI.ConstructionsWindowNew.Instance.Refresh(this),
                HoverFunc = () => "Construct [" + GlobalVars.KeyBindings.Build + "]"
            };
            return btn_Construct;
        }
        //public override ToolDrawing GetTool(Action<ToolDrawingSinglePreview.Args> callback, Block block)
        //{
        //    return new ToolDrawingSinglePreview(callback, block);
        //}
        //public override ToolDrawing GetTool(BlockConstruction.ProductMaterialPair item)
        //{
        //    return new ToolDrawingEnclosure(a => CallBack(item, a));
        //}
        public override ToolDrawing GetTool(Func<BlockRecipe.ProductMaterialPair> itemGetter)
        {
            return new ToolDrawingEnclosure(a => CallBack(itemGetter, a));

            //var item = itemGetter();
            //return new ToolDrawingEnclosure(a => CallBack(item, a));
        }
        public override List<ToolDrawing> GetAvailableTools(Func<BlockRecipe.ProductMaterialPair> itemGetter)
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

                //new ToolDrawingSingle(a => CallBack(item, a)),
                //new ToolDrawingLine(a => CallBack(item, a)),
                //new ToolDrawingWall(a => CallBack(item, a)),
                //new ToolDrawingEnclosure(a => CallBack(item, a)),
                //new ToolDrawingBox(a => CallBack(item, a)),
            };
        }
    }
}
