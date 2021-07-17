using System;
using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Start_a_Town_.Modules.Construction;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Towns.Constructions
{
    public class UITools : Window
    {
        PanelTitled PanelButtons;
        public List<ToolDrawing> Tools;

        public UITools()
        {
            this.AutoSize = true;
            this.Client.AutoSize = true;
            this.Name = "Brushes";
            this.Movable = true;
            this.PanelButtons = new PanelTitled("Brushes") { AutoSize = true };
            this.PanelButtons.Client.AutoSize = true;
            this.PanelButtons.Client.BackgroundStyle = BackgroundStyle.TickBox;
            this.Client.AddControls(this.PanelButtons);
        }
        public void Refresh(List<ToolDrawing> tools, Func<ProductMaterialPair> itemGetter)
        {
            this.Tools = tools;
            var item = itemGetter();
            this.Title = item.Block.ToString();
            this.PanelButtons.Client.ClearControls();
            foreach (var tool in tools)
            {
                var radio = new RadioButton(tool.Name);
                radio.LeftClickAction = () => ToolManager.SetTool(tool);
                this.PanelButtons.Client.AddControls(radio);
            }
            (this.PanelButtons.Client.Controls.First() as RadioButton).PerformLeftClick();
            this.PanelButtons.Client.AlignLeftToRight();
            this.PanelButtons.AlignTopToBottom();
            this.Client.ClearControls();
            this.Client.AddControls(this.PanelButtons);
        }
        public override bool Hide()
        {
            ToolManager.SetTool(null);
            return base.Hide();
        }
    }
}
