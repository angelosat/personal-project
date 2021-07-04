using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Towns.Constructions
{
    public class UIToolsBoxCheckBoxes : GroupBox
    {
        PanelTitled PanelButtons;
        public List<ToolDrawing> Tools;
        public ToolDrawing CurrentTool;
        ConstructionCategory CurrentCategory;
        public UIToolsBoxCheckBoxes(BlockRecipe.ProductMaterialPair product, Action<Type> onToolSelected)
        {

            this.Name = "Brushes";
            this.PanelButtons = new PanelTitled("Brushes")
            {
                AutoSize = true//,
            };
            this.PanelButtons.Client.BackgroundStyle = BackgroundStyle.TickBox;
            this.AddControls(
                this.PanelButtons);
        }

        public void SetProduct(BlockRecipe.ProductMaterialPair product, Action<Type> onToolSelected)
        {
            if (product != null)
            {
                if (product.Recipe.Category != this.CurrentCategory)
                    this.Refresh(product.Recipe.Category.GetAvailableTools(() => product), onToolSelected);// product));
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
            this.PanelButtons.Client.ClearControls();
            foreach (var tool in tools)
            {
                var radio = new RadioButton(tool.Name);
                radio.LeftClickAction = () =>
                {
                    this.CurrentTool = tool;
                    onToolSelected(tool.GetType());
                };
                this.PanelButtons.Client.AddControls(radio);
            }
            (this.PanelButtons.Client.Controls.First() as RadioButton).PerformLeftClick();
            this.PanelButtons.Client.AlignLeftToRight();
            this.PanelButtons.AlignTopToBottom();
            this.Controls.Remove(this.PanelButtons);
            this.AddControls(this.PanelButtons);
        }
    }

    
}
