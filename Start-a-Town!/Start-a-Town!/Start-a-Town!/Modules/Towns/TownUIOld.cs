using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.Towns.Stockpiles;

namespace Start_a_Town_.Towns
{
    public class UITownWindowOld : Window
    {
        public StockpilesManagerUI StockpileUI;
        Town Town;
        IconButton BtnDesignate;
        public UITownWindowOld(Town town)
        {
            this.Title = "Town";
            this.AutoSize = true;
            this.Movable = true;
            this.Town = town;

            Panel panelButtons = new Panel() { Location = this.Client.Controls.BottomLeft, AutoSize = true };
            this.BtnDesignate = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate stockpiles\n\nLeft click & drag: Add stockpile\nCtrl+Left click: Remove stockpile",// "Add/Remove stockpiles",
                LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new StockpileTool(Engine.Map.GetTown())// this.Create)
            };
            panelButtons.Controls.Add(this.BtnDesignate);
            this.Client.Controls.Add(panelButtons);

            PanelLabeled stockpiles = new PanelLabeled("Stockpiles") { Location = this.Client.Controls.BottomLeft, AutoSize = true };
            this.StockpileUI = new StockpilesManagerUI(town) { Location = stockpiles.Controls.BottomLeft };
            stockpiles.Controls.Add(StockpileUI);
            this.Client.Controls.Add(stockpiles);
        }

        public override bool Toggle()
        {
            this.Location = this.CenterScreen * 0.5f;
            StockpileUI.Refresh();
            return base.Toggle();
        }

        public override void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            this.Town.DrawWorld(sb, camera.Map, camera);
        }
    }

    class TownUIOld : GroupBox
    {
        StockpilesManagerUI StockpileUI;
        Town Town;
        public TownUIOld(Town town)
        {
            this.Town = town;
            this.StockpileUI = new StockpilesManagerUI(this.Town);
            this.Controls.Add(this.StockpileUI);
        }
    }

}
