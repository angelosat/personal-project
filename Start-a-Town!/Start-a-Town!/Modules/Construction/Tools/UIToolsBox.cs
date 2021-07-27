using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Towns.Constructions
{
    public class UIToolsBox : GroupBox
    {
        Panel PanelButtons;
        public List<ToolDrawing> Tools;
        public ToolDrawing LastSelectedTool;
        ConstructionCategory CurrentCategory;
        public UIToolsBox(ProductMaterialPair product, Action<Type> onToolSelected)
        {

            this.Name = "Brushes";
            this.PanelButtons = new Panel()
            {
                AutoSize = true
            };
            this.AddControls(
                this.PanelButtons);
        }

        public void SetProduct(ProductMaterialPair product, Action<Type> onToolSelected)
        {
            if (product is not null)
            {
                var cat = product.Block.ConstructionCategory;
                if (cat != this.CurrentCategory)
                    this.Refresh(cat.GetAvailableTools(() => product), onToolSelected);
                this.CurrentCategory = cat;
            }
            else
                this.CurrentCategory = null;
        }
        public void Refresh(List<ToolDrawing> tools, Action<Type> onToolSelected)
        {
            this.Tools = tools;
            this.PanelButtons.ClearControls();
            var grid = new ButtonGridGenericNew<ToolDrawing>();                                   
            grid.AddItems(tools, (tool, btn) =>
                {
                    btn.LeftClickAction = () =>
                    {
                        this.LastSelectedTool = tool;
                        onToolSelected(tool.GetType());
                    };
                    btn.IsToggledFunc = () => ToolManager.Instance.ActiveTool != null && ToolManager.Instance.ActiveTool.GetType() == tool.GetType();
                });

            this.LastSelectedTool = tools.First();
            this.PanelButtons.AddControls(grid);
            this.Controls.Remove(this.PanelButtons);
            this.AddControls(this.PanelButtons);
        }
    }
}
