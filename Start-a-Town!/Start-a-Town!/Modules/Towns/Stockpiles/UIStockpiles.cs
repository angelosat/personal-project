using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns.Stockpiles
{
    class UIStockpiles : Window
    {
        IconButton BtnDesignate;

        public UIStockpiles()
        //:base("Stockpiles")
        {
            this.Title = "Stockpiles";
            this.Movable = true;
            this.AutoSize = true;
            this.BtnDesignate = new IconButton()
            {
                //Location = this.Controls.BottomLeft,
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate stockpile",
                //LeftClickAction = () => ScreenManager.CurrentScreen.ToolManager.ActiveTool = new StockpileTool(Engine.Map.GetTown())// this.Create)
                LeftClickAction = () => ToolManager.SetTool(new ToolDesignateZone(Engine.Map.GetTown(), typeof(Stockpile)))

            };
            this.Client.Controls.Add(this.BtnDesignate);
        }
       
    }
}
