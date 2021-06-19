using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.Construction.UI;
using Start_a_Town_.Blocks;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Towns.Constructions
{
    public class UIToolsBox : GroupBox
    {
        //PanelTitled PanelButtons;
        Panel PanelButtons;
        public List<ToolDrawing> Tools;
        public ToolDrawing LastSelectedTool;
        ConstructionCategory CurrentCategory;
        public UIToolsBox(BlockRecipe.ProductMaterialPair product, Action<Type> onToolSelected)
        {

            this.Name = "Brushes";
            //this.PanelButtons = new PanelTitled("Brushes")
            this.PanelButtons = new Panel()
            {
                AutoSize = true//,
            };
            //this.PanelButtons.Panel.BackgroundStyle = BackgroundStyle.TickBox;
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
