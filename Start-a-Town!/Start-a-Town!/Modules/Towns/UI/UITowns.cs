using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class TownsUI
    {
        public TownsUI()
        {
            
        }
        public void InitHud(Hud hud)
        {
            
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
                    break;
            }
        }
    }
}
