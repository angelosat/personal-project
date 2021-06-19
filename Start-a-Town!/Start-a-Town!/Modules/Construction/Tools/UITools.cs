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

namespace Start_a_Town_.Towns.Constructions
{
    public class UITools : Window
    {
        PanelTitled PanelButtons;
        //Panel PanelButtons;
        public List<ToolDrawing> Tools;

        public UITools()
        {
            this.AutoSize = true;
            this.Client.AutoSize = true;
            this.Name = "Brushes";
            this.Movable = true;
            //BackgroundStyle = BackgroundStyle.TickBox;
            //this.PanelButtons = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.TickBox };
            this.PanelButtons = new PanelTitled("Brushes") { AutoSize = true };
            this.PanelButtons.Client.AutoSize = true;
            this.PanelButtons.Client.BackgroundStyle = BackgroundStyle.TickBox;

            this.Client.AddControls(this.PanelButtons);
        }
        public void Refresh(List<ToolDrawing> tools, Func<BlockRecipe.ProductMaterialPair> itemGetter)
        {
            this.Tools = tools;
            var item = itemGetter();
            this.Title = item.Block.ToString();
            this.PanelButtons.Client.ClearControls();
            //this.PanelButtons.ClearControls();
            foreach (var tool in tools)
            {
                var radio = new RadioButton(tool.Name);// { Location = this.PanelButtons.Panel.TopRight };
                radio.LeftClickAction = () => ToolManager.SetTool(tool);
                this.PanelButtons.Client.AddControls(radio);
                //this.PanelButtons.AddControls(radio);
            }
            (this.PanelButtons.Client.Controls.First() as RadioButton).PerformLeftClick();
            this.PanelButtons.Client.AlignLeftToRight();
            this.PanelButtons.AlignTopToBottom();
            //(this.PanelButtons.Controls.First() as RadioButton).PerformLeftClick();
            //this.PanelButtons.AlignLeftToRight();
            this.Client.ClearControls();
            this.Client.AddControls(this.PanelButtons);
            //this.Controls.Remove(this.Client);
            //this.Controls.Add(this.Client);
        }
        public override bool Hide()
        {
            ToolManager.SetTool(null);
            return base.Hide();
        }
        //private void SelectBrush(ToolDrawing tool)
        //{
        //    ToolManager.SetTool(tool);
        //}
    }
}
