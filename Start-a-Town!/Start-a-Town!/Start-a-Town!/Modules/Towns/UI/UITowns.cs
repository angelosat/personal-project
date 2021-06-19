using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;
using Start_a_Town_.Towns.Stockpiles;

namespace Start_a_Town_.Towns
{
    class TownsUI
    {
        IconButton HudButton;
        UIHudPanel HudPanel;

        TownManagerUI WindowTown;
        public TownsUI()
        {
            this.HudPanel = new UIHudPanel();
            this.HudButton = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Town",
                LeftClickAction = () =>// ScreenManager.CurrentScreen.ToolManager.ActiveTool = new StockpileTool(s => { })
                    {
                        //this.HudPanel.Toggle();
                        //Engine.Map.GetTown().UITownWindow.Toggle();
                        if (this.WindowTown == null)
                        {
                            this.WindowTown = new TownManagerUI(Engine.Map.GetTown()) { Location = UIManager.Mouse};// = this.HudButton.ScreenLocation + new Vector2( this.HudButton.Width,0) };// new UITownWindow(Engine.Map.GetTown()) { Location = Vector2.Zero };
                            this.WindowTown.Anchor = Vector2.One;
                        }
                        this.WindowTown.Toggle();
                        //this.WindowTown.Location = Vector2.Zero;
                    }
            };
        }
        public void InitHud(Hud hud)
        {
            this.HudPanel.Location = hud.Box_Buttons.TopRight;// this.HudButton.ScreenLocation + new Vector2(this.HudButton.Width, 0); 
            this.HudPanel.Anchor = Vector2.One;
            hud.AddButton(this.HudButton);
        }

        public void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.StockpileCreated:
                    var stockpile = e.Parameters[0] as Stockpile;
                    FloatingText.Manager.Create(() => stockpile.Positions.First(), "Stockpile created", ft => ft.Font = UIManager.FontBold);
                    break;

                case Components.Message.Types.StockpileDeleted:
                    stockpile = e.Parameters[0] as Stockpile;
                    FloatingText.Manager.Create(() => stockpile.Positions.First(), "Stockpile deleted", ft => ft.Font = UIManager.FontBold);
                    break;

                default:
                    //Engine.Map.GetTown().OnGameEvent(e);
                    break;
            }
        }
    }
}
