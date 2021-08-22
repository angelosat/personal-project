using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    public class UIToolsBox : GroupBox
    {
        Panel PanelButtons;
        public BuildToolDef LastSelectedTool;
        ConstructionCategoryDef CurrentCategory;
        Action<BuildToolDef> OnToolSelectedCallback;

        public UIToolsBox(Action<BuildToolDef> onToolSelected)
        {
            this.OnToolSelectedCallback = onToolSelected;
            this.Name = "Brushes";
            this.PanelButtons = new Panel()
            {
                AutoSize = true
            };
            this.AddControls(
                this.PanelButtons);
        }
        ProductMaterialPair CurrentProduct;
        ProductMaterialPair GetCurrentProduct() => this.CurrentProduct;
        public void SetProduct(ProductMaterialPair product)
        {
            this.CurrentProduct = product;
            if (product is not null)
            {
                var cat = product.Block.ConstructionCategory;
                if (cat != this.CurrentCategory)
                {
                    //cat.ProductGetter = this.GetCurrentProduct;
                    this.Refresh(cat.Tools);//.GetAvailableTools());
                }
                this.CurrentCategory = cat;
            }
            else
                this.CurrentCategory = null;
        }

        public void Refresh(IEnumerable<BuildToolDef> tools)
        {
            //this.PanelButtons.ClearControls();
            this.ClearControls();
            //var grid = new ButtonGridGenericNew<BuildToolDef>();
            //grid.AddItems(tools, (tool, btn) =>
            //{
            //    btn.LeftClickAction = () =>
            //    {
            //        this.LastSelectedTool = tool;
            //        this.OnToolSelectedCallback(tool);
            //    };
            //    btn.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolBlockBuild buildTool && buildTool.ToolDef == tool;
            //});

            var grid = new GroupBox().AddControlsHorizontally(tools.Select(t =>
            {
                //var btn = IconButton.CreateSmall(t.Icon, () => selectTool(t));
                var btn = ButtonNew.CreateMedium(t.Icon, () => selectTool(t));
                btn.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolBlockBuild buildTool && buildTool.ToolDef == t;
                return btn;
            }));

            this.LastSelectedTool = tools.First();
            //this.PanelButtons.AddControls(grid);
            //this.Controls.Remove(this.PanelButtons);
            //this.AddControls(this.PanelButtons);
            this.AddControls(grid);

            void selectTool(BuildToolDef t)
            {
                this.LastSelectedTool = t;
                this.OnToolSelectedCallback(t);
            }
        }
    }

    //public class UIToolsBox : GroupBox
    //{
    //    Panel PanelButtons;
    //    public BuildToolDef LastSelectedTool;
    //    ConstructionCategoryDef CurrentCategory;
    //    Action<BuildToolDef> OnToolSelectedCallback;

    //    public UIToolsBox(Action<BuildToolDef> onToolSelected)
    //    {
    //        this.OnToolSelectedCallback = onToolSelected;
    //        this.Name = "Brushes";
    //        this.PanelButtons = new Panel()
    //        {
    //            AutoSize = true
    //        };
    //        this.AddControls(
    //            this.PanelButtons);
    //    }
    //    ProductMaterialPair CurrentProduct;
    //    ProductMaterialPair GetCurrentProduct() => this.CurrentProduct;
    //    public void SetProduct(ProductMaterialPair product)
    //    {
    //        this.CurrentProduct = product;
    //        if (product is not null)
    //        {
    //            var cat = product.Block.ConstructionCategory;
    //            if (cat != this.CurrentCategory)
    //            {
    //                //cat.ProductGetter = this.GetCurrentProduct;
    //                this.Refresh(cat.Tools);//.GetAvailableTools());
    //            }
    //            this.CurrentCategory = cat;
    //        }
    //        else
    //            this.CurrentCategory = null;
    //    }

    //    public void Refresh(IEnumerable<BuildToolDef> tools)
    //    {
    //        this.PanelButtons.ClearControls();
    //        var grid = new ButtonGridGenericNew<BuildToolDef>();
    //        grid.AddItems(tools, (tool, btn) =>
    //        {
    //            btn.LeftClickAction = () =>
    //            {
    //                this.LastSelectedTool = tool;
    //                this.OnToolSelectedCallback(tool);
    //            };
    //            btn.IsToggledFunc = () => ToolManager.Instance.ActiveTool is ToolBlockBuild buildTool && buildTool.ToolDef == tool;
    //        });

    //        this.LastSelectedTool = tools.First();
    //        this.PanelButtons.AddControls(grid);
    //        this.Controls.Remove(this.PanelButtons);
    //        this.AddControls(this.PanelButtons);
    //    }
    //}
}
