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
    public class ConstructionCategoryProduction : ConstructionCategory
    {
        public override string Name
        {
            get { return "Workbenches"; }
        }
        //public ToolDrawing Tool;
        //public HashSet<BlockConstruction> List = new HashSet<BlockConstruction>();
        //public ConstructionCategoryProduction()
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
        public override IconButton GetButton()
        {
            IconButton btn_ConstructProduction = new IconButton()
            {
                //Location = Btn_Structures.TopRight,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                //LeftClickAction = () => Modules.Construction.UI.ConstructionsWindowNew.Instance.Refresh(this),
                LeftClickAction = () => this.Window.ToggleSmart(),// new Modules.Construction.UI.ConstructionsWindowNewNew(this).Show(),
                HoverFunc = () => "Production"
            };
            return btn_ConstructProduction;
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

        internal void ShowQuickButtons()
        {
            foreach(var bl in this.List)
            {
                var slot = new IconButton();
                var block = bl.BlockProduct.Block;
                slot.Tag = block;
                //slot.Togglable = true;
                //var variants = bl.BlockProduct.Block.GetCraftingVariations();

                slot.IsToggledFunc = () => { var drawing = ToolManager.Instance.ActiveTool as ToolDrawing; return drawing != null ? drawing.Block == block : false; };
                //slot.IsToggledFunc = () => this.CurrentSelected != null && slot.Tag == this.CurrentSelected.BlockProduct.Block;
                slot.PaintAction = () =>
                {
                    bl.BlockProduct.Block.PaintIcon(slot.Width, slot.Height, block.Recipe.GetVariants().First().Data);
                };
                slot.LeftClickAction = () =>
                {
                    //this.CurrentSelected = bl;
                    BlockRecipe.ProductMaterialPair product = block.Recipe.GetVariants().First();// GetLastSelectedVariantOrDefault(bl);

                    //this.ToolBox.SetProduct(product, OnToolSelected);
                    //var tools = product.Recipe.Category.GetAvailableTools(() => GetLastSelectedVariantOrDefault(this.CurrentSelected));
                    //OnToolSelected(this.ToolBox.LastSelectedTool.GetType());
                    //var win = this.ToolBox.GetWindow();
                    //if (win == null)
                    //{
                    //    win = this.ToolBox.ToWindow("Brushes");
                    //    win.HideAction = () => ToolManager.SetTool(null);
                    //}
                    //if (win.Show())
                    //    win.Location = this.GetWindow().BottomLeft;
                };
                //slot.RightClickAction = () =>
                //{
                //    UIBlockVariationPicker.Refresh(bl.Block, this.OnVariationSelected);
                //};
                //                slot.HoverFunc = () => string.Format(
                //@"{0}

                //Right click to select variation", GetLastSelectedVariantOrDefault(bl.Block).GetName());
                QuickButtonBar.AddItem(slot);
            }
        }
    }
}
