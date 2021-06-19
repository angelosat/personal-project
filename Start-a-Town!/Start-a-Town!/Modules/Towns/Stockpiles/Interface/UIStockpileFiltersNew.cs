using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class UIStockpileFiltersNew : Window
    {
        static UIStockpileFiltersNew _Instance;
        static UIStockpileFiltersNew Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new UIStockpileFiltersNew();
                return _Instance;
            }
        }
        Stockpile Stockpile;
        //StockpileFiltersUIAdvanced UIFilters;
        StorageUI UIFilters;
        UIStockpileFiltersNew()
        {
            this.AutoSize = true;
            this.Movable = true;
            this.Closable = true;
            //this.UIFilters = new StockpileFiltersUIAdvanced();
            this.UIFilters = new StorageUI();
            this.Client.AddControls(this.UIFilters);
        }
        static public void Refresh(Stockpile stockpile)
        {
            var isopen = Instance.IsOpen;
            if(stockpile == Instance.Stockpile && isopen)
            {
                Instance.Hide();
                return;
            }
            Instance.Stockpile = stockpile;
            Instance.Title = stockpile.Name;
            Instance.UIFilters.Refresh(stockpile);
            if(!isopen)
                Instance.Show();
        }
        internal override void OnGameEvent(GameEvent e)
        {
            switch(e.Type)
            {
                case Components.Message.Types.StockpileDeleted:
                    if (this.Stockpile == e.Parameters[0] as Stockpile)
                        this.Hide();
                    break;

                default:
                    this.UIFilters.OnGameEvent(e);
                    break;
            }
        }
    }
}
