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
    public class ConstructionCategoryFurniture : ConstructionCategory
    {
        public override string Name
        {
            get { return "Furniture"; }
        }
        //public ToolDrawing Tool;
        //public HashSet<BlockConstruction> List = new HashSet<BlockConstruction>();
        //public ConstructionCategoryFurniture()
        //{

        //}
        //public void Add(BlockConstruction constr)
        //{
        //    this.List.Add(constr);
        //}
        public override IconButton GetButton()
        {
            IconButton btn_ConstructFurniture = new IconButton()
            {
                //Location = Btn_Structures.TopRight,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                //LeftClickAction = () => Modules.Construction.UI.ConstructionsWindowNew.Instance.Refresh(this),
                LeftClickAction = () => this.Window.ToggleSmart(),// new Modules.Construction.UI.ConstructionsWindowNewNew(this).Show(),
                HoverFunc = () => "Furniture"
            };
            return btn_ConstructFurniture;
        }
        //public override ToolDrawing GetTool(Action<ToolDrawingSinglePreview.Args> callback, Block block)
        //{
        //    return new ToolDrawingSinglePreview(callback, block);
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
        //public ConstructionsWindowNewNew GetWindow()
        //{
        //    var win = new constru
        //}
        
        public override ToolDrawing GetTool(Func<BlockRecipe.ProductMaterialPair> itemGetter)
        {
            return new ToolDrawingSinglePreview(a => CallBack(itemGetter, a), ()=>itemGetter().Block);

            //var item = itemGetter();
            //return new ToolDrawingSinglePreview(a => CallBack(item, a), item.Block);
        }
        public override List<ToolDrawing> GetAvailableTools(Func<BlockRecipe.ProductMaterialPair> itemGetter)
        {
            return new List<ToolDrawing>() { this.GetTool(itemGetter) };
        }
    }
}
