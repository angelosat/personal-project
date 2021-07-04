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
