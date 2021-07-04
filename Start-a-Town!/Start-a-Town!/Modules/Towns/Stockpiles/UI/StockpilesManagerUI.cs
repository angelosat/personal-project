using System;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    [Obsolete]
    public class StockpilesManagerUI : GroupBox
    {
        StockpilesListUI StockpilesList;
        StockpileSlotsUI SlotsUI;
        IconButton BtnDesignate;
        Town Town;
        public StockpilesManagerUI(Town town)
        {
            this.Town = town;

            Panel panelButtons = new Panel() { Location = this.Controls.BottomLeft, AutoSize = true };
            this.BtnDesignate = new IconButton()
            {
                BackgroundTexture = UIManager.DefaultIconButtonSprite,
                Icon = new Icon(UIManager.Icons32, 12, 32),
                HoverFunc = () => "Designate stockpiles\n\nLeft click & drag: Add stockpile\nCtrl+Left click: Remove stockpile",
                LeftClickAction = () => ZoneNew.Edit(typeof(Stockpile))
            };
            panelButtons.Controls.Add(this.BtnDesignate);
            this.Controls.Add(panelButtons);

            PanelLabeled stockpiles = new PanelLabeled("Stockpiles") { Location = this.Controls.BottomLeft, AutoSize = true };

            this.StockpilesList = new StockpilesListUI(this.Town, 100, 150) { Location = stockpiles.Controls.BottomLeft};

            stockpiles.Controls.Add(this.StockpilesList);

            this.SlotsUI = new StockpileSlotsUI(this.Town) { Location = stockpiles.TopRight };

            this.Controls.Add(stockpiles, this.SlotsUI
                );
        }

        public new void Refresh()
        {
            this.SlotsUI.Refresh();
            this.StockpilesList.Refresh();
            this.Invalidate(true);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch (e.Type)
            {
                case Components.Message.Types.StockpileCreated:
                    this.Refresh();
                    Stockpile stockpile = e.Parameters[0] as Stockpile;
                    // do i want to immediately open the stockpile's interface upon creation?
                    FloatingText.Manager.Create(() => stockpile.Positions.First(), "Stockpile created", ft => ft.Font = UIManager.FontBold);
                    break;

                case Components.Message.Types.StockpileDeleted:
                    this.Refresh();
                    stockpile = e.Parameters[0] as Stockpile;
                    FloatingText.Manager.Create(() => stockpile.Positions.First(), "Stockpile deleted", ft => ft.Font = UIManager.FontBold);
                    break;

                default:
                    base.OnGameEvent(e);
                    break;
            }
        }
    }
}
