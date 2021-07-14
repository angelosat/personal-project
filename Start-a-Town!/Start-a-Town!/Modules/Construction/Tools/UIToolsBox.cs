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
        public UIToolsBox(BlockRecipe.ProductMaterialPair product, Action<Type> onToolSelected)
        {

            this.Name = "Brushes";
            this.PanelButtons = new Panel()
            {
                AutoSize = true
            };
            this.AddControls(
                this.PanelButtons);
        }

        public void SetProduct(BlockRecipe.ProductMaterialPair product, Action<Type> onToolSelected)
        {
            if (product is not null)
            {
                if (product.Recipe.Category != this.CurrentCategory)
                    this.Refresh(product.Recipe.Category.GetAvailableTools(() => product), onToolSelected);
                this.CurrentCategory = product.Recipe.Category;
            }
            else
            {
                this.CurrentCategory = null;
            }
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
        public void RefreshOld(List<ToolDrawing> tools, Action<Type> onToolSelected)
        {
            this.Tools = tools;
            this.PanelButtons.ClearControls();
            foreach (var tool in tools)
            {
                var radio = new RadioButton(tool.Name);
                radio.LeftClickAction = () =>
                {
                    this.LastSelectedTool = tool;
                    onToolSelected(tool.GetType());
                };
                this.PanelButtons.AddControls(radio);
            }
            (this.PanelButtons.Controls.First() as RadioButton).PerformLeftClick();
            this.PanelButtons.AlignLeftToRight();
            this.PanelButtons.AlignTopToBottom();
            this.Controls.Remove(this.PanelButtons);
            this.AddControls(this.PanelButtons);
        }
    }
}
