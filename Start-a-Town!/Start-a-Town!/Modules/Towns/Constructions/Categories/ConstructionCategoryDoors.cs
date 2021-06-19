using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Modules.Construction.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Constructions
{
    public class ConstructionCategoryDoors : ConstructionCategory
    {
        public override string Name
        {
            get { return "Doors"; }
        }
        //public ToolDrawing Tool;
        //public HashSet<BlockConstruction> List = new HashSet<BlockConstruction>();
        //public ConstructionCategoryDoors()
        //{

        //}
        //public void Add(BlockConstruction constr)
        //{
        //    this.List.Add(constr);
        //}
        TerrainWindowNew _Window;
        TerrainWindowNew Window
        {
            get
            {
                if (this._Window == null)
                    this._Window = new TerrainWindowNew(this);
                return this._Window;
            }
        }
        public override UI.IconButton GetButton()
        {
            IconButton btn_ConstructDoors = new IconButton()
            {
                //Location = Btn_Structures.TopRight,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                //LeftClickAction = () => Modules.Construction.UI.ConstructionsWindowNew.Instance.Refresh(this),
                LeftClickAction = () => this.Window.ToggleSmart(), //new Modules.Construction.UI.ConstructionsWindowNewNew(this).Show(),

                HoverFunc = () => "Doors"
            };
            return btn_ConstructDoors;
        }
        //public override ToolDrawing GetTool(Action<ToolDrawingSinglePreview.Args> callback, Block block)
        //{
        //    return new ToolDrawingSinglePreview(callback, block);
        //}
        //public override ToolDrawing GetTool(BlockConstruction.ProductMaterialPair item)
        //{
        //    return new ToolDrawingSinglePreview(a => CallBack(item, a), item.Block);
        //}
        public override ToolDrawing GetTool(Func<BlockRecipe.ProductMaterialPair> itemGetter)
        {
            return new ToolDrawingSinglePreview(a => CallBack(itemGetter, a), () => itemGetter().Block);

            //var item = itemGetter();
            //return new ToolDrawingSinglePreview(a => CallBack(item, a), item.Block);
        }
        public override List<ToolDrawing> GetAvailableTools(Func<BlockRecipe.ProductMaterialPair> itemGetter)
        {
            return new List<ToolDrawing>() { this.GetTool(itemGetter) };
        }
    }
}
