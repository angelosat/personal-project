using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Towns.Stockpiles;

namespace Start_a_Town_.Towns
{
    public class UITownWindow : Window
    {
        //public StockpileUI StockpileUI;
        Town Town;
        //IconButton BtnDesignate;

        Panel PanelClient;

        public UITownWindow(Town town)
        {
            this.Title = "Town";
            //this.AutoSize = true;
            this.Movable = true;
            this.Town = town;
            //this.Width = 400;
            //this.Height = 400;
            this.Size = new Rectangle(0, 0, 400, 400);
            this.Location = Vector2.Zero;
            Panel panelTabs = new Panel() { AutoSize = true };
            var w = 0;
            var lastPoint = Vector2.Zero;
            foreach (var comp in town.TownComponents)
            {
                var radio = new RadioButton(comp.Name);// { Location = panelTabs.Controls.TopRight };
                var ui = comp.GetInterface();
                radio.LeftClickAction =
                    () =>
                    {
                        this.PanelClient.Controls.Clear();
                        if(ui!=null)
                        this.PanelClient.Controls.Add(ui);
                    };
                w += radio.Width;
                if (w >= this.ClientSize.Width)
                {
                    radio.Location = new Vector2(0, panelTabs.Controls.BottomLeft.Y);
                    w = 0;
                }
                else
                {
                    radio.Location = lastPoint;// panelTabs.Controls.Last().TopRight;
                }
                lastPoint = radio.TopRight;
                panelTabs.Controls.Add(radio);

            }
            this.PanelClient = new Panel() { Location = panelTabs.BottomLeft, Size = new Microsoft.Xna.Framework.Rectangle(0, 0, this.ClientSize.Width, this.ClientSize.Height - panelTabs.Height - this.Label_Title.Height) };
            this.Client.Controls.Add(panelTabs, this.PanelClient);
            var rd = panelTabs.Controls.FirstOrDefault() as RadioButton;
            if (rd != null)
                rd.PerformLeftClick();

            //Panel panelButtons = new Panel() { Location = this.Client.Controls.BottomLeft, AutoSize = true };
            //this.BtnDesignate = new IconButton()
            //{
            //    BackgroundTexture = UIManager.DefaultIconButtonSprite,
            //    Icon = new Icon(UIManager.Icons32, 12, 32),
            //    HoverFunc = () => "Designate stockpiles\n\nLeft click & drag: Add stockpile\nCtrl+Left click: Remove stockpile",// "Add/Remove stockpiles",
            //    LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new StockpileTool(Engine.Map.GetTown())// this.Create)
            //};
            //panelButtons.Controls.Add(this.BtnDesignate);
            //this.Client.Controls.Add(panelButtons);

            //PanelLabeled stockpiles = new PanelLabeled("Stockpiles") { Location = this.Client.Controls.BottomLeft, AutoSize = true };
            //this.StockpileUI = new StockpileUI(town) { Location = stockpiles.Controls.BottomLeft };
            //stockpiles.Controls.Add(StockpileUI);
            //this.Client.Controls.Add(stockpiles);
        }

        //public override bool Toggle()
        //{
        //    this.Location = this.CenterScreen * 0.5f;
        //    //StockpileUI.Refresh();
        //    return base.Toggle();
        //}

        public override void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            this.Town.DrawWorld(sb, camera.Map, camera);
        }
    }

    class TownUI : GroupBox
    {
        StockpilesManagerUI StockpileUI;
        Town Town;
        public TownUI(Town town)
        {
            this.Town = town;
            this.StockpileUI = new StockpilesManagerUI(this.Town);
            this.Controls.Add(this.StockpileUI);
        }
    }

}
