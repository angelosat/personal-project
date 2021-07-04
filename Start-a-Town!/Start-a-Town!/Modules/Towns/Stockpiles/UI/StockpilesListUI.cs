using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class StockpilesListUI : GroupBox
    {
        ListBox<Stockpile, Label> ListStockpiles;
        Town Town;
        public StockpilesListUI(Town town, int w, int h)
        {
            this.Town = town;
            this.ListStockpiles = new ListBox<Stockpile, Label>(w, h);
            this.Controls.Add(this.ListStockpiles);
            this.Refresh();
        }

        public new StockpilesListUI Refresh()
        {
            this.Controls.Clear();
            this.ListStockpiles.Build(this.Town.StockpileManager.Stockpiles.Values.ToList(), s => s.Name, (s, b) => b.LeftClickAction = () => OpenStockpile(s));
            this.Controls.Add(this.ListStockpiles);
            this.Invalidate();
            return this;
        }
        private void OpenStockpile(Stockpile s)
        {
            //StockpileUI.ShowWindow(s);
        }

        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.StockpileUpdated:
                    this.Refresh();
                    break;

                default:
                    break;
            }
            base.OnGameEvent(e);
        }
    }
}
