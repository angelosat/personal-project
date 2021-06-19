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
    class UIHudPanel : PanelLabeled
    {
        IconButton BtnStockpiles;

        UIStockpiles UIStockpiles;

        public UIHudPanel():base("Town")
        {
            UIStockpiles = new UIStockpiles();
            this.AutoSize = true;
            this.BtnStockpiles = new IconButton()
            {
                Location = this.Controls.BottomLeft,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Stockpiles",
                LeftClickAction = () =>// ScreenManager.CurrentScreen.ToolManager.ActiveTool = new StockpileTool(s => { })
                    {
                        UIStockpiles.Location = this.BtnStockpiles.ScreenLocation + this.BtnStockpiles.TopRight;
                        UIStockpiles.Anchor = Vector2.One;
                        UIStockpiles.Toggle();
                    }
            };

            this.Controls.Add(this.BtnStockpiles);
        }
    }
}
